using AWSInfrastructure.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AWSInfrastructure.Lessons
{
    public class WordFamilies : ILesson
    {
        public string Name => "word families";

        public int FreeStartIndex => 1000;

        public int FreeEndIndex => 1002;

        public int CostStartIndex => 1003;

        public int CostEndIndex => 1035;
    }
}
