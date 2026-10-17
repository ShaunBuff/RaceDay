namespace RaceDay.Api.Models
{
    public class Category
    {
        public int CategoryID { get; set; }

        public int EventID { get; set; }

        public string CategoryName { get; set; } = string.Empty;

        public decimal DistanceKm { get; set; }
    }
}