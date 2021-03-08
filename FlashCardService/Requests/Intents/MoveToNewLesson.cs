using Alexa.NET;
using Alexa.NET.Request;
using Alexa.NET.Response;
using Infrastructure.GlobalConstants;
using Infrastructure.Interfaces;
using Infrastructure.Lessons;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FlashCardService.Requests.Intents
{
    public class MoveToNewLesson : Intent
    {        
        public MoveToNewLesson(SkillRequest request) : base(request) { }

        public async Task<SkillResponse> HandleIntent()
        {
            LOGGER.log.INFO("MoveToNewLesson", "HandleIntent", "Deck to move to: " );
            var intentRequest = (Alexa.NET.Request.Type.IntentRequest)skillRequest.Request;

            string lessonType = GetSlotValue(intentRequest);
            LOGGER.log.INFO("MoveToNewLesson", "HandleIntent", "Deck to move to: " + lessonType);
            ILesson lesson = LessonFactory.GetLesson(lessonType, LOGGER.log);
            await base.userProfile.ChangeLesson(lesson);

            this.sessionAttributes.SessionState = STATE.Introduction;
            // relaunch for new lesson
            return await new LaunchRequest(base.skillRequest).HandleRequest();
        }

        private string GetSlotValue(Alexa.NET.Request.Type.IntentRequest input)
        {
            string slotValue = "";

            if (input.Intent.Slots?.Any() ?? false)
            {                
                foreach (KeyValuePair<string, Slot> slot in input.Intent.Slots)
                {
                   slotValue = slot.Value.Value;
                }
            }

            return slotValue;
        }
    }
}
