using System.Threading.Tasks;
using Alexa.NET;
using Alexa.NET.Response;
using Alexa.NET.Request;
using Infrastructure.GlobalConstants;

namespace MoycaSubtraction.Requests.Intents
{
    public class Fallback : Intent
    {
        public Fallback(SkillRequest request) : base(request) { }

        public SkillResponse HandleIntent()
        {  
            return new Math(base.skillRequest).HandleIntent();
        }

    }
}
