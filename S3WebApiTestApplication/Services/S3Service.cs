using Amazon.S3;
using Amazon.S3.Model;
using Amazon.S3.Transfer;
using Amazon.S3.Util;
using S3WebApiTestApplication.Models;
using System;
using System.IO;
using System.Net;
using System.Threading.Tasks;

namespace S3WebApiTestApplication.Services
{
    public class S3Service : IS3Service
    {
        private readonly IAmazonS3 _client;
        public S3Service(IAmazonS3 client)
        {
            _client = client;
        }

        [Obsolete]
        public async Task<S3Response> CreateBucketAsync(string bucketName)
        {
            try
            {
                if (await AmazonS3Util.DoesS3BucketExistAsync(_client, bucketName) == false)
                {
                    var putBucketRequest = new PutBucketRequest
                    {
                        BucketName = bucketName,
                        UseClientRegion = true
                    };

                    var response = await _client.PutBucketAsync(putBucketRequest);

                    return new S3Response
                    {
                        Message = response.ResponseMetadata.RequestId,
                        Status = response.HttpStatusCode
                    };
                }
            }
            catch (AmazonS3Exception ex)
            {
                return new S3Response
                {
                    Status = ex.StatusCode,
                    Message = ex.Message
                };
            }
            catch (Exception ex)
            {
                return new S3Response
                {
                    Status = HttpStatusCode.InternalServerError,
                    Message = ex.Message
                };
            }

            return new S3Response
            {
                Status = HttpStatusCode.InternalServerError,
                Message = "Something went wrong!"
            };
        }

        private const string FilePath = "C:\\Users\\viki4\\Desktop\\AWS\\Test.txt";
        private const string UploadWithKeyName = "UploadWithKeyName";
        private const string FileStreamUpload = "FileStreamUpload";
        private const string AdvancedUpload = "AdvancedUpload";

        public async Task UploadFileAsync(string bucketName)
        {
            try
            {
                var fileTransferUtility = new TransferUtility(_client);

                //option1
                await fileTransferUtility.UploadAsync(FilePath, bucketName);

                //Option2
                await fileTransferUtility.UploadAsync(FilePath, bucketName, UploadWithKeyName);

                //Option3
                using (var fileToUpload = new FileStream(FilePath, FileMode.Open, FileAccess.Read))
                {
                    await fileTransferUtility.UploadAsync(fileToUpload, bucketName, FileStreamUpload);
                }

                //Option4
                var fileTransferUtilityRequest = new TransferUtilityUploadRequest
                {
                    BucketName = bucketName,
                    FilePath = FilePath,
                    StorageClass = S3StorageClass.Standard,
                    PartSize = 6291456,  //6MB
                    Key = AdvancedUpload,
                    CannedACL = S3CannedACL.NoACL
                };

                fileTransferUtilityRequest.Metadata.Add("param1", "Value1");
                fileTransferUtilityRequest.Metadata.Add("param2", "Value2");

                await fileTransferUtility.UploadAsync(fileTransferUtilityRequest);
            }

            catch (AmazonS3Exception ex)
            {
                Console.WriteLine("Error occured on Server. Message: '{0}' when writing an object", ex.Message);
            }
            catch (Exception ex)
            {
                Console.WriteLine("Error occured on Server. Message: '{0}' when writing an object", ex.Message);
            }
        }

        public async Task GetObjectFromS3Async(string bucketName)
        {
            const string keyName = "Test.txt";
            try
            {
                var request = new GetObjectRequest
                {
                    BucketName = bucketName,
                    Key = keyName
                };

                string responseBody;

                using (var response = await _client.GetObjectAsync(request))
                using (var responseStream = response.ResponseStream)
                using (var reader = new StreamReader(responseStream))
                {
                    var title = response.Metadata["x-amz-meta-title"];
                    var contentType = response.Headers["Content-Type"];

                    Console.WriteLine($"Object meta, Title: {title}");
                    Console.WriteLine($"Content Type: {contentType}");

                    responseBody = reader.ReadToEnd();
                }

                //var pathAndFileName = $"C:\\Users\\viki4\\Desktop\\AWS\\{keyName}";
                var pathAndFileName = $"C:\\Users\\viki4\\Desktop\\AWS\\DownloadedFileFromS3.txt";

                var createText = responseBody;

                File.WriteAllText(pathAndFileName, createText);
            }
            catch (AmazonS3Exception ex)
            {
                Console.WriteLine("Error encountered ***. Message: '{0}' when getting an object", ex.Message);
            }
            catch (Exception ex)
            {
                Console.WriteLine("Unknown error occured on server. Message: '{0}' when getting an object", ex.Message);
            }
        }
    }
}
