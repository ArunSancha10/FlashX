﻿
using Hangfire.Server;
using Hangfire;
using System.Diagnostics.CodeAnalysis;

namespace outofoffice.Helper
{
    public class RecurringJobsService : BackgroundService
    {
        private readonly IRecurringJobManager _recurringJobs;
        private readonly ILogger<RecurringJobScheduler> _logger;

        public RecurringJobsService(
            [NotNull] IRecurringJobManager recurringJobs,
            [NotNull] ILogger<RecurringJobScheduler> logger)
        {
            _recurringJobs = recurringJobs ?? throw new ArgumentNullException(nameof(recurringJobs));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        protected override Task ExecuteAsync(CancellationToken stoppingToken)
        {
            try
            {
                //_recurringJobs.AddOrUpdate("seconds", () => Console.WriteLine("Hello, seconds!"), "*/15 * * * * *");
                //_recurringJobs.AddOrUpdate("minutely", () => Console.WriteLine("Hello, world!"), Cron.Minutely);

                _recurringJobs.AddOrUpdate("hourly", () => Console.WriteLine("Hello"), "25 15 * * *");

                //_recurringJobs.AddOrUpdate("neverfires", () => Console.WriteLine("Can only be triggered"), "0 0 31 2 *");

                //_recurringJobs.AddOrUpdate("Hawaiian", () => Console.WriteLine("Hawaiian"), "15 08 * * *", new RecurringJobOptions
                //{
                //    TimeZone = TimeZoneInfo.FindSystemTimeZoneById("Hawaiian Standard Time")
                //});
                //_recurringJobs.AddOrUpdate("UTC", () => Console.WriteLine("UTC"), "15 18 * * *");
                //_recurringJobs.AddOrUpdate("Russian", () => Console.WriteLine("Russian"), "15 21 * * *", new RecurringJobOptions
                //{
                //    TimeZone = TimeZoneInfo.Local
                //});
            }
            catch (Exception e)
            {
                _logger.LogError(e, "An exception occurred while creating recurring jobs.");
            }

            return Task.CompletedTask;
        }
    }
}