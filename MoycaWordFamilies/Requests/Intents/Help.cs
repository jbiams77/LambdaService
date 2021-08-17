using System.Threading.Tasks;
using Alexa.NET;
using Alexa.NET.Request;
using Alexa.NET.Response;
using Infrastructure.Alexa;
using Infrastructure.GlobalConstants;
using MoycaWordFamilies.Utility;

namespace MoycaWordFamilies.Requests.Intents
{
    public class Help : Intent
    {
        public Help(SkillRequest request) : base(request) { }

        public SkillResponse HandleIntent()
        {
            LOGGER.log.INFO("Help", "HandleIntent");

            MoycaResponse.Prompt = base.words.Help();
            MoycaResponse.SessionAttributes = base.words;
            MoycaResponse.Reprompt = MoycaResponse.Prompt;
            MoycaResponse.DisplayValue = base.words.CurrentWord;
            MoycaResponse.ShouldEndSession = false;

            return MoycaResponse.Deliver();
        }

    }
}
