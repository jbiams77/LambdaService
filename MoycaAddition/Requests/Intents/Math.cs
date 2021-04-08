using System.Threading.Tasks;
using Alexa.NET.Response;
using Alexa.NET.Request;
using System.Linq;
using MoycaAddition;
using MoycaAddition.Utility;
using Infrastructure.Alexa;
using Newtonsoft.Json;
using System;

namespace MoycaAddition.Requests.Intents
{
    public class Math : Intent
    {
        public Math(SkillRequest request) : base(request) { }

        public SkillResponse HandleIntent()
        {
            LOGGER.log.INFO("Math", "HandleIntent");

            var request = (Alexa.NET.Request.Type.IntentRequest)skillRequest.Request;
            
            

            bool problemSolved = SolvedTheProblem(request);
            string prompt = "";
            Addition nextProblem = null;

            if (problemSolved)
            {
                nextProblem = new Addition();
                prompt = CommonPhrases.ShortAffirmation +  ". " +  nextProblem.ProblemPompt;
            }
            else
            {
                Problem currentProblem = ConvertSessionAttributeToProblem(base.skillRequest.Session);
                nextProblem = new Addition(currentProblem);
                prompt = CommonPhrases.TryAgain + ". " + nextProblem.ProblemPompt;
            }

            MoycaResponse.SetSessionAttribute(nextProblem.problem);
            MoycaResponse.SetSessionPromptAndReprompt(prompt);
            MoycaResponse.SetSessionSlotTypeAndValue("mathProblemType", nextProblem.Answer.ToString());
            MoycaResponse.SetSessionDisplayValue(nextProblem.ProblemDisplay);
            MoycaResponse.ShouldEndSession(false);

            LOGGER.log.DEBUG("Math", "HandleIntent", "Current Problem: " + nextProblem.problem.ToString());

            return MoycaResponse.Deliver();
        }

        private Problem ConvertSessionAttributeToProblem(Session session)
        {
            try
            {
                if (session.Attributes.TryGetValue("SessionAttribute", out object cP))
                {
                    return JsonConvert.DeserializeObject<Problem>(cP.ToString());
                }
                else
                {
                    throw new ArgumentException("Session Attributes do not exist.");
                }
            }
            catch (Exception e)
            {
                LOGGER.log.WARN("Math","ConvertSessionAttributeToProblem",e.Message);
                return null;
            }
           
        }

        private bool SolvedTheProblem(Alexa.NET.Request.Type.IntentRequest input)
        {
            if (input.Intent.Slots?.Any() ?? false)
            {
                foreach (ResolutionAuthority auth in input.Intent.Slots.Last().Value.Resolution.Authorities)
                {
                    if (auth.Status.Code == ResolutionStatusCode.SuccessfulMatch)
                    {
                        return true;
                    }
                }
            }

            return false;
        }

    }

}