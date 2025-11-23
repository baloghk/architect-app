using ArchitectApp.Data;
using ArchitectApp.Models;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace ArchitectApp.Controllers.Api
{
    [Route("api/[controller]")]
    [ApiController]
    public class QuoteController : ControllerBase
    {
        private readonly ArchitectDbContext _context;

        public QuoteController(ArchitectDbContext context)
        {
            _context = context;
        }

        [HttpPost]
        public async Task<IActionResult> CreateQuote([FromBody] QuoteRequest request)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            request.CreatedAt = DateTime.UtcNow;

            _context.QuoteRequests.Add(request);
            await _context.SaveChangesAsync();

            return Ok(new { message = "Árajánlat sikeresen elküldve!", id = request.Id });

            //TODO: email to admin
        }

        [HttpGet]
        public IActionResult GetAll()
        {
            return Ok(_context.QuoteRequests.ToList());
        }
    }
}
