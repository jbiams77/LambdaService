
using Alexa.NET.Request;
using MoycaWordFamilies.Utility;

namespace MoycaWordFamilies.Requests.Intents
{
    public class Intent
    {
        protected SkillRequest skillRequest;
        protected WordFamilies words;

        public Intent(SkillRequest request)
        {
            this.skillRequest = request;
            this.words = new WordFamilies(request);
        }

    }
}
