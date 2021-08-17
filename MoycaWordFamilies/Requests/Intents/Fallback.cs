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

        public async Task<SkillResponse> HandleIntent()
        {  
            return await new ValidateUserResponse(base.skillRequest).HandleIntent();
        }

    }
}
