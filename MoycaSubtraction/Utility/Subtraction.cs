using System;
using System.Collections.Generic;
using System.Text;

namespace MoycaSubtraction.Utility
{
    public class Subtraction
    {        
        public Problem problem;
        private Random random;

        public int Answer => problem.answer;
        public string ProblemPompt => "What is " + problem.num1 + " minus " + problem.num2 + "?";
        public string ProblemDisplay => "" + problem.num1 + " - " + problem.num2 + " = ";
        public Subtraction()
        {
            problem = new Problem();
            random = new Random();
            problem.num1 = random.Next(10);
            problem.num2 = random.Next(0, problem.num1);
            problem.answer = problem.num1 - problem.num2;
        }

        public Subtraction(Problem prob)
        {
            random = new Random();
            this.problem = prob;
        }
        
    }
}
