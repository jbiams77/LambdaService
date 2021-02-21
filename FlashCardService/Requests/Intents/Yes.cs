using System.Threading.Tasks;
using Alexa.NET.Response;
using AWSInfrastructure.GlobalConstants;

namespace FlashCardService.Requests.Intents
{
    public class Yes : Intent
    {
        public Yes() { }

        public async Task<SkillResponse> HandleIntent()
        {
            Function.log.INFO("Function", "HandleYesIntent", "Current Schedule: " + this.sessionAttributes.Schedule);
                        
            if(this.sessionAttributes.SessionState != STATE.Introduction)
            {
                WordsToRead wordsToRead = new WordsToRead();
                return await wordsToRead.HandleIntent();
            }

            this.sessionAttributes.SessionState = STATE.FirstWord;

            string currentWord = this.sessionAttributes.CurrentWord;

            string prompt = "Say the word ";

            Function.log.DEBUG("Function", "HandleYesIntent", "Teach Mode: " + this.sessionAttributes.LessonMode.ToString());
            Function.log.INFO("Function", "HandleYesIntent", "Current Word: " + this.sessionAttributes.CurrentWord);


            if (this.sessionAttributes.LessonMode == MODE.Teach)
            {
                WordAttributes wordAttributes = await WordAttributes.GetWordAttributes(this.sessionAttributes.CurrentWord, Function.log);
                return this.teachMode.TeachTheWord(" ", wordAttributes);
            }
            else
            {
                return AlexaResponse.PresentFlashCard(currentWord, prompt, prompt);
            }
        }

    }
}
