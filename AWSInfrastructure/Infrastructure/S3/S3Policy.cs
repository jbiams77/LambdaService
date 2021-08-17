using Amazon;
using Amazon.Runtime;
using Amazon.S3;
using Amazon.S3.Model;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Threading.Tasks;

namespace Infrastructure.S3
{    

    public class S3
    {
        public static RegionEndpoint REGION = RegionEndpoint.USEast1;
        public const string COGNITO_POOL_ID = "";

        private static AWSCredentials cognitoCredentials;

            
        public static async Task<StreamReader> GetFile(string bucketName, string key)
        {
            IAmazonS3 S3Client = new AmazonS3Client();

            GetObjectRequest request = new GetObjectRequest
            {
                BucketName = bucketName,
                Key = key
            };

            using (GetObjectResponse response = await S3Client.GetObjectAsync(request))
            {
                return new StreamReader(response.ResponseStream);
            }

        }
    }
}
