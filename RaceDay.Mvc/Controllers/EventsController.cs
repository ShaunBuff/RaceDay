using Microsoft.AspNetCore.Mvc;
using RaceDay.Mvc.Services;
using RaceDay.Mvc.ViewModels;

namespace RaceDay.Mvc.Controllers
{
    public class EventsController : Controller
    {
        private readonly RaceDayApiService _api;

        public EventsController(RaceDayApiService api)
        {
            _api = api;
        }

        // GET: /Events
        public async Task<IActionResult> Index()
        {
            try
            {
                var events = await _api.GetAsync<List<EventViewModel>>(
                    "api/Events");

                var locations = await _api.GetAsync<List<LocationViewModel>>(
                    "api/Locations");

                var eventTypes = await _api.GetAsync<List<EventTypeViewModel>>(
                    "api/EventTypes");

                events ??= new List<EventViewModel>();
                locations ??= new List<LocationViewModel>();
                eventTypes ??= new List<EventTypeViewModel>();

                foreach (var eventItem in events)
                {
                    var location = locations.FirstOrDefault(
                        x => x.LocationID == eventItem.LocationID);

                    if (location != null)
                    {
                        eventItem.LocationName =
                            location.LocationName;
                    }

                    var eventType = eventTypes.FirstOrDefault(
                        x => x.EventTypeID == eventItem.EventTypeID);

                    if (eventType != null)
                    {
                        eventItem.EventTypeName =
                            eventType.TypeName;
                    }
                }

                return View(events);
            }
            catch
            {
                TempData["Error"] =
                    "Unable to load RaceDay events.";

                return View(new List<EventViewModel>());
            }
        }

        // GET: /Events/Details/1
        public async Task<IActionResult> Details(int id)
        {
            try
            {
                var eventItem =
                    await _api.GetAsync<EventViewModel>(
                        $"api/Events/{id}");

                if (eventItem == null)
                {
                    return NotFound();
                }

                var locations =
                    await _api.GetAsync<List<LocationViewModel>>(
                        "api/Locations");

                var eventTypes =
                    await _api.GetAsync<List<EventTypeViewModel>>(
                        "api/EventTypes");

                var categories =
                    await _api.GetAsync<List<CategoryViewModel>>(
                        "api/Categories");

                locations ??= new List<LocationViewModel>();
                eventTypes ??= new List<EventTypeViewModel>();
                categories ??= new List<CategoryViewModel>();

                var location = locations.FirstOrDefault(
                    x => x.LocationID == eventItem.LocationID);

                if (location != null)
                {
                    eventItem.LocationName =
                        location.LocationName;
                }

                var eventType = eventTypes.FirstOrDefault(
                    x => x.EventTypeID == eventItem.EventTypeID);

                if (eventType != null)
                {
                    eventItem.EventTypeName =
                        eventType.TypeName;
                }

                var eventCategories = categories
                    .Where(x => x.EventID == id)
                    .ToList();

                ViewBag.Categories = eventCategories;

                return View(eventItem);
            }
            catch
            {
                TempData["Error"] =
                    "Unable to load event details.";

                return RedirectToAction(nameof(Index));
            }
        }

        // POST: /Events/Enrol
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Enrol(
            EnrolmentViewModel model)
        {
            if (!ModelState.IsValid)
            {
                TempData["Error"] =
                    "Invalid enrolment details.";

                return RedirectToAction(
                    nameof(Details),
                    new { id = model.EventID });
            }

            try
            {
                var response = await _api.PostAsync(
                    "api/Enrolments",
                    model);

                if (response.IsSuccessStatusCode)
                {
                    TempData["Success"] =
                        "You have successfully enrolled in this category.";

                    return RedirectToAction(
                        nameof(Details),
                        new { id = model.EventID });
                }

                if (response.StatusCode ==
                    System.Net.HttpStatusCode.Conflict)
                {
                    TempData["Error"] =
                        "You are already enrolled in this category.";

                    return RedirectToAction(
                        nameof(Details),
                        new { id = model.EventID });
                }

                if (response.StatusCode ==
                    System.Net.HttpStatusCode.Unauthorized)
                {
                    TempData["Error"] =
                        "Your session has expired. Please log in again.";

                    return RedirectToAction(
                        "Login",
                        "Auth");
                }

                TempData["Error"] =
                    "Unable to complete the enrolment.";
            }
            catch
            {
                TempData["Error"] =
                    "Unable to connect to the RaceDay API.";
            }

            return RedirectToAction(
                nameof(Details),
                new { id = model.EventID });
        }
    }
}