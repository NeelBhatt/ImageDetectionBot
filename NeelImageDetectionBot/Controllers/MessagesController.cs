using System;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Web.Http;
using NeelImageDetectionBot.Service;
using Microsoft.Bot.Builder.Dialogs;
using Microsoft.Bot.Connector;

namespace NeelImageDetectionBot
{
    [BotAuthentication]
    public class MessagesController : ApiController
    {
        private readonly IImageService captionService = new ImageService();
        /// <summary>
        /// POST: api/Messages
        /// Receive a message from a user and reply to it
        /// </summary>
        public async Task<HttpResponseMessage> Post([FromBody]Activity activity)
        {
            if (activity.Type == ActivityTypes.Message)
            {
                //await Conversation.SendAsync(activity, () => new Dialogs.RootDialog());
                var connector = new ConnectorClient(new Uri(activity.ServiceUrl));
                string message;
                try
                {
                    message = await this.GetCaptionAsync(activity, connector);
                }
                catch (Exception ex)
                {
                    message = "Welcome to Image detection Bot , Please Upload or share Image Url ";

                }
                Activity reply = activity.CreateReply(message);
                await connector.Conversations.ReplyToActivityAsync(reply);
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

        private async Task<string> GetCaptionAsync(Activity activity, ConnectorClient connector)
        {
            var imageAttachment = activity.Attachments?.FirstOrDefault(a => a.ContentType.Contains("image"));
            if (imageAttachment != null)
            {
                using (var stream = await GetImageStream(connector, imageAttachment))
                {
                    return await this.captionService.GetCaptionAsync(stream);
                }
            }

            string url;
            if (TryParseAnchorTag(activity.Text, out url))
            {
                return await this.captionService.GetCaptionAsync(url);
            }

            if (Uri.IsWellFormedUriString(activity.Text, UriKind.Absolute))
            {
                return await this.captionService.GetCaptionAsync(activity.Text);
            }

            // If we reach here then the activity is neither an image attachment nor an image URL. 
            throw new ArgumentException("The activity doesn't contain a valid image attachment or an image URL.");
        }

        private static async Task<Stream> GetImageStream(ConnectorClient connector, Attachment imageAttachment)
        {
            using (var httpClient = new HttpClient())
            {

                var uri = new Uri(imageAttachment.ContentUrl);

                return await httpClient.GetStreamAsync(uri);
            }
        }
        /// <summary> 
        /// Gets the href value in an anchor element. 
        /// </summary> 
        ///  Skype transforms raw urls to html. Here we extract the href value from the url 
        /// <param name="text">Anchor tag html.</param> 
        /// <param name="url">Url if valid anchor tag, null otherwise</param> 
        /// <returns>True if valid anchor element</returns> 
        private static bool TryParseAnchorTag(string text, out string url)
        {
            var regex = new Regex("^<a href=\"(?<href>[^\"]*)\">[^<]*</a>$", RegexOptions.IgnoreCase);
            url = regex.Matches(text).OfType<Match>().Select(m => m.Groups["href"].Value).FirstOrDefault();
            return url != null;
        }
    }
}