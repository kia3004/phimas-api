using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace PHIMAS_PREDICTIVE_ANALYTICS.Controllers;

[Authorize(Roles = "CHO")]
public class CoreController : Controller
{
    public IActionResult Dashboard() => RedirectToAction("Dashboard", "CHO");
    public IActionResult Households() => RedirectToAction("Households", "CHO");
    public IActionResult HealthRecords() => RedirectToAction("HealthRecords", "CHO");
    public IActionResult PredictiveAnalytics() => RedirectToAction("PredictiveAnalytics", "CHO");
    public IActionResult Inventory() => RedirectToAction("Reports", "CHO");
    public IActionResult Reports() => RedirectToAction("Reports", "CHO");
    public IActionResult TaskMonitoring() => RedirectToAction("TaskMonitoring", "CHO");
    public IActionResult AccountSet() => RedirectToAction("AccountSet", "CHO");
    public IActionResult AddHealthRecord() => RedirectToAction("HealthRecords", "CHO");
    public IActionResult AddHousehold() => RedirectToAction("Households", "CHO");
}
