﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SE.Common.Request.Emergency
{
    public class CreateEmergencyConfirmationRequest
    {
        public int ElderlyId { get; set; }
        public int FamilyMemberId { get; set; }
    }
}
