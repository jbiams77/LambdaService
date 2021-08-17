using System.Threading.Tasks;
using Alexa.NET;
using Alexa.NET.Response;
using Alexa.NET.Request;
using Infrastructure.GlobalConstants;
using Alexa.NET.Request.Type;
using Infrastructure.DynamoDB;
using FlashCardService.Requests.Intents;
using Infrastructure;
using FlashCardService.Factories;

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
                       

            LOGGER.log.DEBUG("Yes", "HandleIntent", "Current Word: " + base.sessionAttributes.CurrentWord);
            WordAttributes wordAttributes = await WordAttributes.GetWordAttributes(this.sessionAttributes.CurrentWord, LOGGER.log);

            return LessonFactory.GetLesson(this.sessionAttributes.LessonType).Dialogue(sessionAttributes.LessonMode, wordAttributes);

        }

    }
}
