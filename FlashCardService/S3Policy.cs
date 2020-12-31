using Amazon.S3;
using Amazon.S3.Model;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Threading.Tasks;

namespace FlashCardService
{
    class S3
    {
        private string BucketName { get { return "moyca-policies"; } }

        AmazonS3Client client = new AmazonS3Client();
        public S3()
        {            
        }

        public async Task<string> GetFile(string key)
        {
            // Create a GetObject request
            GetObjectRequest request = new GetObjectRequest
            {
                BucketName = this.BucketName,
                Key = key
            };

            // Issue request and remember to dispose of the response
            using (GetObjectResponse response = await client.GetObjectAsync(request))
            {
                // convert stream to string
                StreamReader reader = new StreamReader(response.ResponseStream);
                return reader.ReadToEnd();
            }

        }
    }
}
