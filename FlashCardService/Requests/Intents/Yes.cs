using System.Threading.Tasks;
using Alexa.NET;
using Alexa.NET.Response;
using Alexa.NET.Request;
using AWSInfrastructure.GlobalConstants;
using Alexa.NET.Request.Type;
using AWSInfrastructure.DynamoDB;
using FlashCardService.Requests.Intents;

namespace FlashCardService.Requests.Intents
{
    public class Yes : Intent
    {
        public Yes(SkillRequest request) : base(request) { }

        public async Task<SkillResponse> HandleIntent()
        {
            Function.log.INFO("Function", "HandleYesIntent", "Current Schedule: " + base.sessionAttributes.Schedule);
                        
            if(base.sessionAttributes.SessionState != STATE.Introduction)
            {
                WordsToRead wordsToRead = new WordsToRead(this.skillRequest);
                return await wordsToRead.HandleIntent();
            }

            base.sessionAttributes.SessionState = STATE.FirstWord;

            string currentWord = this.sessionAttributes.CurrentWord;

            string prompt = "Say the word ";

            Function.log.DEBUG("Function", "HandleYesIntent", "Teach Mode: " + base.sessionAttributes.LessonMode.ToString());
            Function.log.INFO("Function", "HandleYesIntent", "Current Word: " + base.sessionAttributes.CurrentWord);


            if (this.sessionAttributes.LessonMode == MODE.Teach)
            {
                WordAttributes wordAttributes = await WordAttributes.GetWordAttributes(base.sessionAttributes.CurrentWord, Function.log);
                return base.teachMode.TeachTheWord(" ", wordAttributes);
            }
            else
            {
                return AlexaResponse.PresentFlashCard(currentWord, prompt, prompt);
            }
        }

    }
}
