using System;

namespace RaceDay.Api.Models
{
    public class Event
    {
        public int EventID { get; set; }

        public int OrganiserID { get; set; }

        public int EventTypeID { get; set; }

        public int LocationID { get; set; }

        public string EventName { get; set; } = string.Empty;

        public string? Description { get; set; }

        public DateTime EventDate { get; set; }

        public decimal DistanceKm { get; set; }

        public DateTime CreatedAt { get; set; }
    }
}