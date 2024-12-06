using Microsoft.Graph;
using Microsoft.Graph.Me.Presence.SetStatusMessage;
using Microsoft.Graph.Me.Presence.SetUserPreferredPresence;
using Microsoft.Graph.Models;

namespace outofoffice.Services
{
    public class Team_Services
    {
        private readonly GraphServiceClient _graphServiceClient;

        public PresenceService(GraphServiceClient graphServiceClient)
        {
            _graphServiceClient = graphServiceClient;
        }

        public async Task SetUserPresenceAsync(string availability, string activity)
        {
            var requestBody = new SetUserPreferredPresencePostRequestBody
            {
                Availability = availability,
                Activity = activity,
                ExpirationDuration = new TimeSpan(0, 8, 0, 0) // 8-hour expiration duration
            };

            await _graphServiceClient.Me.Presence.SetUserPreferredPresence.PostAsync(requestBody);
        }
        public async Task SetStatusMessageAsync(string message)
        {
            var statusMessageRequestBody = new SetStatusMessagePostRequestBody
            {
                StatusMessage = new PresenceStatusMessage
                {
                    Message = new ItemBody
                    {
                        Content = message,
                        ContentType = BodyType.Text,
                    },
                },
            };

            await _graphServiceClient.Me.Presence.SetStatusMessage.PostAsync(statusMessageRequestBody);
        }
    }
}
