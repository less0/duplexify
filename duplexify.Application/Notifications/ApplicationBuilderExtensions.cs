using duplexify.Application.Contracts.Notifications;

namespace duplexify.Application.Notifications
{
    internal static class ApplicationBuilderExtensions
    {
        public static IHostApplicationBuilder AddNotifications(this IHostApplicationBuilder applicationBuilder)
        {
            var pushoverSection = applicationBuilder.Configuration.GetSection("Pushover");

            var pushoverToken = pushoverSection?.GetValue<string>("Token");
            var pushoverUser = pushoverSection?.GetValue<string>("User");

            if (pushoverToken != null
                && pushoverUser != null)
            {
                applicationBuilder.Services.AddTransient<IErrorNotifications, PushoverErrorNotifications>();
            }
            else
            {
                applicationBuilder.Services.AddTransient<IErrorNotifications, NullErrorNotifications>();
            }

            return applicationBuilder;
        }
    }
}
