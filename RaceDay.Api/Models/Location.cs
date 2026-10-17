namespace RaceDay.Api.Models
{
    public class Location
    {
        public int LocationID { get; set; }

        public string LocationName { get; set; } = string.Empty;

        public string? Address { get; set; }

        public string City { get; set; } = string.Empty;

        public string Province { get; set; } = string.Empty;
    }
}