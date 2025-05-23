﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SE.Common.Response.Professor
{
    public class GetAllProfessorReponse
    {
        public string ProfessorName {  get; set; }

        public string ProfessorAvatar { get; set; }
        public int ProfessorId {  get; set; }
        public int AccountId {  get; set; }
        public string Major {  get; set; }
        public string DateTime { get; set; }
        public string Date {  get; set; }
        public decimal Rating { get; set; }

        public int TotalRating { get; set; }
    }
}
