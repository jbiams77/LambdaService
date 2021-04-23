using Infrastructure.Logger;
using Infrastructure.GlobalConstants;
using Infrastructure;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Alexa.NET.Response;
using Infrastructure.Alexa;

namespace Infrastructure.Interfaces
{
    public interface ILesson
    {
        string ProductName { get; }
        string InSkillPurchaseName { get; }
        string LessonTypeName { get; }
        string QuickReply { get; set; }

        string Introduction(WordEntry wordAttributes);
        string Dialogue(MODE mode, WordEntry wordAttributes);
    }
}
