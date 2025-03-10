﻿// <auto-generated> This file has been auto generated by EF Core Power Tools. </auto-generated>
#nullable disable
using System;
using System.Collections.Generic;

namespace SE.Data.Models;

public partial class TimeSlot
{
    public int TimeSlotId { get; set; }

    public int ProfessorScheduleId { get; set; }

    public TimeOnly? StartTime { get; set; }

    public TimeOnly? EndTime { get; set; }

    public string Note { get; set; }

    public string Status { get; set; }

    public virtual ICollection<ProfessorAppointment> ProfessorAppointments { get; set; } = new List<ProfessorAppointment>();

    public virtual ProfessorSchedule ProfessorSchedule { get; set; }
}