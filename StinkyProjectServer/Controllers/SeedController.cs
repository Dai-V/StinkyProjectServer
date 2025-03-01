using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using StinkyModel;

namespace StinkyProjectServer.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class SeedController(WorldcitiessourceContext context) : ControllerBase
    {
        private readonly WorldcitiessourceContext _context = context;
        [HttpPost("Country")]
        public async Task<ActionResult> ImportCountryAsync (Country country)
        {
            _context.Countries.Add(country);
            await _context.SaveChangesAsync();

            return CreatedAtAction("GetCity", new { id = city.Id }, city);
        }
    }
}
