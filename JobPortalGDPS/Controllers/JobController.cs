using JobPortalGDPS.Data;
using JobPortalGDPS.Models;
using Microsoft.AspNetCore.Mvc;

public class JobController : Controller
{
    private readonly DapperRepository _repository;

    public JobController(DapperRepository repository)
    {
        _repository = repository;
    }

    // GET: Index
    // Retrieves and displays the list of job postings.
    public async Task<IActionResult> Index()
    {
        var jobs = await _repository.GetJobsAsync(); // Fetch the list of job postings
        return View(jobs); // Pass the jobs list to the view
    }

    // GET: Apply
    // Displays the application form for a specific job based on its ID.
    [HttpGet]
    public IActionResult Apply(int jobId)
    {
        var model = new Application { JobId = jobId }; // Create an application model with the specified job ID
        return View(model); // Pass the application model to the view
    }
}
