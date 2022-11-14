using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using serieshue.Models;
using Microsoft.EntityFrameworkCore;
using serieshue.Services;
using serieshue.Interfaces;
using Microsoft.AspNetCore.WebUtilities;
using Microsoft.Net.Http.Headers;
using Hangfire.Storage.Monitoring;

namespace marina_backend.Controllers
{
    [ApiController]
    public class SeriesHueController : ControllerBase
    {
        private readonly SeriesHueContext _context;

        public SeriesHueController(SeriesHueContext context)
        {
            _context = context;
        }

        [HttpGet("/api/search")]
        public ActionResult<IEnumerable<TitleDTO>> Search([FromQuery] string query)
        {
            var titles = _context.Titles
            .Where(t => EF.Functions.TrigramsWordSimilarity(t.PrimaryTitle, query) > 0.8)
            .OrderByDescending(t => t.StartYear)
            .Take(15)
            .ToList();
            var titlesDTO = titles.Select(t => new TitleDTO(t)).ToList();
            return titlesDTO;
        }

        [HttpGet("/api/lastUpdated")]
        public ActionResult<DateTime> LastUpdated()
        {
            var MonitoringApi = Hangfire.JobStorage.Current.GetMonitoringApi();
            KeyValuePair<string, SucceededJobDto> lastJob = MonitoringApi.SucceededJobs(0, 1).FirstOrDefault();
            if (!lastJob.Equals(default(KeyValuePair<string, SucceededJobDto>)))
            {
                return lastJob.Value.SucceededAt;
            }
            else
            {
                return DateTime.MinValue;
            }

        }
    }

    public class TitleDTO
    {
        public string Tcode { get; set; }
        public string Title { get; set; }

        public TitleDTO(Title title)
        {
            Tcode = title.Tconst;

            if (title.StartYear != null)
            {
                Title = title.PrimaryTitle + " (" + title.StartYear + ")";
            }
            else
            {
                Title = title.PrimaryTitle;
            }
        }
    }
}