﻿// <auto-generated> This file has been auto generated by EF Core Power Tools. </auto-generated>
#nullable disable
using System;
using System.Collections.Generic;

namespace SE.Data.Models;

public partial class UserLink
{
    public int UserLinkId { get; set; }

    public int AccountId1 { get; set; }

    public int AccountId2 { get; set; }

    public string RelationshipType { get; set; }

    public DateTime? CreatedAt { get; set; }

    public string Status { get; set; }

    public DateTime? UpdatedAt { get; set; }

    public virtual Account AccountId1Navigation { get; set; }

    public virtual Account AccountId2Navigation { get; set; }
}