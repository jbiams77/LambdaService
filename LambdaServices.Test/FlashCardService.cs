using NUnit.Framework;
using System;

namespace LambdaServices.Test
{
    public class Tests
    {
        [SetUp]
        public void Setup()
        {
            Console.WriteLine("Setup");
        }

        [Test]
        public void Test1()
        {
            Console.WriteLine("Test1");
            Assert.Pass();
        }
    }
}