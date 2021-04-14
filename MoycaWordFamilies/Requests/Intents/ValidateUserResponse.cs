using Alexa.NET.Request;
using Alexa.NET.Response;
using Infrastructure.Alexa;
using MoycaWordFamilies.Utility;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace MoycaWordFamilies.Requests.Intents
{
    public class ValidateUserResponse : Intent
    {

        public ValidateUserResponse(SkillRequest request) : base(request) { }

        public SkillResponse HandleIntent()
        {
            LOGGER.log.INFO("Math", "HandleIntent");

            var request = (Alexa.NET.Request.Type.IntentRequest)skillRequest.Request;

            WordsToRead word = new WordsToRead(base.skillRequest.Session);

            bool problemSolved = false;

            if (SaidTheWord(request, out string wordSaid))
            {
                problemSolved = (wordSaid.Equals(word.CurrentWord));
            }
            else
            {
                problemSolved = false;
            }

            string prompt = "";
            WordsToRead nextWord = null;

            if (problemSolved)
            {
                nextWord = new WordsToRead();
                prompt = CommonPhrases.ShortAffirmation + ". " + nextWord.ToString();
            }
            else
            {

                nextWord = word;
                prompt = CommonPhrases.TryAgain + ". " + nextWord.ToString();
            }

            MoycaResponse.SetSessionAttribute(nextWord.ToString());
            MoycaResponse.SetSessionPromptAndReprompt(prompt);
            //MoycaResponse.SetSessionSlotTypeAndValue("mathProblemType", nextProblem.Answer.ToString());
            MoycaResponse.SetSessionDisplayValue(nextWord.ToString());
            MoycaResponse.ShouldEndSession(false);

            LOGGER.log.DEBUG("Math", "HandleIntent", "Current Problem: " + nextWord.ToString());

            return MoycaResponse.Deliver();
        }
                


        private bool SaidTheWord(Alexa.NET.Request.Type.IntentRequest input, out string number)
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
