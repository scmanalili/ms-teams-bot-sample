using System.Collections.Concurrent;
using System.IO;
using System.Net;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Bot.Builder;
using Microsoft.Bot.Builder.Integration.AspNet.Core;
using Microsoft.Bot.Schema;
using Microsoft.Extensions.Configuration;
using Newtonsoft.Json;

namespace MSTeamsBotSample.Web.Controllers
{
    [Route("api/notify")]
    [ApiController]
    public class NotifyController : ControllerBase
    {
        private readonly IBotFrameworkHttpAdapter _adapter;
        private readonly string _appId;
        private readonly ConcurrentDictionary<string, ConversationReference> _conversationReferences;

        public NotifyController(IBotFrameworkHttpAdapter adapter, IConfiguration configuration,
            ConcurrentDictionary<string, ConversationReference> conversationReferences)
        {
            _adapter = adapter;
            _conversationReferences = conversationReferences;
            _appId = configuration["MicrosoftAppId"] ?? string.Empty;
        }

        public async Task<IActionResult> Get()
        {
            foreach (var conversationReference in _conversationReferences.Values)
            {
                await ((BotAdapter) _adapter).ContinueConversationAsync(_appId, conversationReference, BotCallback,
                    default(CancellationToken));
            }

            return new ContentResult
            {
                Content = "<html><body><h1>Messages sent!</h1></body></html>",
                ContentType = "text/html",
                StatusCode = (int) HttpStatusCode.OK
            };
        }

        private static async Task BotCallback(ITurnContext turnContext, CancellationToken cancellationToken)
        {
            await turnContext.SendActivityAsync(MessageFactory.Text("Proactive message..."), cancellationToken: cancellationToken);

            var attachment = CreateAttachment();
            await turnContext.SendActivityAsync(MessageFactory.Attachment(attachment), cancellationToken);
        }

        private static Attachment CreateAttachment()
        {
            var json = System.IO.File.ReadAllText(Path.Combine(".", "Resources", "AdaptiveCard.json"));
            var attachment = new Attachment
            {
                ContentType = "application/vnd.microsoft.card.adaptive",
                Content = JsonConvert.DeserializeObject(json)
            };
            return attachment;
        }
    }
}
