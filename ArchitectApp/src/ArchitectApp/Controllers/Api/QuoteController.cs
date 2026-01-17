using ArchitectApp.Data;
using ArchitectApp.Dto;
using ArchitectApp.Models;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using System.Text.Json;

namespace ArchitectApp.Controllers.Api
{
    [Route("api/[controller]")]
    [ApiController]
    public class QuoteController : ControllerBase
    {
        private readonly ArchitectDbContext _context;
        private readonly IWebHostEnvironment _environment;
        private readonly IConfiguration _configuration;
        private readonly IHttpClientFactory _httpClientFactory;

        public QuoteController(
            ArchitectDbContext context,
            IWebHostEnvironment environment,
            IConfiguration configuration,
            IHttpClientFactory httpClientFactory)
        {
            _context = context;
            _environment = environment;
            _configuration = configuration;
            _httpClientFactory = httpClientFactory;
        }

        [HttpPost]
        public async Task<IActionResult> CreateQuote([FromForm] CreateQuoteRequest dto)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            string relativePath = string.Empty;
            string filePath = string.Empty;

            if (dto.HousePlan != null && dto.HousePlan.Length > 0)
            {
                string webRootPath = _environment.WebRootPath;
                if (string.IsNullOrWhiteSpace(webRootPath))
                {
                    webRootPath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot");
                }

                string uploadsFolder = Path.Combine(webRootPath, "uploads", "houseplans");
                if (!Directory.Exists(uploadsFolder))
                    Directory.CreateDirectory(uploadsFolder);

                string uniqueFileName = Guid.NewGuid().ToString() + "_" + dto.HousePlan.FileName;
                filePath = Path.Combine(uploadsFolder, uniqueFileName);

                using (var fileStream = new FileStream(filePath, FileMode.Create))
                {
                    await dto.HousePlan.CopyToAsync(fileStream);
                }

                relativePath = "/uploads/houseplans/" + uniqueFileName;
            }

            var quoteRequest = new QuoteRequest
            {
                Name = dto.Name,
                Email = dto.Email,
                PhoneNumber = dto.PhoneNumber,
                ProjectType = dto.ProjectType,
                Location = dto.Location,
                HousePlanPath = relativePath,
                StartOfProject = dto.StartOfProject,
                EndOfProject = dto.EndOfProject,
                Description = dto.Description,
                CreatedAt = DateTime.UtcNow
            };

            _context.QuoteRequests.Add(quoteRequest);
            await _context.SaveChangesAsync();

            var responseDto = new ResponseQuoteRequest
            {
                PublicId = quoteRequest.PublicId,
                Name = quoteRequest.Name,
                Email = quoteRequest.Email,
                PhoneNumber = quoteRequest.PhoneNumber,
                ProjectType = quoteRequest.ProjectType,
                Location = quoteRequest.Location,
                HousePlanPath = quoteRequest.HousePlanPath,
                StartOfProject = quoteRequest.StartOfProject,
                EndOfProject = quoteRequest.EndOfProject,
                Description = quoteRequest.Description,
            };

            var templateBody = new
            {
                name = dto.Name,
                email = dto.Email,
                phone_number = dto.PhoneNumber,
                project_type = dto.ProjectType,
                location = dto.Location,
                description = dto.Description,
                start_date = dto.StartOfProject.ToString("yyyy-MM-dd"),
                end_date = dto.EndOfProject.ToString("yyyy-MM-dd"),
                has_attachment = dto.HousePlan != null,
                created_at = DateTime.UtcNow.ToString("yyyy-MM-dd HH:mm:ss")
            };

            try
            {
                await SendEmailAsync(
                    emails: new List<string> { dto.Email },
                    subject: "Köszönjük az árajánlatkérést!",
                    templateName: "client_confirmation",
                    templateBody: templateBody,
                    attachments: null
                );

                var contractorEmail = _configuration["CONTRACTOR_EMAIL"];
                if (!string.IsNullOrEmpty(contractorEmail))
                {
                    var attachments = new List<(string path, string fileName, string contentType)>();
                    if (!string.IsNullOrEmpty(filePath) && dto.HousePlan != null)
                    {
                        attachments.Add((filePath, dto.HousePlan.FileName, dto.HousePlan.ContentType));
                    }
    
                    await SendEmailAsync(
                        emails: new List<string> { contractorEmail },
                        subject: "Új árajánlatkérés érkezett",
                        templateName: "contractor_notification",
                        templateBody: templateBody,
                        attachments: attachments
                    );
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Email enqueue failed: {ex.Message}");
            }

            return Ok(new { message = "Árajánlat sikeresen elküldve!", responseDto });
        }

        [HttpGet]
        public async Task<IActionResult> GetAllAsync()
        {
            var quotes = await _context.QuoteRequests
                .Select(q => new ResponseQuoteRequest
                {
                    PublicId = q.PublicId,
                    Name = q.Name,
                    Email = q.Email,
                    PhoneNumber = q.PhoneNumber,
                    ProjectType = q.ProjectType,
                    HousePlanPath = q.HousePlanPath,
                    Location = q.Location,
                    StartOfProject = q.StartOfProject,
                    EndOfProject = q.EndOfProject,
                    Description = q.Description,
                })
                .ToListAsync();

            return Ok(quotes);
        }

        private async Task SendEmailAsync(
            List<string> emails,
            string subject,
            string templateName,
            object templateBody,
            List<(string path, string fileName, string contentType)>? attachments = null)
        {
            var emailServiceUrl = _configuration["EMAIL_SERVICE_URL"] ?? throw new Exception("EMAIL_SERVICE_URL is missing");
            var url = $"{emailServiceUrl}/emails";

            var apiKey = _configuration["EMAIL_SERVICE_API_KEY"]
                ?? throw new Exception("EMAIL_SERVICE_API_KEY is missing");

            var httpClient = _httpClientFactory.CreateClient("EmailService");
            httpClient.DefaultRequestHeaders.Add("X-API-KEY", apiKey);

            using var formData = new MultipartFormDataContent();

            var emailsJson = JsonSerializer.Serialize(emails);
            var templateBodyJson = JsonSerializer.Serialize(templateBody);

            formData.Add(new StringContent(emailsJson), "emails");
            formData.Add(new StringContent(subject), "subject");
            formData.Add(new StringContent(templateName), "template_name");
            formData.Add(new StringContent(templateBodyJson), "template_body");

            if (attachments != null && attachments.Any())
            {
                foreach (var (path, fileName, contentType) in attachments)
                {
                    if (!string.IsNullOrEmpty(path) && System.IO.File.Exists(path))
                    {
                        var fileBytes = await System.IO.File.ReadAllBytesAsync(path);
                        var fileContent = new ByteArrayContent(fileBytes);

                        if (!string.IsNullOrEmpty(contentType))
                        {
                            fileContent.Headers.ContentType =
                                new System.Net.Http.Headers.MediaTypeHeaderValue(contentType);
                        }

                        formData.Add(fileContent, "attachments", fileName);
                    }
                }
            }

            try
            {
                var response = await httpClient.PostAsync(url, formData);

                if (!response.IsSuccessStatusCode)
                {
                    var error = await response.Content.ReadAsStringAsync();
                    Console.WriteLine($"Email service error: {response.StatusCode} - {error}");
                    throw new Exception($"Email service error: {response.StatusCode}");
                }
                else
                {
                    var responseContent = await response.Content.ReadAsStringAsync();
                    Console.WriteLine($"Email queued successfully: {responseContent}");
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Failed to send email: {ex.Message}");
                throw;
            }
        }
    }
}