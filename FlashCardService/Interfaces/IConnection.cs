using Alexa.NET.Response;
using System;
using System.Collections.Generic;
using System.Text;

namespace FlashCardService.Interfaces
{
    public interface IConnection
    {
        public SkillResponse Handle(string purchaseResult);
    }
}
