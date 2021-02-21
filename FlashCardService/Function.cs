using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Amazon.Lambda.Core;
using Alexa.NET.Response;
using Alexa.NET.Request;
using Amazon.DynamoDBv2.Model;
using AWSInfrastructure.Logger;
using Newtonsoft.Json;

// Assembly attribute to enable the Lambda function's JSON input to be converted into a .NET class.
[assembly: LambdaSerializer(typeof(Amazon.Lambda.Serialization.Json.JsonSerializer))]

namespace FlashCardService
{
    using DatabaseItem = Dictionary<string, AttributeValue>;
    
    public class Function
    {
        public static MoycaLogger log;
        public SkillResponse response;

        public async Task<SkillResponse> FunctionHandler(SkillRequest request, ILambdaContext context)
        {            
            string requestType = request.Request.Type;     
            log = new MoycaLogger(context, LogLevel.TRACE);                       
            AlexaResponse.SetDisplaySupported(request.APLSupported());            
            LogSessionStart(request);
                        
            switch (requestType)
            {
                case "LaunchRequest":
                    log.DEBUG("Function", "Launch Request");
                    Requests.Launch launch = new Requests.Launch(request);
                    response = await launch.HandleRequest();
                    break;

                case "IntentRequest":
                    log.DEBUG("Function", "Intent Request");
                    Requests.IntentRequest intent = new Requests.IntentRequest(request);
                    response = await intent.HandleRequest();
                    break;

                case "SessionEndedRequest":
                    log.DEBUG("Function", "Session Ended Request");
                    Requests.SessionEnded sessionEnded = new Requests.SessionEnded(request);
                    response = await sessionEnded.HandleRequest();
                    break;

                case "Connections.Response":
                    log.DEBUG("Function", "Connection Response ");
                    response = AlexaResponse.Say("Goodbye Moycan!");
                    break;                    

                default:
                    log.DEBUG("Function", "Default Error Request");
                    response = AlexaResponse.Say("Error");
                    break;
            }

            string skillResponse = JsonConvert.SerializeObject(response, Formatting.Indented);
            log.DEBUG("output response: ", skillResponse);
            return response;
        }

        private void LogSessionStart(SkillRequest request)
        {            
            log.INFO("BEGIN", "-----------------------------------------------------------------------");
            string skillRequest = JsonConvert.SerializeObject(request, Formatting.Indented);
            log.DEBUG("INPUT RECEIVED: ", skillRequest);
            log.INFO("Function", "USERID: " + request.Session.User.UserId);
            log.INFO("Function", "REQUEST TYPE: " + request.Request.Type);
            log.INFO("Function", "DisplaySupported: " + request.APLSupported());
        }

    }

}