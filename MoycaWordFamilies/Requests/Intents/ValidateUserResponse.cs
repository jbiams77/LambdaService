using Alexa.NET;
using Alexa.NET.Request;
using Alexa.NET.Response;
using Infrastructure.Alexa;
using System.Linq;
using System.Threading.Tasks;

namespace MoycaWordFamilies.Requests.Intents
{
    public class ValidateUserResponse : Intent
    {

        public ValidateUserResponse(SkillRequest request) : base(request) { }

        public async Task<SkillResponse> HandleIntent()
        {
            LOGGER.log.INFO("ValidateUserResponse", "HandleIntent");

            var request = (Alexa.NET.Request.Type.IntentRequest)skillRequest.Request;
            bool wordSaid = SaidTheWord(request);

            string prompt = "";

            if (wordSaid)
            {
                // remove current word, grab next word
                bool sessionFinished = base.words.RemoveCurrentWord();
                words.Attempts = 1;

                if (sessionFinished && words.Purchased)
                {
                    MoycaResponse.Prompt += CommonPhrases.LongAffirmation;
                    return await new Launch(skillRequest).HandleRequest();
                }
                else if (sessionFinished && !words.Purchased && words.Purchasable)
                {
                    MoycaResponse.Prompt += CommonPhrases.LongAffirmation;
                    return await new MakePurchase(this.skillRequest).HandleIntent();
                }
                else if (sessionFinished && !words.Purchased && !words.Purchasable)
                {
                    prompt += CommonPhrases.LongAffirmation;
                    return ResponseBuilder.Tell("If you would like to play again, say 'Alexa, open Moyca Word Families'. Goodbye.");
                }
                else
                {
                    LOGGER.log.DEBUG("ValidateUserResponse", "HandleIntent", "Word was said");
                    prompt += CommonPhrases.ShortAffirmation + ". " + base.words.SayTheWord();
                }
                
            }
            else
            {               

                if (base.words.Attempts > 1)
                {
                    prompt += base.words.Teach();       
                }
                else
                {
                    
                    prompt += CommonPhrases.TryAgain + ". " + base.words.SayTheWord();
                }

                words.Attempts += 1;
            }

            MoycaResponse.SessionAttributes = base.words;
            MoycaResponse.SlotName = "wordToReadType";
            MoycaResponse.SlotWord = words.CurrentWord;
            MoycaResponse.Prompt += prompt;
            MoycaResponse.Reprompt = prompt;
            MoycaResponse.DisplayValue = base.words.CurrentWord;
            MoycaResponse.ShouldEndSession = false;
            
            LOGGER.log.DEBUG("ValidateUserResponse", "HandleIntent", "Current Word: " + base.words.CurrentWord);

            return MoycaResponse.Deliver();
        }


        private bool SaidTheWord(Alexa.NET.Request.Type.IntentRequest input)
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
