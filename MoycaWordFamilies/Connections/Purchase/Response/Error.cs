using Alexa.NET.Response;
using System.Threading.Tasks;
using Alexa.NET.Request;
using Alexa.NET;
using Infrastructure.Alexa;
using MoycaWordFamilies.Requests;

namespace MoycaWordFamilies.Connections.Purchase.Response
{
    public class Error
    {
        SkillRequest skillRequest;
        public Error(SkillRequest skillRequest)
        {
            this.skillRequest = skillRequest;
        }

        public async Task<SkillResponse> Handle()
        {            
            return await new Launch(skillRequest).HandleRequest();
        }

    }
}