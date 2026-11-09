using Microsoft.AspNetCore.Mvc;
using RaceDay.Mvc.Services;
using RaceDay.Mvc.ViewModels;

namespace RaceDay.Mvc.Controllers
{
    public class ParticipantController : Controller
    {
        private readonly RaceDayApiService _api;

        public ParticipantController(RaceDayApiService api)
        {
            _api = api;
        }

        public async Task<IActionResult> MyEvents()
        {
            if (HttpContext.Session.GetString("Role") != "Participant")
            {
                return RedirectToAction("Login", "Auth");
            }

            try
            {
                var enrolments =
                    await _api.GetAsync<List<EnrolmentResultViewModel>>(
                        "api/Enrolments");

                enrolments ??= new List<EnrolmentResultViewModel>();

                return View(enrolments);
            }
            catch
            {
                TempData["Error"] =
                    "Unable to load your enrolments.";

                return View(new List<EnrolmentResultViewModel>());
            }
        }

        public async Task<IActionResult> MyResults()
        {
            if (HttpContext.Session.GetString("Role") != "Participant")
            {
                return RedirectToAction("Login", "Auth");
            }

            try
            {
                var results =
                    await _api.GetAsync<List<ResultViewModel>>(
                        "api/Results");

                results ??= new List<ResultViewModel>();

                return View(results);
            }
            catch
            {
                TempData["Error"] =
                    "Unable to load your results.";

                return View(new List<ResultViewModel>());
            }
        }

        public IActionResult Profile()
        {
            if (HttpContext.Session.GetString("Role") != "Participant")
            {
                return RedirectToAction("Login", "Auth");
            }

            return View();
        }
    }
}