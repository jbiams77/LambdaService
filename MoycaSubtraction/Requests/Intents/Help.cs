using System.Threading.Tasks;
using Alexa.NET;
using Alexa.NET.Request;
using Alexa.NET.Response;
using Infrastructure.Alexa;
using Infrastructure.GlobalConstants;
using MoycaAddition;
using MoycaAddition.Utility;

namespace MoycaAddition.Requests.Intents
{
    public class Help : Intent
    {
        public Help(SkillRequest request) : base(request) { }
        public SkillResponse HandleIntent()
        {
            LOGGER.log.INFO("Help", "HandleIntent");
            string prompt = "Tell me the answer and I will say if your answer is correct " + SSML.PauseFor(0.5) + 
                SSML.SayExtraSlow("or") + " tell me stop to finish.  Lets try this.  ";
            var addition = new Subtraction();
            prompt += addition.ProblemPompt;
            MoycaResponse.SetSessionAttribute(null);
            MoycaResponse.SetSessionPromptAndReprompt(prompt);
            MoycaResponse.SetSessionSlotTypeAndValue("mathProblemType", addition.Answer.ToString());
            MoycaResponse.SetSessionDisplayValue(addition.ProblemDisplay);

            return MoycaResponse.Deliver();
        }

    }
}
