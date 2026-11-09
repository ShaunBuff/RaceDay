namespace RaceDay.Mvc.ViewModels
{
    public class EnrolmentResultViewModel
    {
        public int EnrolmentID { get; set; }

        public int UserID { get; set; }

        public int EventID { get; set; }

        public int CategoryID { get; set; }

        public string Status { get; set; } = "";

        public EventViewModel? Event { get; set; }

        public CategoryViewModel? Category { get; set; }
    }
}