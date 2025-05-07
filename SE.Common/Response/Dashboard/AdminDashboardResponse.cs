using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace SE.Common.Response.Dashboard
{
    public class AdminDashboardResponse
    {
        public int TotalUsers { get; set; }
        public int TotalDoctor { get; set; }
        public int TotalContentProvider { get; set; }
        public int TotalFamilyMember { get; set; }
        public int TotalElderly { get; set; }
        public int Appointments { get; set; }
        public int EmergencyAlerts { get; set; }
        public double Revenue { get; set; }
        public int Subscriptions { get; set; }
        public int UserUseSubs { get; set; }
        public List<MonthlyValue> MonthlyGrowth { get; set; }
        public List<BoughtPackage> BoughtPackages { get; set; }
        public List<MonthlyValue> RevenueByMonth { get; set; }
    }

    public class MonthlyValue
    {
        [JsonIgnore] 
        public int MonthValue {  get; set; }        
        [JsonIgnore] 
        public int YearValue {  get; set; }
        public string Month { get; set; }
        public double Value { get; set; }

        [JsonIgnore] 
        public double Revenue { get; set; }
    }

    public class BoughtPackage
    {
        public string PackageName { get; set; }
        public double PackagePrice { get; set; }
        public int BoughtQuantity { get; set; }
    }
}
