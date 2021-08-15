﻿using Infrastructure.GlobalConstants;
using System;
using System.Collections.Generic;
using System.Text;

namespace Infrastructure.Alexa
{
    public class CommonPhrases
    {
        private static Random random = new Random(0);

        public static string YesOrNo
        {
            get
            {
                return "Please say yes to continue or no to quit";
            }
        }
        
        public static string Help
        {
            get
            {
                return "To restart say, 'Alexa, open Moyca Readers'. If you are connected to a display," +
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
            "Your on a role!",
            "wowza!",
            "Impressive!",
            "too good"
        };

        private static string[] shortCriticism =
        {
            "I'm sorry, let's try again",
            "So close",
            "Try again"
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

        public static string SessionFinished(string appName)
        {
            return "You're ready to move to the next lesson! Just say, Alexa, open Moyca " + appName + ".";
        }


        public static string TryAgain
        {
            get
            {
                return " Try again";
            }
        }

        public static string ConstructiveCriticism
        {
            get
            {
                return @"<amazon:emotion name=""disappointed"" intensity=""medium"">" +
                        shortCriticism[random.Next(shortCriticism.Length)] +
                        @"</amazon:emotion>";
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

        public static string Upsell(string singularName, string pluralName)
        {
            return "You only have access to one " + singularName + ". Would you like to hear how to unlock all the " + pluralName + "? ";
        }
        public static string NotPurchaseable()
        {
            return "I'm sorry, that is not available. ";
        }

        public static string InvalidRefund()
        {
            return "I'm sorry, that is not available. ";
        }

        public static string UpSellDeclined()
        {
                return "If you change your mind, at any time just say, Alexa I would like to purchase " + 
                         "content. Meanwhile, you can continue using the free flash cards. " + SSML.PauseFor(0.5);
            
        }

        public static string UpSellAccepted(string pluralName)
        {   
            return "You now have all the " + pluralName + " flash cards. ";       
        }

    }
}
