﻿// <auto-generated> This file has been auto generated by EF Core Power Tools. </auto-generated>
#nullable disable
using System;
using System.Collections.Generic;

namespace SE.Data.Models;

public partial class UserSubscription
{
    public int UserSubscriptionId { get; set; }

    public int? BookingId { get; set; }

    public int? ProfessorId { get; set; }

    public DateTime? StartDate { get; set; }

    public DateTime? EndDate { get; set; }

    public string Status { get; set; }

    public int? NumberOfMeetingLeft { get; set; }

    public string ProfessorGroupChatId { get; set; }

    public virtual Booking Booking { get; set; }

    public virtual Professor Professor { get; set; }

    public virtual ICollection<ProfessorAppointment> ProfessorAppointments { get; set; } = new List<ProfessorAppointment>();
}