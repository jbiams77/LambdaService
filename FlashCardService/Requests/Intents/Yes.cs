using System.Threading.Tasks;
using Alexa.NET;
using Alexa.NET.Response;
using Alexa.NET.Request;
using Infrastructure.GlobalConstants;
using Alexa.NET.Request.Type;
using Infrastructure.DynamoDB;
using FlashCardService.Requests.Intents;
using Infrastructure;
using Infrastructure.Lessons;

namespace FlashCardService.Requests.Intents
{
    public class Yes : Intent
    {
        public Yes(SkillRequest request) : base(request) { }

        public async Task<SkillResponse> HandleIntent()
        {
            LOGGER.log.INFO("Yes", "HandleIntent");
                        
            if(base.sessionAttributes.SessionState != STATE.Introduction)
            {
                LOGGER.log.DEBUG("Yes", "HandleIntent", "Yes was said, but not in introduction");
                WordsToRead wordsToRead = new WordsToRead(this.skillRequest);
                return await wordsToRead.HandleIntent();
            }

            base.sessionAttributes.SessionState = STATE.FirstWord;

            string currentWord = this.sessionAttributes.CurrentWord;

            string prompt = "Say the word ";

            LOGGER.log.DEBUG("Yes", "HandleIntent", "Current Word: " + base.sessionAttributes.CurrentWord);


            if (this.sessionAttributes.LessonMode == MODE.Teach)
            {
                WordAttributes wordAttributes = await WordAttributes.GetWordAttributes(this.sessionAttributes.CurrentWord, LOGGER.log);
                string lesson = LessonFactory.GetLesson(this.sessionAttributes.LessonType, LOGGER.log).Introduction(wordAttributes);
                return AlexaResponse.PresentFlashCard(wordAttributes.Word, lesson, "Please say " + wordAttributes.Word);
            }
            else
            {
                return AlexaResponse.PresentFlashCard(currentWord, prompt, prompt);
            }
        }

    }
}
