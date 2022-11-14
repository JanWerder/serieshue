using System.ComponentModel.DataAnnotations;
using Microsoft.EntityFrameworkCore;

namespace serieshue.Models
{
    public class Episode
    {
        public virtual Title Title { get; set; }        
        public string TitleTconst { get; set; }

        [Key]
        public string Tconst { get; set; }

        [Display(Name = "Season Number")]
        public int SeasonNumber { get; set; }

        [Display(Name = "Episode Number")]
        public int EpisodeNumber { get; set; }

        [Display(Name = "Episode Title")]
        public string? EpisodeTitle { get; set; }

        [Display(Name = "Episode Rating")]
        public double? Rating { get; set; }

        [Display(Name = "Episode Votes")]
        public int? Votes { get; set; }

        [Display(Name = "Episode Runtime")]
        public int? RuntimeMinutes { get; set; }

        [Display(Name = "Episode Genres")]
        public String? Genres { get; set; }
    }
}