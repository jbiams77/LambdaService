using System;
using System.Collections.Generic;
using System.Text;
using Amazon;
using Amazon.Runtime;
using Amazon.CognitoIdentityProvider;
using Amazon.CognitoIdentityProvider.Model;
using Amazon.Lambda.Core;
using System.Threading.Tasks;

namespace AWSInfrastructure.CognitoPool
{
    public class CognitoUserPool
    {
        private readonly string DEFAULT_USERNAME = "default";

        private AmazonCognitoIdentityProviderClient _provider = new AmazonCognitoIdentityProviderClient(new AnonymousAWSCredentials(), RegionEndpoint.USWest2);
        /// <summary>
        /// Get Username from Cognito UserPool Account
        /// </summary>
        /// <returns>the string username if it exists, otherwise a defautl username is used</returns>
        public async Task<string> GetUsername(string accessToken)
        {
            if (accessToken != null)
            {
                var userData = await GetUserData(accessToken);

                if (userData != null)
                {
                    return userData.Username;
                }
            }

            return DEFAULT_USERNAME;
        }

        private async Task<GetUserResponse> GetUserData(string accessToken)
        {
            var getUserRequest = new GetUserRequest();
            getUserRequest.AccessToken = accessToken;

            GetUserResponse response = null;
            try
            {
                response = await _provider.GetUserAsync(getUserRequest);
            }
            catch (Exception e)
            {
                //Function.info.Log("Cognito get user request failed. " + e.Message);
            }

            return response;
        }

       
    }
}
