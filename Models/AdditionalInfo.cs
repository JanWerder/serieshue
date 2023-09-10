using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Mvc.ModelBinding.Validation;
using NpgsqlTypes;

namespace serieshue.Models
{
    public class AdditionalInfo
    {
        [Key]
        public string Id { get; set; } = Guid.NewGuid().ToString();

        public Title Title { get; set; }

        public string ImageURL { get; set; }

        public string Country { get; set; }

        public string Plot { get; set; }
    }
}