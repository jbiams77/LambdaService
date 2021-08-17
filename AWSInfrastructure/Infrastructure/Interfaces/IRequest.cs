using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Alexa.NET;
using Alexa.NET.Response;
using Alexa.NET.Request;
using Alexa.NET.Request.Type;
using Alexa.NET.InSkillPricing.Responses;

namespace Infrastructure.Interfaces
{
    public interface IRequest
    {
        public Task<SkillResponse> HandleRequest();
    }
}
