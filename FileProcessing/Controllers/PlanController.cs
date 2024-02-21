using Microsoft.AspNetCore.Mvc;

namespace FileProcessing.Controllers;

[ApiController]
[Route("api/[controller]")]
public class PlanController : ControllerBase
{
    [HttpGet]
    [Route("download")]
    public async Task<IActionResult> DownloadPlan()
    {
        var filePath = @"Plan\plan.xlsx";
        var memory = new MemoryStream();
        await using (var stream = new FileStream(filePath, FileMode.Open))
        {
            await stream.CopyToAsync(memory);
        }

        memory.Position = 0;

        return File(memory, "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet",
            Path.GetFileName(filePath));
    }

    [HttpPost]
    [Route("upload")]
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

        if (!Path.GetExtension(file.FileName).Equals(".xlsx", StringComparison.CurrentCultureIgnoreCase))
        {
            return BadRequest("Invalid file format");
        }

        var filePath = Path.Combine("Plan", file.FileName);
        await using (var stream = new FileStream(filePath, FileMode.Create))
        {
            await file.CopyToAsync(stream);
        }

        return Ok("File uploaded successfully");
    }
}