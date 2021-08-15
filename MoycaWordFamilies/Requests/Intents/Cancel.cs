using System.Threading.Tasks;
using Alexa.NET;
using Alexa.NET.Response;
using Alexa.NET.Request;
using Infrastructure.GlobalConstants;

namespace MoycaWordFamilies.Requests.Intents
{
    public class Cancel : Intent
    {
        public Cancel(SkillRequest request) : base(request) { }

        public SkillResponse HandleIntent()
        {
            return ResponseBuilder.Tell("If you would like to play again, say 'Alexa, open Word Family Flash Cards'. Goodbye.");
        }

    }
}
