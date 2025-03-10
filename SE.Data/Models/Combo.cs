﻿// <auto-generated> This file has been auto generated by EF Core Power Tools. </auto-generated>
#nullable disable
using System;
using System.Collections.Generic;

namespace SE.Data.Models;

public partial class Combo
{
    public int ComboId { get; set; }

    public string Name { get; set; }

    public string Description { get; set; }

    public decimal Fee { get; set; }

    public DateTime ValidityPeriod { get; set; }

    public int NumberOfMeeting { get; set; }

    public int DurationPerMeeting { get; set; }

    public DateTime CreatedDate { get; set; }

    public DateTime UpdatedDate { get; set; }

    public string Status { get; set; }

    public virtual ICollection<Booking> Bookings { get; set; } = new List<Booking>();
}