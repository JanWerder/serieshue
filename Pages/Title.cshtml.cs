#nullable disable
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using serieshue.Models;

namespace serieshue
{
    public class IndexModel : PageModel
    {
        private readonly serieshue.Models.SeriesHueContext _context;

        public IndexModel(serieshue.Models.SeriesHueContext context)
        {
            _context = context;
        }

        public Title Title { get; set; }

        public AdditionalInfo AdditionalInfo { get; set; }

        public int EpisodeCount { get; set; }

        public int SeasonCount { get; set; }

        public IActionResult OnGet(string Tcode)
        {
            Title = _context.Titles
                .Include(t => t.Episodes)
                .Where(t => t.Tconst == Tcode)
                .FirstOrDefault();

            if (Title == null)
            {
                return new RedirectToPageResult("/Error");
            }

            AdditionalInfo = _context.AdditionalInfos
                .Where(a => a.Title.Tconst == Tcode)
                .FirstOrDefault();

            Title.Episodes = Title.Episodes.OrderBy(e => e.SeasonNumber).ThenBy(e => e.EpisodeNumber).ToList();

            SeasonCount = Title.Episodes.Select(e => e.SeasonNumber).Distinct().Count();
            EpisodeCount = Title.Episodes.Select(e => e.EpisodeNumber).Distinct().Count();

            if (Title.StartYear != null)
            {
                @ViewData["Title"] = " - " + Title.PrimaryTitle + " (" + Title.StartYear + ")";
            }
            else
            {
                @ViewData["Title"] = " - " + Title.PrimaryTitle;
            }

            return Page();
        }
    }
}