namespace lab3_Api.Models
{
    public class InterestRelation
    {
        public int PersonId { get; set; }
        public int InterestId { get; set; }

        public virtual Person Person { get; set; }
        public virtual Interest Interest { get; set; }
    }
}
