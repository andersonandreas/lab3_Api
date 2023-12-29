using System.ComponentModel.DataAnnotations;

namespace lab3_Api.Models
{
    public class Interest
    {
        [Key]
        public int Id { get; set; }

        [Required]
        [MaxLength(50)]
        public required string Title { get; set; }

        [Required]
        [MaxLength(200)]
        public required string Description { get; set; }

        public virtual List<InterestRelation> InterestRelation { get; set; }

        public virtual List<Link> Links { get; set; }
    }
}
