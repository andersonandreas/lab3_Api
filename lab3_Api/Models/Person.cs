using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace lab3_Api.Models
{
    [Table("Persons")]
    public class Person
    {
        [Key]
        public int Id { get; set; }

        [Required]
        [MaxLength(100)]
        public required string FirstName { get; set; }

        [Required]
        [MaxLength(100)]
        public required string LastName { get; set; }

        public string? PhoneNumber { get; set; }

        public virtual List<InterestRelation>? InterestRelation { get; set; }
    }
}
