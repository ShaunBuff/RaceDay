namespace RaceDay.Mvc.ViewModels
{
    public class CategoryViewModel
    {
        public int CategoryID { get; set; }

        public int EventID { get; set; }

        public string CategoryName { get; set; } = "";

        public decimal DistanceKm { get; set; }
    }
}