namespace lab3_Api.Models.ViewModels
{
    public class PersonAllInfoViewModel
    {

        public int Id { get; set; }

        public required string FirstName { get; set; }

        public required string LastName { get; set; }

        public string? PhoneNumber { get; set; }

        public virtual List<InterestVewModel>? Interests { get; set; }
        public virtual List<LinkViewModel>? Links { get; set; }

    }
}
