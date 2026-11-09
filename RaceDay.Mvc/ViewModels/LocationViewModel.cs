namespace RaceDay.Mvc.ViewModels
{
    public class LocationViewModel
    {
        public int LocationID { get; set; }

        public string LocationName { get; set; } = "";

        public string Address { get; set; } = "";

        public string City { get; set; } = "";

        public string Province { get; set; } = "";
    }
}