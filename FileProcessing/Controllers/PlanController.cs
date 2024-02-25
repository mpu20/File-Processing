using Amazon.S3;
using Amazon.S3.Model;
using Microsoft.AspNetCore.Mvc;

namespace FileProcessing.Controllers;

[ApiController]
[Route("api/[controller]")]
public class PlanController(IAmazonS3 s3Client, IConfiguration configuration) : ControllerBase
{
    private readonly IAmazonS3 _s3Client = s3Client;
    private readonly IConfiguration _configuration = configuration;

    [HttpGet("download")]
    public async Task<IActionResult> DownloadPlan(string key)
    {
        var request = new GetObjectRequest
        {
            BucketName = _configuration["AWS:BucketName"],
            Key = key
        };

        using (var response = await _s3Client.GetObjectAsync(request))
        using (var responseStream = response.ResponseStream)
        {
            MemoryStream memoryStream = new MemoryStream();
            await responseStream.CopyToAsync(memoryStream);
            memoryStream.Position = 0;

            return File(memoryStream, "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet", key);
        }
    }

    [HttpPost("upload")]
    public async Task<IActionResult> UploadPlan(IFormFile? file)
    {
        if (file == null || file.Length == 0)
        {
            return BadRequest("File is empty");
        }

        const int maxSize = 500 * 1024 * 1024; // 500MB
        if (file.Length > maxSize)
        {
            return BadRequest("File size exceeds 500MB");
        }

        string[] validExtension = [".xls", ".xlsx", ".csv"];
        if (!validExtension.Contains(Path.GetExtension(file.FileName).ToLower()))
        {
            return BadRequest("Invalid file format");
        }

        var key = file.FileName;
        using (var stream = file.OpenReadStream())
        {
            var request = new PutObjectRequest
            {
                BucketName = _configuration["AWSCredentials:BucketName"],
                Key = key,
                InputStream = stream,
                ContentType = "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet"
            };

            await _s3Client.PutObjectAsync(request);
        }

        return Ok($"File {key} uploaded successfully!");
    }
}