using Microsoft.Extensions.DependencyInjection;
using System;

namespace TelegramAutomate.Abstract.Jobs
{
    public static class ScheduleExtensions
    {
        public static IServiceCollection AddCronJob<T>(this IServiceCollection services, Action<IScheduleConfig<T>> options) where T : AbstractJobService
        {
            if (options == null)
            {
                throw new ArgumentNullException(nameof(options), "No Schedule Configuration is passed. Please check.");
            }
            var config = new ScheduleConfig<T>();
            options.Invoke(config);
            if (string.IsNullOrWhiteSpace(config.CronExpression))
            {
                throw new ArgumentNullException(nameof(ScheduleConfig<T>.CronExpression), "No Cron Expression or Empty Cron Expression. Please check.");
            }

            services.AddSingleton<IScheduleConfig<T>>(config);
            services.AddHostedService<T>();
            return services;
        }
    }
}
