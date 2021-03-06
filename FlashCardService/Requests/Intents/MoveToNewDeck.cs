using Alexa.NET;
using Alexa.NET.Request;
using Alexa.NET.Response;
using AWSInfrastructure.GlobalConstants;
using AWSInfrastructure.Lessons;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FlashCardService.Requests.Intents
{
    public class MoveToNewDeck : Intent
    {        
        public MoveToNewDeck(SkillRequest request) : base(request) { }

        public async Task<SkillResponse> HandleIntent()
        {
            LOGGER.log.INFO("MoveToNewDeck", "HandleIntent", "Deck to move to: " );
            var request = (Alexa.NET.Request.Type.IntentRequest)skillRequest.Request;

            string slot = GetSlotValue(request);
            this.sessionAttributes.SessionState = STATE.Off;

            return ResponseBuilder.Tell("If you would like to play again, 'Alexa, open Moyca Readers'. Goodbye.");
        }

        private string GetSlotValue(Alexa.NET.Request.Type.IntentRequest input)
        {
            if (input.Intent.Slots?.Any() ?? false)
            {
                
                foreach (KeyValuePair<string, Slot> slot in input.Intent.Slots)
                {
                   string s = slot.Value.Value;
                }
            }
            return "World";
        }
    }
}
