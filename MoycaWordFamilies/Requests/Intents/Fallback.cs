using System.Threading.Tasks;
using Alexa.NET;
using Alexa.NET.Response;
using Alexa.NET.Request;
using Infrastructure.GlobalConstants;

namespace MoycaWordFamilies.Requests.Intents
{
    public class Fallback : Intent
    {
        public Fallback(SkillRequest request) : base(request) { }

        public SkillResponse HandleIntent()
        {  
            return new ValidateUserResponse(base.skillRequest).HandleIntent();
        }

    }
}
