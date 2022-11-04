using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Mvc.ModelBinding.Validation;
using NpgsqlTypes;

namespace serieshue.Models
{
    public class Title
    {
        [Key]
        public string Tconst { get; set; }

        [Display(Name = "Popular Title")]
        public String? PrimaryTitle  { get; set; }

        [Display(Name = "Original Title")]
        public String? OriginalTitle  { get; set; }

        [Display(Name = "is Adult?")]
        public Boolean? IsAdult { get; set; }

        [Display(Name = "Start Year")]
        public Int32? StartYear { get; set; }

        [Display(Name = "End Year")]
        public Int32? EndYear { get; set; }

        [Display(Name = "Runtime Minutes")]
        public Int32? RuntimeMinutes { get; set; }

        [Display(Name = "Genres")]
        public String? Genres { get; set; }

        [Display(Name = "Title Rating")]
        public double? Rating { get; set; }

        [Display(Name = "Title Votes")]
        public int? Votes { get; set; }

        [ValidateNever]
        [Display(Name = "Episodes")]
        public virtual IList<Episode> Episodes { get; set; }        

        public NpgsqlTsVector SearchVector { get; set; }
    }
}