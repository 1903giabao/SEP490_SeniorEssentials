using AutoMapper;
using SE.Common.DTO.Content;
using SE.Common;
using SE.Data.UnitOfWork;
using SE.Service.Base;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SE.Common.Enums;
using SE.Common.Response.Dashboard;

namespace SE.Service.Services
{
    public interface IDashboardService
    {
        Task<IBusinessResult> AdminDashboard();
    }
    public class DashboardService : IDashboardService
    {
        private readonly UnitOfWork _unitOfWork;
        private readonly IMapper _mapper;

        public DashboardService(UnitOfWork unitOfWork, IMapper mapper)
        {
            _unitOfWork = unitOfWork;
            _mapper = mapper;
        }

        private static readonly Dictionary<int, string> VietnameseMonths = new()
        {
            {1, "Tháng 1"}, {2, "Tháng 2"}, {3, "Tháng 3"}, {4, "Tháng 4"},
            {5, "Tháng 5"}, {6, "Tháng 6"}, {7, "Tháng 7"}, {8, "Tháng 8"},
            {9, "Tháng 9"}, {10, "Tháng 10"}, {11, "Tháng 11"}, {12, "Tháng 12"}
        };

        public async Task<IBusinessResult> AdminDashboard()
        {
            try
            {
                var totalUsers = _unitOfWork.AccountRepository.FindByCondition(u => u.Status.Equals(SD.GeneralStatus.ACTIVE) && u.FullName!= null).Count();

                var totalDoctor = _unitOfWork.ProfessorRepository.FindByCondition(u => u.Status.Equals(SD.GeneralStatus.ACTIVE)).Count();

                var totalContentProvider = _unitOfWork.ContentProviderRepository.FindByCondition(u => u.Status.Equals(SD.GeneralStatus.ACTIVE)).Count();

                var totalFamilyMember = _unitOfWork.FamilyMemberRepository.FindByCondition(u => u.Status.Equals(SD.GeneralStatus.ACTIVE)).Count();

                var totalElderly = _unitOfWork.ElderlyRepository.FindByCondition(u => u.Status.Equals(SD.GeneralStatus.ACTIVE)).Count();

                var totalEmergency = _unitOfWork.EmergencyConfirmationRepository.FindByCondition(u => u.Status != "Đã hủy").Count();

                var totalAppointment = _unitOfWork.ProfessorAppointmentRepository.FindByCondition(u => u.Status.Equals(SD.ProfessorAppointmentStatus.JOINED)).Count();

                var totalRevenue = _unitOfWork.BookingRepository.FindByCondition(u => u.Status.Equals(SD.BookingStatus.PAID)).Select(b => b.Price).Sum();

                var totalSubscription = _unitOfWork.SubscriptionRepository.FindByCondition(u => u.Status.Equals(SD.GeneralStatus.ACTIVE)).Count();

                var totalUserSubscription = await _unitOfWork.UserServiceRepository.TotalElderlyUseSub();
                var currentDate = DateTime.UtcNow.AddHours(7);

                var monthlyRevenues = _unitOfWork.BookingRepository
                    .FindByCondition(b =>
                        b.Status.Equals(SD.BookingStatus.PAID) &&
                        b.BookingDate <= currentDate) 
                    .AsEnumerable() 
                    .GroupBy(b => new
                    {
                        Month = b.BookingDate.Month,
                        Year = b.BookingDate.Year,
                    })
                    .Select(g => new MonthlyValue
                    {
                        MonthValue = g.Key.Month,
                        YearValue = g.Key.Year,
                        Value = (double)Math.Round(g.Sum(b => b.Price), 2),
                        Revenue = (double)g.Sum(b => b.Price),
                        Month = VietnameseMonths[g.Key.Month],
                    })
                    .OrderBy(m => m.YearValue)
                    .ThenBy(m => m.MonthValue)
                    .ToList();

                var allMonths = Enumerable.Range(0, 7)
                    .Select(i => currentDate.AddMonths(-i))
                    .Select(d => new
                    {
                        MonthValue = d.Month,
                        YearValue = d.Year,
                    });

                var result12 = allMonths
                        .GroupJoin(monthlyRevenues,
                            month => new { month.MonthValue, month.YearValue },
                            revenue => new { revenue.MonthValue, revenue.YearValue},
                            (month, revenues) => new
                            {
                                month.MonthValue,
                                month.YearValue,
                                Revenue = revenues.FirstOrDefault()?.Revenue ?? 0d
                            })
                        .Select(x => new MonthlyValue
                        {
                            MonthValue = x.MonthValue,
                            YearValue = x.YearValue,
                            Value = (double)Math.Round(x.Revenue, 2),
                            Revenue = (double)x.Revenue,
                            Month = VietnameseMonths[x.MonthValue],
                        })
                        .OrderBy(m => m.YearValue)
                        .ThenBy(m => m.MonthValue)
                        .ToList();

                var monthlyRevenueResult = allMonths
                    .GroupJoin(monthlyRevenues,
                        date => new { date.MonthValue },
                        revenue => new { revenue.MonthValue },
                        (date, revenueGroup) => new MonthlyValue
                        {
                            MonthValue = date.MonthValue,
                            YearValue = date.YearValue,
                            Month = VietnameseMonths[date.MonthValue],
                            Value = revenueGroup.FirstOrDefault()?.Value ?? 0.0
                        })
                    .OrderBy(m => m.YearValue)
                    .ThenBy(m => m.MonthValue)
                    .ToList();

                var monthlyGrowth = result12
                    .Select((month, index) => new MonthlyValue
                    {
                        Month = month.Month,
                        Value = index == 0
                            ? 0
                            : (result12[index - 1].Revenue == 0)
                                ? 0 // Handle division by zero by returning 100
                                : (month.Revenue - result12[index - 1].Revenue) /
                                  result12[index - 1].Revenue * 100
                    })
                    .ToList();

                monthlyGrowth.ForEach(x => x.Value = Math.Round(x.Value, 2));

                var allSubscriptions = await _unitOfWork.SubscriptionRepository.GetAllSubscriptions();

                var boughtPackages = allSubscriptions
                                        .Select(s => new BoughtPackage
                                        {
                                            PackageName = s.Name,
                                            PackagePrice = (double)s.Fee,
                                            BoughtQuantity = s.Bookings.Count(b => b.Status == SD.BookingStatus.PAID)
                                        })
                                        .OrderByDescending(p => p.BoughtQuantity) 
                                        .ToList();

                var rs = new AdminDashboardResponse
                {
                    TotalUsers = totalUsers,
                    TotalDoctor = totalDoctor,
                    TotalContentProvider = totalContentProvider,
                    TotalElderly = totalElderly,
                    TotalFamilyMember = totalFamilyMember,
                    Appointments = totalAppointment,
                    EmergencyAlerts = totalEmergency,
                    Subscriptions = totalSubscription,
                    Revenue = (double)totalRevenue,
                    RevenueByMonth = monthlyRevenueResult,
                    MonthlyGrowth = monthlyGrowth,
                    BoughtPackages = boughtPackages,
                    UserUseSubs = totalUserSubscription,
                };

                return new BusinessResult(Const.SUCCESS_READ, Const.SUCCESS_READ_MSG, rs);
            }
            catch (Exception ex)
            {
                return new BusinessResult(Const.FAIL_CREATE, "An unexpected error occurred: " + ex.Message);
            }
        }
    }
}
