using Infrastructure.Logger;
using Infrastructure.GlobalConstants;
using Infrastructure;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Alexa.NET.Response;

namespace Infrastructure.Alexa
{
    public interface ILesson
    {
        string ProductName { get; }
        string InSkillPurchaseName { get; }
        string LessonTypeName { get; }
        int FreeStartIndex { get; }
        int FreeEndIndex { get; }
        int CostStartIndex { get; }
        int CostEndIndex { get; }
        string QuickReply { get; set; }

        SkillResponse Introduction(WordAttributes wordAttributes);
        SkillResponse Dialogue(MODE mode, WordAttributes wordAttributes);
    }
}
