using Microsoft.Bot.Builder.Dialogs;
using Microsoft.Bot.Connector;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Web;

namespace ShptTwoWayBackchannelBot.Dialogs
{
    [Serializable]
    public class RootDialog : IDialog<IMessageActivity>
    {
        public async Task StartAsync(IDialogContext context)
        {
            context.Wait(MessageReceivedAsync);
        }

        public async Task MessageReceivedAsync(IDialogContext context, IAwaitable<IMessageActivity> result)
        {
            var msg = await result;

            string[] options = new string[] { "Lists", "Webs", "ContentTypes"};
            var user = context.UserData.Get<string>("username");
            string prompt = $"Hey {user}, I'm a bot that can read your mind...well maybe not but I can count things in your SharePoint site. What do you want to count?";
            PromptDialog.Choice(context, async (IDialogContext choiceContext, IAwaitable<string> choiceResult) =>
            {
                var selection = await choiceResult;

                // Send the query through the backchannel using Event activity
                var reply = choiceContext.MakeMessage() as IEventActivity;
                reply.Type = "event";
                reply.Name = "runShptQuery";
                reply.Value = $"/_api/web/{selection}";
                await choiceContext.PostAsync((IMessageActivity)reply);
            }, options, prompt);
        }
    }
}