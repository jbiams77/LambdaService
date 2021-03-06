using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AWSInfrastructure.Interfaces
{
    public interface ILesson
    {
        string Name { get; }
        int FreeStartIndex { get; }
        int FreeEndIndex { get; }
        int CostStartIndex { get; }
        int CostEndIndex { get; }
    }
}
