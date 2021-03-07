//using System;
//using System.Collections.Generic;
//using System.Linq;
//using System.Threading.Tasks;
//using Amazon.Lambda.Core;
//using Amazon.Runtime;
//using Alexa.NET.Request;
//using Alexa.NET.Response;
//using Alexa.NET.Request.Type;
//using Alexa.NET.Response.Converters;
//using Alexa.NET.Response.Directive;
//using Newtonsoft.Json;
//using Alexa.NET;
//using Amazon.DynamoDBv2.Model;
//using AWSInfrastructure.DynamoDB;
//using AWSInfrastructure.GlobalConstants;
//using AWSInfrastructure.Lessons;

//namespace FlashCardService
//{
    
//    public class TeachMode
//    {
//        private SessionAttributes sessionAttributes;
//        private TeachingPrompts teachingPrompts;

//        public TeachMode(SessionAttributes sessionAttributes)
//        {
//            teachingPrompts = new TeachingPrompts();
//            this.sessionAttributes = sessionAttributes;
//        }

//        public SkillResponse Introduction( WordAttributes wordAttributes)
//        {
//            LOGGER.log.INFO("TeachMode", "Introduction", "Provided for schedule" + this.sessionAttributes.Schedule);

//            LOGGER.log.DEBUG("TeachMode", "Introduction", "Lesson Introduction: " + this.sessionAttributes.Lesson.ToString());

//            SkillResponse skillResponse; 

//            switch (this.sessionAttributes.Lesson)
//            {
//                case LESSON.W:
//                    skillResponse = this.teachingPrompts.WordFamilyIntroduction(wordAttributes);
//                    break;
//                case LESSON.CVC:
//                    skillResponse = this.teachingPrompts.CVCWordIntroduction(wordAttributes);
//                    break;
//                case LESSON.ConsonantDigraph:
//                    skillResponse = this.teachingPrompts.CDIntroduction(wordAttributes);
//                    break;
//                case LESSON.ConsonantBlend:
//                    skillResponse = this.teachingPrompts.CBIntroduction(wordAttributes);
//                    break;
//                default: 
//                    skillResponse = AlexaResponse.Introduction("Hello Moycan! Are you ready to begin learning?", "You can say yes to continue or no to stop");
//                    break;
//            }

//            return skillResponse;
//        }

//        public SkillResponse TeachTheWord(string beggining, WordAttributes wordAttributes)
//        {
//            LOGGER.log.INFO("TeachMode", "TeachTheWord", "WORD: " + wordAttributes.Word + " LESSON: " + this.sessionAttributes.Lesson);

//            string teachingPrompts = beggining + " ";

//            LOGGER.log.DEBUG("TeachMode", "TeachTheWord", "Lesson to Teach: " + this.sessionAttributes.Lesson.ToString());


//            switch (this.sessionAttributes.Lesson)
//            {
//                case LESSON.WordFamilies:
//                    teachingPrompts += this.teachingPrompts.WordFamilyTeachTheWord(wordAttributes);
//                    break;
//                case LESSON.CVC:
//                    teachingPrompts += this.teachingPrompts.CVCTeachTheWord(wordAttributes);
//                    break;
//                case LESSON.ConsonantDigraph:
//                    teachingPrompts += this.teachingPrompts.CDTeachTheWord(wordAttributes);
//                    break;
//                case LESSON.ConsonantBlend:
//                    teachingPrompts += this.teachingPrompts.CBTeachTheWord(wordAttributes);
//                    break;
//                default:
//                    teachingPrompts = " ERROR ";
//                    break;
//            }

//            LOGGER.log.DEBUG("TeachMode", "TeachTheWord", "Teaching Prompt: " + teachingPrompts);

//            return AlexaResponse.PresentFlashCard(wordAttributes.Word, teachingPrompts, "Please say " + wordAttributes.Word);
//        }
//    }


//    public class TeachingPrompts
//    {
//        public SkillResponse CDIntroduction(WordAttributes wordAttributes)
//        {            


//            LOGGER.log.DEBUG("TeachingPrompts", "SigthWordsIntroduction", "Alexa Says: " + teachModel);

//            return AlexaResponse.Introduction(teachModel, "You can say yes to continue or no to stop");
//        }    

//    }

//}
