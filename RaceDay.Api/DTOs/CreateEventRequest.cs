
using System;
using System.ComponentModel.DataAnnotations;

namespace RaceDay.Api.DTOs;

public class CreateEventRequest
{
    [Required]
    [StringLength(150, MinimumLength = 3)]
    public string EventName { get; set; } = string.Empty;

    [StringLength(1000)]
    public string? Description { get; set; }

    [Range(1, int.MaxValue)]
    public int EventTypeID { get; set; }

    [Range(1, int.MaxValue)]
    public int LocationID { get; set; }

    [Required]
    public DateTime? EventDate { get; set; }

    [Range(typeof(decimal), "0.01", "1000")]
    public decimal DistanceKm { get; set; }
}
