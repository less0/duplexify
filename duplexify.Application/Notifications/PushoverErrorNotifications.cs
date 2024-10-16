using duplexify.Application.Contracts.Notifications;
using System.Diagnostics.CodeAnalysis;
using System.Net.Http.Json;

namespace duplexify.Application.Notifications
{
    internal class PushoverErrorNotifications : IErrorNotifications
    {
        private readonly HttpClient _httpClient;
        string _token;
        string _user;

        public PushoverErrorNotifications(HttpClient httpClient, IConfiguration configuration, ILogger<PushoverErrorNotifications> logger)
        {
            ReadConfiguration(configuration);
            logger.LogInformation("Configured Pushover for error notifications.");
            _httpClient = httpClient;
        }

        public void Send(string message)
        {
            // TODO make more resilient and background
            _httpClient.Send(new HttpRequestMessage(HttpMethod.Post, "https://api.pushover.net/1/messages.json")
            {
                Content = JsonContent.Create<Payload>(new()
                {
                    Token = _token,
                    User = _user,
                    Message = message,
                    Title = "❌ Duplexify Error",
                    Priority = 1
                })
            });
        }

        [MemberNotNull(nameof(_token))]
        [MemberNotNull(nameof(_user))]
        private void ReadConfiguration(IConfiguration configuration)
        {
            var section = configuration.GetRequiredSection("Pushover");
            _token = section.GetValue<string>("Token") ?? throw new ArgumentNullException("Token");
            _user = section.GetValue<string>("User") ?? throw new ArgumentNullException("User");

            return;
        }

        class Payload
        {
            public required string Token { get; init; }
            public required string User { get; init; }
            public required string Message { get; init; }
            public string? Device { get; set; }
            public int Priority { get; set; } = 0;
            public string? Title { get; set; }  
        }
    }
}
