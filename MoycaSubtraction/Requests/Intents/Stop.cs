using System.Threading.Tasks;
using Alexa.NET;
using Alexa.NET.Request;
using Alexa.NET.Response;
using Infrastructure.GlobalConstants;

namespace MoycaSubtraction.Requests.Intents
{
    public class Stop : Intent
    {
        public Stop(SkillRequest request) : base(request) { }

        public SkillResponse HandleIntent()
        {
            return ResponseBuilder.Tell("If you would like to play again, 'Alexa, open Moyca subtraction'. Goodbye.");
        }

    }
}
