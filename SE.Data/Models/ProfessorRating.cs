﻿// <auto-generated> This file has been auto generated by EF Core Power Tools. </auto-generated>
#nullable disable
using System;
using System.Collections.Generic;

namespace SE.Data.Models;

public partial class ProfessorRating
{
    public int ProfessorRatingId { get; set; }

    public int ProfessorId { get; set; }

    public int ElderlyId { get; set; }

    public string RatingComment { get; set; }

    public decimal Rating { get; set; }

    public string Status { get; set; }

    public string CreatedBy { get; set; }

    public int? ProfessorAppointmentId { get; set; }

    public virtual Elderly Elderly { get; set; }

    public virtual Professor Professor { get; set; }

    public virtual ProfessorAppointment ProfessorAppointment { get; set; }
}