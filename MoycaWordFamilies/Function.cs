using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Amazon.Lambda.Core;
using Alexa.NET.Response;
using Alexa.NET.Request;
using Amazon.DynamoDBv2.Model;
using Infrastructure.Logger;
using Newtonsoft.Json;
using Alexa.NET.InSkillPricing.Responses;
using Infrastructure.Alexa;
using MoycaWordFamilies.Requests;
using Alexa.NET;

// Assembly attribute to enable the Lambda function's JSON input to be converted into a .NET class.
[assembly: Amazon.Lambda.Core.LambdaSerializer(typeof(Amazon.Lambda.Serialization.Json.JsonSerializer))]

namespace MoycaWordFamilies
{
    /// <summary>
    /// Outputs logs to labmda function
    /// </summary>
    public static class LOGGER
    {
        public static MoycaLogger log;
    }

    public class Function
    {
        public SkillResponse response;

        // REQUIRED FOR IN-SKILL PURHCASES TO WORK
        public Function()
        {
            ConnectionRequestHandler.AddToRequestConverter();
        }

        public async Task<SkillResponse> FunctionHandler(SkillRequest request, ILambdaContext context)
        {

            string requestType = request.Request.Type;
            LOGGER.log = MoycaLogger.GetLogger(context, LogLevel.TRACE);
            MoycaResponse.SetDisplaySupported(request.APLSupported());
            LogSessionStart(request);


            switch (requestType)
            {
                case "LaunchRequest":
                    LOGGER.log.DEBUG("Function", "Launch Request");
                    response = await new LaunchRequest(request).HandleRequest();
                    break;

                case "IntentRequest":
                    LOGGER.log.DEBUG("Function", "Intent Request");
                    response = new IntentRequest(request).HandleRequest();
                    break;

                case "SessionEndedRequest":
                    LOGGER.log.DEBUG("Function", "Session Ended Request");
                    response = new SessionEnded().HandleRequest();
                    break;

                //case "Connections.Response":
                //    LOGGER.log.DEBUG("Function", "Connection Response ");
                //    response = await new Connection(request).HandleRequest();
                //    break;

                default:
                    LOGGER.log.DEBUG("Function", "Default Error Request");
                    response = ResponseBuilder.Tell("Error");
                    break;
            }

            return response;
        }

        private void LogSessionStart(SkillRequest request)
        {
            LOGGER.log.INFO("BEGIN", "-----------------------------------------------------------------------");
            string skillRequest = JsonConvert.SerializeObject(request, Formatting.Indented);
            LOGGER.log.DEBUG("INPUT RECEIVED: ", skillRequest);
            LOGGER.log.INFO("Function", "USERID: " + request.Session.User.UserId);
            LOGGER.log.DEBUG("Function", "REQUEST TYPE: " + request.Request.Type);
            LOGGER.log.DEBUG("Function", "DisplaySupported: " + request.APLSupported());
        }

    }

}