
using Alexa.NET.Request;

namespace MoycaAddition.Requests.Intents
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
