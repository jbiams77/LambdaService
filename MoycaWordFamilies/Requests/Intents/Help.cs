using System.Threading.Tasks;
using Alexa.NET;
using Alexa.NET.Request;
using Alexa.NET.Response;
using Infrastructure.Alexa;
using Infrastructure.GlobalConstants;
using MoycaWordFamilies.Utility;

namespace MoycaWordFamilies.Requests.Intents
{
    public class Help : Intent
    {
        public Help(SkillRequest request) : base(request) { }
        public SkillResponse HandleIntent()
        {
            LOGGER.log.INFO("Help", "HandleIntent");

            return MoycaResponse.Deliver();
        }

    }
}
