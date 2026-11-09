using System;

namespace RaceDay.Mvc.ViewModels
{
    public class EventViewModel
    {
        public int EventID { get; set; }

        public string EventName { get; set; } = "";

        public string Description { get; set; } = "";

        public DateTime EventDate { get; set; }

        public int LocationID { get; set; }

        public int EventTypeID { get; set; }

        public decimal DistanceKm { get; set; }

        public string LocationName { get; set; } = "";

        public string EventTypeName { get; set; } = "";
    }
}