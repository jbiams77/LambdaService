using System;
using System.Collections.Generic;
using System.Text;

namespace FlashCardService
{
    public class CommonPhrases
    {
        private static Random random = new Random(0);

        public static string Help
        {
            get
            {
                return "To restart say, 'Alexa, open Moycan Readers'. If you are connected to a display," +
                        "a flashcard will appear and you will read the word aloud. If there is no display, " + 
                        "I will spell the word and you say it.";
            }
        }

        private static string[] greeting =
        {
            "Hello ",
            "Hi ",
            "Good day "            
        };

        public static string Greeting
        {
            get
            {
                return greeting[random.Next(greeting.Length)];
            }
        }

        private static string[] shortAffirmation =
        {
            "GoodJob!",
            "Nice!",
            "Amazing!",
            "Keep it up!",
            "Your good!",
            "Thats right!",
            "Don't stop now!",
            "wowza!",
            "Impressive!",
            "So Good!",

        };

        public static string ShortAffirmation
        {
            get
            {
                return @"<amazon:effect name=""whispered""><amazon:emotion name=""excited"" intensity=""medium"">" + 
                        shortAffirmation[random.Next(shortAffirmation.Length)] +
                        @"</amazon:emotion></amazon:effect>";
            }
        }

        private static string[] longAffirmation =
        {
            "Congratulation my little Moycan. ",
            "I knew Moy Can do it! ",
            "You are turning into quite the reader. ",
            "I wish I could give you a high five! ",
            "Where did you learn to read like that? ",
            "You will be reading Shakespeare before you know it. "
        };

        public static string LongAffirmation
        {
            get
            {
                return longAffirmation[random.Next(longAffirmation.Length)];
            }
        }

    }
}
