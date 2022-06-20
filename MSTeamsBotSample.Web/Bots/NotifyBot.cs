using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Bot.Builder;
using Microsoft.Bot.Builder.Teams;
using Microsoft.Bot.Schema;

namespace MSTeamsBotSample.Web.Bots
{
    public class NotifyBot : TeamsActivityHandler
    {
        private readonly ConcurrentDictionary<string, ConversationReference> _conversationReferences;

        public NotifyBot(ConcurrentDictionary<string, ConversationReference> conversationReferences)
        {
            _conversationReferences = conversationReferences;
        }

        protected override async Task OnMessageActivityAsync(ITurnContext<IMessageActivity> turnContext, CancellationToken cancellationToken)
        {
            turnContext.Activity.RemoveRecipientMention();
            var text = turnContext.Activity.Text.Trim().ToLower();
            var email = await GetEmail(turnContext, cancellationToken);

            var replyText = $"You said: {text}. Your email is {email}.";
            await turnContext.SendActivityAsync(MessageFactory.Text(replyText, replyText), cancellationToken);
        }

        protected override async Task OnMembersAddedAsync(IList<ChannelAccount> membersAdded, ITurnContext<IConversationUpdateActivity> turnContext, CancellationToken cancellationToken)
        {
            var email = await GetEmail(turnContext, cancellationToken);
            var message = $"I welcome you! Your email is: {email}";
            await turnContext.SendActivityAsync(MessageFactory.Text(message, message), cancellationToken);
        }

        protected override Task OnConversationUpdateActivityAsync(ITurnContext<IConversationUpdateActivity> turnContext, CancellationToken cancellationToken)
        {
            var conversationReference = turnContext.Activity.GetConversationReference();
            _conversationReferences.AddOrUpdate(conversationReference.User.Id, conversationReference,
                (_, _) => conversationReference);
            return base.OnConversationUpdateActivityAsync(turnContext, cancellationToken);
        }

        private static async Task<string> GetEmail(ITurnContext turnContext, CancellationToken cancellationToken)
        {
            var members = await TeamsInfo.GetMembersAsync(turnContext, cancellationToken);
            var member = members.First(x => x.Id != turnContext.Activity.Recipient.Id);
            return member.Email;
        }
    }
}
