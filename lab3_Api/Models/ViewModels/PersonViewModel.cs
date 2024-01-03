namespace lab3_Api.Models.ViewModels
{
    public class PersonViewModel
    {

        public required int Id { get; set; }
        public required string FirstName { get; set; }
        public required string LastName { get; set; }
        public string? PhoneNumber { get; set; }

    }
}
