using System;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using System.Web.Http;
using System.Web.Http.Description;
using Microsoft.Bot.Connector;
using Newtonsoft.Json;
using Microsoft.Bot.Builder.Dialogs;
using ShptTwoWayBackchannelBot.Dialogs;
using Newtonsoft.Json.Linq;

namespace ShptTwoWayBackchannelBot
{
    [BotAuthentication]
    public class MessagesController : ApiController
    {
        /// <summary>
        /// POST: api/Messages
        /// Receive a message from a user and reply to it
        /// </summary>
        public async Task<HttpResponseMessage> Post([FromBody]Activity activity)
        {
            if (activity.Type == ActivityTypes.Event)
            {
                // Get the event type and process accordingly
                if (activity.Name == "initialize")
                {
                    // Get the username from activity Properties then save it into BotState
                    var username = activity.Value.ToString();
                    var state = activity.GetStateClient();
                    var userdata = state.BotState.GetUserData(activity.ChannelId, activity.From.Id);
                    userdata.SetProperty<string>("username", username);
                    state.BotState.SetUserData(activity.ChannelId, activity.From.Id, userdata);

                    // Forward to the RootDialog now that we know who the user is
                    await Conversation.SendAsync(activity, () => new RootDialog());
                }
                else if (activity.Name == "queryResults")
                {
                    // Get the operation and count
                    var json = (JObject)activity.Value;
                    var entityType = json.Value<string>("entityType");
                    var count = json.Value<string>("count");

                    // Respond with the results in the bot
                    ConnectorClient connector = new ConnectorClient(new Uri(activity.ServiceUrl));
                    Activity reply = activity.CreateReply($"That was too easy...you have {count} {entityType} in this site! Let's try again...");
                    await connector.Conversations.ReplyToActivityAsync(reply);
                    await Conversation.SendAsync(activity, () => new RootDialog());
                }
                else
                {
                    // Ignore everything else
                }
            }
            else if (activity.Type == ActivityTypes.Message)
            {
                // Forward to the RootDialog
                await Conversation.SendAsync(activity, () => new RootDialog());
            }
            else
            {
                HandleSystemMessage(activity);
            }
            var response = Request.CreateResponse(HttpStatusCode.OK);
            return response;
        }

        private Activity HandleSystemMessage(Activity message)
        {
            if (message.Type == ActivityTypes.DeleteUserData)
            {
                // Implement user deletion here
                // If we handle user deletion, return a real message
            }
            else if (message.Type == ActivityTypes.ConversationUpdate)
            {
                // Handle conversation state changes, like members being added and removed
                // Use Activity.MembersAdded and Activity.MembersRemoved and Activity.Action for info
                // Not available in all channels
            }
            else if (message.Type == ActivityTypes.ContactRelationUpdate)
            {
                // Handle add/remove from contact lists
                // Activity.From + Activity.Action represent what happened
            }
            else if (message.Type == ActivityTypes.Typing)
            {
                // Handle knowing tha the user is typing
            }
            else if (message.Type == ActivityTypes.Ping)
            {
            }

            return null;
        }
    }
}