﻿// <auto-generated> This file has been auto generated by EF Core Power Tools. </auto-generated>
#nullable disable
using System;
using System.Collections.Generic;

namespace SE.Data.Models;

public partial class LessonHistory
{
    public int LessonHistoryId { get; set; }

    public int LessonId { get; set; }

    public int AccountId { get; set; }

    public DateTime? StartTime { get; set; }

    public bool IsCompleted { get; set; }

    public string Status { get; set; }

    public virtual Account Account { get; set; }

    public virtual Lesson Lesson { get; set; }
}