using JobPortalGDPS.Data;
using JobPortalGDPS.Models;
using Microsoft.AspNetCore.Mvc;
using System.Diagnostics;
using System.Security.Claims;

namespace JobPortalGDPS.Controllers
{
    public class HomeController : Controller
    {
        private readonly DapperRepository _repository;

        public HomeController(DapperRepository repository)
        {
            _repository = repository;
        }

        // GET: Index
        // Retrieves and displays the list of job postings.
        public async Task<IActionResult> Index()
        {
            var jobs = await _repository.GetJobsAsync(); // Fetch the list of jobs
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier); // Get the current UserId

            ViewData["UserId"] = userId; // Pass UserId to the view
            return View(jobs); // Pass the jobs list to the view
        }

        // GET: Details
        // Retrieves and displays details for a specific job based on its ID.
        public async Task<IActionResult> Details(int id)
        {
            var job = await _repository.GetJobByIdAsync(id); // Fetch job details by ID
            if (job == null)
            {
                return NotFound(); // Return a 404 error if job is not found
            }
            return View(job); // Pass the job details to the view
        }

        // GET: AppliedJobs
        // Retrieves and displays a list of jobs that the current user has applied for.
        [HttpGet]
        public async Task<IActionResult> AppliedJobs()
        {
            var userId = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)); // Get the current UserId
            if (userId == 0)
            {
                return RedirectToAction("Login", "Account"); // Redirect to login if user is not authenticated
            }

            var appliedJobs = await _repository.GetAppliedJobsWithDetailsAsync(userId); // Fetch applied jobs for the user
            if (appliedJobs == null)
            {
                // Handle the case where no applied jobs are found
                return View(new List<AppliedJobViewModel>()); // Return an empty list if no applied jobs
            }

            return View(appliedJobs); // Pass the list of applied jobs to the view
        }

        // GET: Error
        // Displays the error view with a request ID for debugging.
        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }
    }
}
