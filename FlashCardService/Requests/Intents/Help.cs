using System.Threading.Tasks;
using Alexa.NET;
using Alexa.NET.Request;
using Alexa.NET.Response;
using Infrastructure.GlobalConstants;

namespace FlashCardService.Requests.Intents
{
    public class Help : Intent
    {
        public Help(SkillRequest request) : base(request) { }
        public async Task<SkillResponse> HandleIntent()
        {
            LOGGER.log.DEBUG("Help", "HandleIntent", "CURRENT STATE: " + this.sessionAttributes.Schedule);

            if (this.sessionAttributes.SessionState != STATE.Introduction)
            {
                WordsToRead wordsToRead = new WordsToRead(base.skillRequest);
                return await wordsToRead.HandleIntent();
            }

            this.sessionAttributes.SessionState = STATE.Help;
            var prompt = new SsmlOutputSpeech("You can move to other flash card lessons. Lessons available are word families, " +
                                        "short vowels, consonant digraphs, consonant blends, long vowels, and sight words. Just say, " +
                                        "Alexa, move to word families");
            string reprompt = "Try to say, Alexa, move to word families";
            return AlexaResponse.SayWithReprompt(prompt, reprompt);
        }

    }
}
