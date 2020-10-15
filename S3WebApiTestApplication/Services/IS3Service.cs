using Amazon.S3;
using S3WebApiTestApplication.Models;
using System.Threading.Tasks;

namespace S3WebApiTestApplication.Services
{
    public interface IS3Service
    {
        Task<S3Response> CreateBucketAsync(string bucketName);
        Task UploadFileAsync(string bucketName);
        Task GetObjectFromS3Async(string bucketName);
    }
}
