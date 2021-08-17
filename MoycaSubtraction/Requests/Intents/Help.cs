using System.Threading.Tasks;
using Alexa.NET;
using Alexa.NET.Request;
using Alexa.NET.Response;
using Infrastructure.Alexa;
using Infrastructure.GlobalConstants;
using MoycaSubtraction;
using MoycaSubtraction.Utility;

namespace MoycaSubtraction.Requests.Intents
{
    public class Help : Intent
    {
        public Help(SkillRequest request) : base(request) { }
        public SkillResponse HandleIntent()
        {
            LOGGER.log.INFO("Help", "HandleIntent");
            string prompt = "Tell me the answer and I will say if your answer is correct " + SSML.PauseFor(0.5) + 
                SSML.SayExtraSlow("or") + " tell me stop to finish.  Lets try this.  ";
            var subtraction = new Subtraction();
            prompt += subtraction.ProblemPompt;
            MoycaResponse.SessionAttributes = null;
            MoycaResponse.Prompt = prompt;
            MoycaResponse.Reprompt = MoycaResponse.Prompt;
            MoycaResponse.DisplayValue = subtraction.ProblemDisplay;

            return MoycaResponse.Deliver();
        }

    }
}
