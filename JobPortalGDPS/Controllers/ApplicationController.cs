using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using JobPortalGDPS.Data;
using JobPortalGDPS.Models;
using System.IO;
using System.Security.Claims;
using System.Threading.Tasks;

namespace JobPortalGDPS.Controllers
{
    public class ApplicationController : Controller
    {
        private readonly DapperRepository _repository;
        private readonly ILogger<ApplicationController> _logger;

        public ApplicationController(DapperRepository repository, ILogger<ApplicationController> logger)
        {
            _repository = repository;
            _logger = logger;
        }

        // GET: Display the application form for a specific job
        [HttpGet]
        public IActionResult Apply(int jobId)
        {
            var model = new Application
            {
                JobId = jobId
            };
            return View(model);
        }

        // GET: Download the resume file associated with a specific application
        [HttpGet]
        public async Task<IActionResult> DownloadResume(int applicationId)
        {
            // Retrieve the application from the database
            var application = await _repository.GetApplicationByIdAsync(applicationId);

            if (application == null || application.ResumeFile == null)
            {
                return NotFound(); // Handle case where application or resume is not found
            }

            // Prepare the file for download
            var fileName = $"resume_{applicationId}.pdf"; // Filename for the resume
            return File(application.ResumeFile, "application/pdf", fileName);
        }

        // POST: Delete an application by its ID
        [HttpPost]
        public async Task<IActionResult> DeleteApplication(int applicationId)
        {
            if (applicationId <= 0)
            {
                return BadRequest(); // Handle invalid application ID
            }

            try
            {
                await _repository.DeleteApplicationAsync(applicationId);
                TempData["Message"] = "Application deleted successfully."; // Show success message
            }
            catch (Exception ex)
            {
                // Log the exception and display an error message
                _logger.LogError(ex, "An error occurred while deleting the application.");
                TempData["Error"] = "An error occurred while deleting the application.";
            }

            return RedirectToAction("AppliedJobs", "Home"); // Redirect to applied jobs page
        }

        // POST: Handle the submission of a new job application
        [HttpPost]
        public async Task<IActionResult> Apply()
        {
            _logger.LogInformation("Apply POST request started.");

            // Get the resume file from the form data
            var resumeFile = Request.Form.Files["Resume"];
            if (resumeFile == null || resumeFile.Length == 0)
            {
                return Json(new { success = false, message = "Resume file is required." }); // Handle missing resume file
            }

            byte[] resumeData = null;
            try
            {
                // Convert the resume file to a byte array
                using (var memoryStream = new MemoryStream())
                {
                    await resumeFile.CopyToAsync(memoryStream);
                    resumeData = memoryStream.ToArray();
                }
                _logger.LogInformation("File uploaded and converted to byte array successfully.");
            }
            catch (Exception ex)
            {
                // Log the error and return a failure response
                _logger.LogError(ex, "Error occurred while handling file upload.");
                return Json(new { success = false, message = "Error occurred while processing file upload." });
            }

            // Retrieve the user ID from the claims
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (userId == null)
            {
                _logger.LogWarning("User not authenticated.");
                return Json(new { success = false, message = "User not authenticated." });
            }

            // Check if the user has already applied for the job
            var applicationExists = await _repository.ApplicationExistsAsync(int.Parse(userId), int.Parse(Request.Form["JobId"]));
            if (applicationExists)
            {
                _logger.LogWarning("User has already applied for this job.");
                return Json(new { success = false, message = "You have already applied for this job." });
            }

            var application = new Application
            {
                UserId = int.Parse(userId),
                JobId = int.Parse(Request.Form["JobId"]),
                ResumeFile = resumeData,
                CreatedAt = DateTime.Now
            };

            try
            {
                // Save the new application to the database
                await _repository.AddApplicationAsync(application);
                _logger.LogInformation("Application added successfully.");
                return Json(new { success = true, redirectUrl = Url.Action("Index", "Home") });
            }
            catch (Exception ex)
            {
                // Log the error and return a failure response
                _logger.LogError(ex, "Error occurred while adding application.");
                return Json(new { success = false, message = "Error occurred while processing your application." });
            }
        }
    }
}
