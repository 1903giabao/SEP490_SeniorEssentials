using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using AutoMapper;
using Google.Cloud.Firestore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using SE.Common;
using SE.Common.DTO;
using SE.Common.Enums;
using SE.Common.Response.HealthIndicator;
using SE.Data.Models;
using SE.Data.UnitOfWork;
using SE.Service.Base;
using SE.Service.Services;
using Serilog;

namespace SE.Service.BackgroundWorkers
{
    public class Worker : BackgroundService
    {
        private readonly ILogger<Worker> _logger;
        private readonly IServiceProvider _serviceProvider;
        private readonly TimeSpan _checkInterval = TimeSpan.FromMinutes(1); // Kiểm tra mỗi phút

        public Worker(ILogger<Worker> logger, IServiceProvider serviceProvider)
        {
            _logger = logger;
            _serviceProvider = serviceProvider;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            Log.Information("Activity Notification Background Service started.");

            while (!stoppingToken.IsCancellationRequested)
            {
                try
                {
                    using (var scope = _serviceProvider.CreateScope())
                    {
                        var activityService = scope.ServiceProvider.GetRequiredService<IActivityService>();
                        var notificationService = scope.ServiceProvider.GetRequiredService<INotificationService>();
                        var firestoreDb = scope.ServiceProvider.GetRequiredService<FirestoreDb>();
                        var unitOfWork = scope.ServiceProvider.GetRequiredService<UnitOfWork>();

                        await CheckAndSendActivityNotifications(unitOfWork, activityService, notificationService, stoppingToken);
                        await CheckAndSendWaterReminders(unitOfWork, notificationService, stoppingToken);
                        await CheckAndDisableStatus(unitOfWork, firestoreDb, stoppingToken);
                        await DisableAppointment(unitOfWork, stoppingToken);

                    }
}
                catch (Exception ex)
                {
                    Log.Error(ex, "Error occurred in Activity Notification Background Service");
                }

                await Task.Delay(_checkInterval, stoppingToken);
            }

            Log.Information("Activity Notification Background Service is stopping.");
        }

        private async Task CheckAndSendActivityNotifications(UnitOfWork _unitOfWork, IActivityService _activityService, INotificationService _notificationService, CancellationToken stoppingToken)
        {
            Log.Information("Background Worker for Activity Notifications starting...");

            try
            {
                var now = DateTime.UtcNow.AddHours(7);
                var currentDate = DateOnly.FromDateTime(now);
                var currentTime = now.AddMinutes(5).ToString("HH:mm");

                var accountIds = await _unitOfWork.AccountRepository.GetAllAsync();

                foreach (var account in accountIds)
                {
                    var result = await _activityService.GetAllActivityForDay(account.AccountId, currentDate);
                    if (result is BusinessResult businessResult && businessResult.Data is List<GetScheduleInDayResponse> schedules)
                    {
                        var upcomingActivities = schedules
                            .Where(a => a.StartTime == currentTime)
                            .ToList();

                        if (account.DeviceToken != null && account.DeviceToken != "string")
                        {
                            foreach (var activity in upcomingActivities)
                            {
                                if (activity.Type == "Medication")
                                {
                                    var title ="Nhắc nhở uống thuốc";
                                    var body = $"Đã đến giờ uống thuốc {activity.Title}. Đừng quên nhé!";

                                   await _notificationService.SendNotification(account.DeviceToken, title, body);

                                    var newNoti = new Notification
                                    {
                                        Title = title,
                                        AccountId = account.AccountId,
                                        CreatedDate = DateTime.UtcNow.AddHours(7),
                                        Message = body,
                                        NotificationType = title,
                                        Status = SD.NotificationStatus.SEND,
                                        Data = currentDate.ToString("yyyy-MM-dd")
                                    };
                                    await _unitOfWork.NotificationRepository.CreateAsync(newNoti);
                                }
                                else if (activity.Type == "Activity")
                                {
                                    var title = "Lịch trình hàng ngày";
                                    var body =  $"Bạn có hoạt động '{activity.Title}' bắt đầu lúc {activity.StartTime}";

                                    await _notificationService.SendNotification(account.DeviceToken, title, body);

                                    var newNoti = new Notification
                                    {
                                        Title = title,
                                        AccountId = account.AccountId,
                                        CreatedDate = DateTime.UtcNow.AddHours(7),
                                        Message = body,
                                        NotificationType = title,
                                        Status = SD.NotificationStatus.SEND,
                                        Data = currentDate.ToString("yyyy-MM-dd")
                                    };
                                    await _unitOfWork.NotificationRepository.CreateAsync(newNoti);
                                }
                                else if (activity.Type == "Professor Appointment")    
                                {
                                    var title = "Lịch gặp bác sĩ";
                                    var body = $"Bạn có lịch hẹn với bác sĩ {activity.Description} vào lúc {activity.StartTime}";

                                    await _notificationService.SendNotification(account.DeviceToken, title, body);

                                    var newNoti = new Notification
                                    {
                                        Title = title,
                                        AccountId = account.AccountId,
                                        CreatedDate = DateTime.UtcNow.AddHours(7),
                                        Message = body,
                                        NotificationType = title,
                                        Status = SD.NotificationStatus.SEND,
                                    };
                                    await _unitOfWork.NotificationRepository.CreateAsync(newNoti);
                                }

                                   
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                Log.Error(ex, "Error while checking and sending activity notifications");
            }
        }

        private async Task CheckAndSendWaterReminders(UnitOfWork _unitOfWork, INotificationService _notificationService, CancellationToken stoppingToken)
        {
            Log.Information("Water Reminder Background Service starting...");

            var reminderSchedule = new List<WaterReminder>
            {
                new WaterReminder { Time = "06:30", Amount = "250 ml", Reason = "Sau khi thức dậy, làm sạch cơ thể" },
                new WaterReminder { Time = "08:40", Amount = "200 ml", Reason = "Trước ăn sáng, hỗ trợ tiêu hóa" },
                new WaterReminder { Time = "10:00", Amount = "200 ml", Reason = "Giữa buổi sáng, giữ tỉnh táo" },
                new WaterReminder { Time = "11:30", Amount = "200 ml", Reason = "Trước ăn trưa khoảng 30 phút" },
                new WaterReminder { Time = "14:00", Amount = "200 ml", Reason = "Sau ăn trưa, bổ sung nước nhẹ" },
                new WaterReminder { Time = "16:00", Amount = "200 ml", Reason = "Giữ nước cho cơ thể, chống mệt mỏi" },
                new WaterReminder { Time = "18:00", Amount = "200 ml", Reason = "Trước ăn tối 30 phút" },
                new WaterReminder { Time = "20:00", Amount = "150 ml", Reason = "Sau ăn tối nhẹ" },
                new WaterReminder { Time = "21:30", Amount = "100 ml (hoặc ít hơn)", Reason = "Trước khi ngủ, tránh tiểu đêm" }
            };

            try
            {
                var now = DateTime.UtcNow.AddHours(7);
                var currentTime = now.ToString("HH:mm");

                var currentReminder = reminderSchedule.FirstOrDefault(r => r.Time == currentTime);
                if (currentReminder != null)
                {
                    var accounts = _unitOfWork.AccountRepository.FindByCondition(a => a.RoleId == 2).ToList();

                    foreach (var account in accounts)
                    {
                        if (account.DeviceToken != null && account.DeviceToken != "string")
                        {
                            var title = "Nhắc nhở uống nước";
                            var body = $"Đã đến giờ uống nước! {currentReminder.Amount} {currentReminder.Reason}";

                            await _notificationService.SendNotification(account.DeviceToken, title, body);

                            var newNoti = new Notification
                            {
                                Title = title,
                                AccountId = account.AccountId,
                                CreatedDate = DateTime.UtcNow.AddHours(7),
                                Message = body,
                                NotificationType = "Nhắc nhở uống nước",
                                Status = SD.NotificationStatus.SEND
                            };
                            await _unitOfWork.NotificationRepository.CreateAsync(newNoti);
                        }
                    }

                    // Wait for 1 hour to prevent duplicate sends
                    await Task.Delay(TimeSpan.FromHours(1), stoppingToken);
                }
            }
            catch (Exception ex)
            {
                Log.Error(ex, "Error while checking and sending water reminders");
            }
        }        
        
        private async Task CheckAndDisableStatus(UnitOfWork _unitOfWork, FirestoreDb _firestoreDb, CancellationToken stoppingToken)
        {
            Log.Information("Disable status is working...");

            try
            {
                var now = DateTime.UtcNow.AddHours(7);
                var currentTime = now.ToString("HH:mm");

                var allUserSubscriptions = await _unitOfWork.UserServiceRepository.GetAllActive(SD.UserSubscriptionStatus.AVAILABLE);

                if (allUserSubscriptions.Any())
                {
                    foreach (var subscription in allUserSubscriptions)
                    {
                        var endTime = subscription.EndDate;

                        if (now > endTime)
                        {
                            subscription.Status = SD.UserSubscriptionStatus.EXPIRED;
                            await _unitOfWork.UserServiceRepository.UpdateAsync(subscription);

                            var groupRef = _firestoreDb.Collection("ChatRooms").Document(subscription.ProfessorGroupChatId);
                            var groupDoc = await groupRef.GetSnapshotAsync();

                            if (groupDoc.Exists)
                            {
                                var updateData = new Dictionary<string, object>
                                {
                                    { "IsDisabled", true }
                                };

                                await groupRef.UpdateAsync(updateData);
                            }
                        }                        
                    }
                }
            }
            catch (Exception ex)
            {
                Log.Error(ex, "Error while checking and disable status");
            }
        }

        private async Task DisableAppointment(UnitOfWork _unitOfWork, CancellationToken stoppingToken)
        {
            Log.Information("Activity Disable Appointment Background Service started.");
            try
            {
                var now = DateTime.UtcNow.AddHours(7);
                var currentTime = now.ToString("HH:mm");
                var appointments = _unitOfWork.ProfessorAppointmentRepository.FindByCondition(a=>a.Status == SD.ProfessorAppointmentStatus.NOTYET && a.EndTime.ToString() == currentTime).ToList();
                foreach (var appointment in appointments)
                {
                     appointment.Status = SD.ProfessorAppointmentStatus.OUTOFTIME;
                       await _unitOfWork.ProfessorAppointmentRepository.UpdateAsync(appointment);
                   
                }
                await Task.Delay(TimeSpan.FromHours(1), stoppingToken);

            }
            catch (Exception ex)
            {
                Log.Error(ex, "Error while checking and sending water reminders");
            }
        }
    }
}
