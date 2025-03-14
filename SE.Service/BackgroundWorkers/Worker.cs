/*using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Serilog;


namespace SE.Service.BackgroundWorkers
{
    public class Worker : BackgroundService
    {
        private readonly ILogger<Worker> _logger;

        public Worker(ILogger<Worker> logger)
        {
            _logger = logger;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            Log.Information("Background Worker starting...");

            try
            {
                while (!stoppingToken.IsCancellationRequested)
                {
                    Log.Information("Background Worker running at: {time}", DateTimeOffset.Now);
                    await Task.Delay(2000, stoppingToken);
                }
            }
        
            catch (Exception ex) // Catch all exceptions
            {
                Log.Error(ex, "An error occurred in the background worker.");
            }
           
        }

    }
}
*/