/*using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using AutoMapper;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
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
        private readonly UnitOfWork _unitOfWork;
        private readonly IActivityService _activityService;
        private readonly INotificationService _notificationService;
        private readonly TimeSpan _checkInterval = TimeSpan.FromMinutes(1); // Kiểm tra mỗi phút

        public Worker(ILogger<Worker> logger, UnitOfWork unitOfWork, IActivityService activityService, INotificationService notificationService)
        {
            _logger = logger;
            _unitOfWork = unitOfWork;
            _activityService = activityService;
            _notificationService = notificationService;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            Log.Information("Activity Notification Background Service started.");

            while (!stoppingToken.IsCancellationRequested)
            {
                try
                {
                    await CheckAndSendActivityNotifications(stoppingToken);
                    await CheckAndSendWaterReminders(stoppingToken);

                }
                catch (Exception ex)
                {
                    Log.Error(ex, "Error occurred in Activity Notification Background Service");
                }

                await Task.Delay(_checkInterval, stoppingToken);
            }

            Log.Information("Activity Notification Background Service is stopping.");
        }

        protected async Task CheckAndSendActivityNotifications(CancellationToken stoppingToken)
        {
            Log.Information("Background Worker for Activity Notifications starting...");

            try
            {
                while (!stoppingToken.IsCancellationRequested)
                {
                    try
                    {
                        var now = DateTime.Now;
                        var currentDate = DateOnly.FromDateTime(now);
                        var currentTime = now.ToString("HH:mm");

                        // Giả sử bạn có cách lấy tất cả accountId cần kiểm tra
                        var accountIds = await _unitOfWork.AccountRepository.GetAllAsync(); 

                        foreach (var account in accountIds)
                        {
                            var result = await _activityService.GetAllActivityForDay(account.AccountId, currentDate);
                            if (result is BusinessResult businessResult && businessResult.Data is List<GetScheduleInDayResponse> schedules)
                            {
                                var upcomingActivities = schedules
                                    .Where(a => a.StartTime == currentTime)
                                    .ToList();
                                if (account.DeviceToken != null&& account.DeviceToken != "string")
                                {
                                    foreach (var activity in upcomingActivities)
                                    {
                                        if (!string.IsNullOrEmpty(account.DeviceToken))
                                        {
                                            if (activity.Type == "Medication")
                                            {
                                                var title = $"Nhắc nhở uống thuốc";
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
                                                var rs = await _unitOfWork.NotificationRepository.CreateAsync(newNoti);
                                            }
                                            else
                                            {

                                                var title = $"Lịch trình hàng ngày";
                                                var body = $"Bạn có hoạt động '{activity.Title}' bắt đầu lúc {activity.StartTime}";

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
                                                var rs = await _unitOfWork.NotificationRepository.CreateAsync(newNoti);
                                            }

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

                    await Task.Delay(_checkInterval, stoppingToken);
                }
            }
            catch (Exception ex) when (stoppingToken.IsCancellationRequested)
            {
                Log.Information("Background Worker is stopping due to cancellation request");
            }
            catch (Exception ex)
            {
                Log.Error(ex, "An error occurred in the background worker.");
            }
        }


        protected async Task CheckAndSendWaterReminders(CancellationToken stoppingToken)
        {
            Log.Information("Water Reminder Background Service starting...");

            // Define the reminder schedule
            var reminderSchedule = new List<WaterReminder>
                                            {
                                                new WaterReminder { Time = "07:00", Amount = "250 ml", Reason = "Sau khi thức dậy, thanh lọc cơ thể" },
                                                new WaterReminder { Time = "10:00", Amount = "250 ml", Reason = "Giữ tỉnh táo, tránh khô người" },
                                                new WaterReminder { Time = "14:00", Amount = "250 ml", Reason = "Bổ sung sau ăn trưa, tránh mệt mỏi" },
                                                new WaterReminder { Time = "18:00", Amount = "250 ml", Reason = "Hỗ trợ tiêu hóa trước bữa tối" }
                                            };

            try
            {
                while (!stoppingToken.IsCancellationRequested)
                {
                    try
                    {
                        var now = DateTime.Now;
                        var currentTime = now.ToString("HH:mm");
                        var currentDate = DateOnly.FromDateTime(now);

                        // Check if current time matches any reminder time
                        var currentReminder = reminderSchedule.FirstOrDefault(r => r.Time == currentTime);
                        if (currentReminder != null)
                        {
                            // Get all accounts with roleId = 2
                            var accounts = _unitOfWork.AccountRepository.FindByCondition(a=>a.RoleId ==2).ToList();

                            foreach (var account in accounts)
                            {
                                if (account.DeviceToken != null && account.DeviceToken != "string")
                                {
                                    var title = "Nhắc nhở uống nước";
                                    var body = $"Đã đến giờ uống nước! {currentReminder.Amount} {currentReminder.Reason}";

                                    await _notificationService.SendNotification(account.DeviceToken, title, body);

                                    // Save notification to database
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

                            // Wait for 1 minute to avoid sending duplicate notifications
                            await Task.Delay(TimeSpan.FromHours(1), stoppingToken);
                        }
                    }
                    catch (Exception ex)
                    {
                        Log.Error(ex, "Error while checking and sending water reminders");
                    }

                    // Check every minute
                    await Task.Delay(TimeSpan.FromMinutes(1), stoppingToken);
                }
            }
            catch (Exception ex) when (stoppingToken.IsCancellationRequested)
            {
                Log.Information("Water Reminder Background Service is stopping due to cancellation request");
            }
            catch (Exception ex)
            {
                Log.Error(ex, "An error occurred in the water reminder background service.");
            }
        }

    }
}
*/