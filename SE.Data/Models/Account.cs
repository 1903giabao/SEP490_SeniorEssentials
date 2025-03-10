﻿// <auto-generated> This file has been auto generated by EF Core Power Tools. </auto-generated>
#nullable disable
using System;
using System.Collections.Generic;

namespace SE.Data.Models;

public partial class Account
{
    public int AccountId { get; set; }

    public int RoleId { get; set; }

    public string Email { get; set; }

    public string Password { get; set; }

    public string FullName { get; set; }

    public string Avatar { get; set; }

    public string Gender { get; set; }

    public string PhoneNumber { get; set; }

    public DateTime? DateOfBirth { get; set; }

    public DateTime? CreatedDate { get; set; }

    public string Status { get; set; }

    public string Otp { get; set; }

    public bool? IsVerified { get; set; }

    public string DeviceToken { get; set; }

    public virtual ContentProvider ContentProvider { get; set; }

    public virtual Elderly Elderly { get; set; }

    public virtual FamilyMember FamilyMember { get; set; }

    public virtual ICollection<GroupMember> GroupMembers { get; set; } = new List<GroupMember>();

    public virtual ICollection<Notification> Notifications { get; set; } = new List<Notification>();

    public virtual Professor Professor { get; set; }

    public virtual Role Role { get; set; }

    public virtual ICollection<Transaction> Transactions { get; set; } = new List<Transaction>();

    public virtual ICollection<UserLink> UserLinkAccountId1Navigations { get; set; } = new List<UserLink>();

    public virtual ICollection<UserLink> UserLinkAccountId2Navigations { get; set; } = new List<UserLink>();
}