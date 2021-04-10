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

            Problem currentProblem = ConvertSessionAttributeToProblem(base.skillRequest.Session);

            bool problemSolved = false;

            if (SolvedTheProblem(request, out string number))
            {
                problemSolved = NumberIsRight(number, currentProblem.answer);
            }
            else
            {
                problemSolved = false;
            }

            string prompt = "";
            Subtraction nextProblem = null;

            if (problemSolved)
            {
                nextProblem = new Subtraction();
                prompt = CommonPhrases.ShortAffirmation +  ". " +  nextProblem.ProblemPompt;
            }
            else
            {
                
                nextProblem = new Subtraction(currentProblem);
                prompt = CommonPhrases.TryAgain + ". " + nextProblem.ProblemPompt;
            }

            MoycaResponse.SetSessionAttribute(nextProblem.problem);
            MoycaResponse.SetSessionPromptAndReprompt(prompt);
            //MoycaResponse.SetSessionSlotTypeAndValue("mathProblemType", nextProblem.Answer.ToString());
            MoycaResponse.SetSessionDisplayValue(nextProblem.ProblemDisplay);
            MoycaResponse.ShouldEndSession(false);

            LOGGER.log.DEBUG("Math", "HandleIntent", "Current Problem: " + nextProblem.problem.ToString());

            return MoycaResponse.Deliver();
        }

        private bool NumberIsRight(string number, int answer)
        {
            switch (number)
            {
                case "one":
                    return (1 == answer);
                case "two":
                    return (2 == answer);
                case "three":
                    return (3 == answer);
                case "four":
                    return (4 == answer);
                case "five":
                    return (5 == answer);
                case "six":
                    return (6 == answer);
                case "seven":
                    return (7 == answer);
                case "eight":
                    return (8 == answer);
                case "nine":
                    return (9 == answer);
                case "ten":
                    return (10 == answer);
                case "eleven":
                    return (11 == answer);
                case "twelve":
                    return (12 == answer);
                case "thirteen":
                    return (13 == answer);
                case "fourteen":
                    return (14 == answer);
                case "fifteen":
                    return (15 == answer);
                case "sixteen":
                    return (16 == answer);
                case "seventeen":
                    return (17 == answer);
                case "eighteen":
                    return (18 == answer);
                default:
                    return false;
            }
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

        private bool SolvedTheProblem(Alexa.NET.Request.Type.IntentRequest input, out string number)
        {
            if (input.Intent.Slots?.Any() ?? false)
            {
                foreach (ResolutionAuthority auth in input.Intent.Slots.Last().Value.Resolution.Authorities)
                {
                    if (auth.Status.Code == ResolutionStatusCode.SuccessfulMatch)
                    {
                        number = auth.Values[0].Value.Name;
                        return true;
                    }
                }
            }
            number = null;
            return false;
        }

    }

}