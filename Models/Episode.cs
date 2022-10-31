using System.ComponentModel.DataAnnotations;
using Microsoft.EntityFrameworkCore;

namespace serieshue.Models
{
    public class Episode
    {
        public virtual Title Title { get; set; }

        [Key]
        public string TConst { get; set; }

        [Display(Name = "Season Number")]
        public int SeasonNumber { get; set; }

        [Display(Name = "Episode Number")]
        public int EpisodeNumber { get; set; }

        [Display(Name = "Episode Title")]
        public string EpisodeTitle { get; set; }

        [Display(Name = "Episode Rating")]
        public double? Rating { get; set; }

        [Display(Name = "Episode Votes")]
        public int? Votes { get; set; }

        [Display(Name = "Episode Runtime")]
        public int? RuntimeMinutes { get; set; }

        [Display(Name = "Episode Genres")]
        public string Genres { get; set; }
    }
}