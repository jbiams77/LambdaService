using System;
using System.Collections.Generic;
using System.Text;

namespace MoycaAddition.Utility
{
    public class Problem 
    {
        public int num1;
        public int num2;
        public int answer;
        public override string ToString()
        {
            return "" + num1 + " + " + num2 + " = " + answer;
        }
    }
}
