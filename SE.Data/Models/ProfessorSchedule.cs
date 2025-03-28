﻿// <auto-generated> This file has been auto generated by EF Core Power Tools. </auto-generated>
#nullable disable
using System;
using System.Collections.Generic;

namespace SE.Data.Models;

public partial class ProfessorSchedule
{
    public int ProfessorScheduleId { get; set; }

    public int ProfessorId { get; set; }

    public string DayOfWeek { get; set; }

    public DateTime? StartDate { get; set; }

    public DateTime? EndDate { get; set; }

    public string Status { get; set; }

    public virtual Professor Professor { get; set; }

    public virtual ICollection<TimeSlot> TimeSlots { get; set; } = new List<TimeSlot>();
}