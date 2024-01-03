using System.ComponentModel.DataAnnotations;

namespace lab3_Api.Dtos
{
    public class LinkDto
    {

        [Required]
        public required string Url { get; set; }

    }
}
