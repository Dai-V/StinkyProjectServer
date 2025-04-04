﻿using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Threading.Tasks;
using CsvHelper;
using CsvHelper.Configuration;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using StinkyModel;
using StinkyProjectServer.Data;

namespace StinkyProjectServer.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class SeedController(WorldcitiessourceContext context, IHostEnvironment environment,
    UserManager<StinkyUser> userManager) : ControllerBase
    {
        string _pathName = Path.Combine(environment.ContentRootPath,"Data/worldcities.csv");

        [HttpPost("User")]
        public async Task ImportUserAsync()
        {   

            StinkyUser user = new()
            {
                UserName = "user",
                Email = "user@gmail.com",
                SecurityStamp = Guid.NewGuid().ToString(),
            };
            IdentityResult x = await userManager.CreateAsync(user,"Password123!");

            int y = await context.SaveChangesAsync();
            

        }

        [HttpPost("Country")]
        public async Task<ActionResult> ImportCountryAsync ()
        {
            // create a lookup dictionary containing all the countries already existing 
            // into the Database (it will be empty on first run).
            Dictionary<string, Country> countriesByName = context.Countries
                .AsNoTracking().ToDictionary(x => x.Name, StringComparer.OrdinalIgnoreCase);

            CsvConfiguration config = new(CultureInfo.InvariantCulture)
            {
                HasHeaderRecord = true,
                HeaderValidated = null
            };

            using StreamReader reader = new(_pathName);
            using CsvReader csv = new(reader, config);

            List<WorldCitiesDTO> records = csv.GetRecords<WorldCitiesDTO>().ToList();
            foreach (WorldCitiesDTO record in records)
            {
                if (countriesByName.ContainsKey(record.country))
                {
                    continue;
                }

                Country country = new()
                {
                    Name = record.country,
                    Iso2 = record.iso2,
                    Iso3 = record.iso3
                };
                await context.Countries.AddAsync(country);
                countriesByName.Add(record.country, country);
            }

            await context.SaveChangesAsync();

            return new JsonResult(countriesByName.Count);
        }

        [HttpPost("City")]
        public async Task<ActionResult> ImportCityAsync()
        {
            Dictionary<string, Country> countries = await context.Countries//.AsNoTracking()
            .ToDictionaryAsync(c => c.Name);

            CsvConfiguration config = new(CultureInfo.InvariantCulture)
            {
                HasHeaderRecord = true,
                HeaderValidated = null
            };
            int cityCount = 0;
            using (StreamReader reader = new(_pathName))
            using (CsvReader csv = new(reader, config))
            {
                IEnumerable<WorldCitiesDTO>? records = csv.GetRecords<WorldCitiesDTO>();
                foreach (WorldCitiesDTO record in records)
                {
                    if (!countries.TryGetValue(record.country, out Country? value))
                    {
                        Console.WriteLine($"Not found country for {record.city}");
                        return NotFound(record);
                    }

                    if (!record.population.HasValue || string.IsNullOrEmpty(record.city_ascii))
                    {
                        Console.WriteLine($"Skipping {record.city}");
                        continue;
                    }
                    City city = new()
                    {
                        Name = record.city,
                        Lat = record.lat,
                        Lon = record.lng,
                        Population = (int)record.population.Value,
                        Countryid = value.Id
                    };
                    context.Cities.Add(city);
                    cityCount++;
                }
                await context.SaveChangesAsync();
            }
            return new JsonResult(cityCount);
        }
    }
}
