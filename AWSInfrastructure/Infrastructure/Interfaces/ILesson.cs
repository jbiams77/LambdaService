using Infrastructure.Logger;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Infrastructure.Interfaces
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
        MoycaLogger Log { get; set; }

        string Introduction(WordAttributes wordAttributes);

        string TeachTheWord(WordAttributes wordAttributes);
    }
}
