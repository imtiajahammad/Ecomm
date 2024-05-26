using System.Diagnostics;
using Microsoft.AspNetCore.Mvc;
using ClientPlatform.Models;
using ClientPlatform.DataAccess;

namespace ClientPlatform.Controllers;

public class HomeController : Controller
{
    private readonly IOrderDetailsProvider _orderDetailsProvider;
    private readonly ILogger<HomeController> _logger;

    public HomeController(ILogger<HomeController> logger, IOrderDetailsProvider orderDetailsProvider)
    {
        _logger = logger;
        _orderDetailsProvider = orderDetailsProvider;
    }

    public async Task<IActionResult> Index()
    {
        var orderDetails = await _orderDetailsProvider.Get();
        return View(orderDetails);
    }

    public IActionResult Privacy()
    {
        return View();
    }

    [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
    public IActionResult Error()
    {
        return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
    }
}

