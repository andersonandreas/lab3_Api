using System.ComponentModel.DataAnnotations;

namespace lab3_Api.Models
{
    public class Link
    {
        [Required]
        public int Id { get; set; }

        [Required]
        public required string Url { get; set; }

        public int InterestId { get; set; }
        public virtual Interest Interest { get; set; }
    }
}
