using System;
using System.Collections.Generic;
using System.Text;
using Amazon;
using Amazon.Runtime;
using Amazon.CognitoIdentityProvider;
using Amazon.CognitoIdentityProvider.Model;
using Amazon.Lambda.Core;
using System.Threading.Tasks;

namespace FlashCardService
{
    class CognitoUserPool
    {
        private AmazonCognitoIdentityProviderClient _provider = new AmazonCognitoIdentityProviderClient(new AnonymousAWSCredentials(), RegionEndpoint.USWest2);

        public async Task<GetUserResponse> GetUserData(string accessToken)
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
                Function.info.Log("Cognito get user request failed. " + e.Message);
            }

            return response;
        }
    }
}
