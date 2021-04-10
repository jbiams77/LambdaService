
using Alexa.NET.Request;

namespace MoycaSubtraction.Requests.Intents
{
    public class Intent
    {
        protected SkillRequest skillRequest;

        public Intent(SkillRequest request)
        {
            this.skillRequest = request;
        }

    }
}
