using System.Threading.Tasks;
using Alexa.NET;
using Alexa.NET.Response;
using Alexa.NET.Request;
using Alexa.NET.Request.Type;
using Infrastructure.GlobalConstants;
using System.Linq;
using Infrastructure.DynamoDB;
using Infrastructure;
using FlashCardService.Interfaces;
using FlashCardService.Factories;

namespace FlashCardService.Requests.Intents
{
    public class WordsToRead : Intent
    {
        public WordsToRead(SkillRequest request) : base(request) { }

        public async Task<SkillResponse> HandleIntent()
        {
            LOGGER.log.INFO("WordsToRead", "HandleIntent", "Current Schedule: " + this.sessionAttributes.Schedule);

            ILesson lesson = LessonFactory.GetLesson(this.sessionAttributes.LessonType);
            this.sessionAttributes.SessionState = STATE.Assess;

            var request = (Alexa.NET.Request.Type.IntentRequest)skillRequest.Request;

            string currentWord = this.sessionAttributes.CurrentWord;

            LOGGER.log.INFO("WordsToRead", "HandleIntent", "Current Word: " + currentWord);

            bool wordWasSaid = ReaderSaidTheWord(request);

            LOGGER.log.DEBUG("WordsToRead", "HandleIntent", "Reader said the word? " + wordWasSaid);

            if (wordWasSaid)
            {
                lesson.QuickReply = CommonPhrases.ShortAffirmation;
                GradeBook.Passed(sessionAttributes);
                bool sessionFinished = !sessionAttributes.WordsToRead.Any();

                if (sessionFinished)
                {
                    LOGGER.log.DEBUG("WordsToRead", "HandleIntent", "Session Finished");
                    await this.userProfile.IncrementUserProfileSchedule();
                    return ResponseBuilder.Tell(CommonPhrases.LongAffirmation + CommonPhrases.SessionFinished);
                }
            }
            else
            {
                lesson.QuickReply = CommonPhrases.ConstructiveCriticism;
                GradeBook.Missed(sessionAttributes);
            }

            WordAttributes wordAttributes = await WordAttributes.GetWordAttributes(this.sessionAttributes.CurrentWord, LOGGER.log);


            return lesson.Dialogue(sessionAttributes.LessonMode, wordAttributes);
        }

        private bool ReaderSaidTheWord(Alexa.NET.Request.Type.IntentRequest input)
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