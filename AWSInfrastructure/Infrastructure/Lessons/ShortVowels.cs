using AWSInfrastructure.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AWSInfrastructure.Lessons
{
    public class ShortVowels : ILesson
    {
        public string Name => "short vowels";
        public int FreeStartIndex => 1036;
        public int FreeEndIndex => 1038;
        public int CostStartIndex => 1039;
        public int CostEndIndex => 1095;
    }
}
