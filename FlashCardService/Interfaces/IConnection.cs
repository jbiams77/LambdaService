using Alexa.NET.Response;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace FlashCardService.Interfaces
{
    public interface IConnection
    {
        public Task<SkillResponse> Handle(string purchaseResult);
    }
}
