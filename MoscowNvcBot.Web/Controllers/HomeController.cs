﻿using System.Diagnostics;
using Microsoft.AspNetCore.Mvc;
using MoscowNvcBot.Web.Models;

namespace MoscowNvcBot.Web.Controllers
{
    public class HomeController : Controller
    {
        public IActionResult Index()
        {
            return View();
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            var model = new ErrorViewModel {RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier};
            return View(model);
        }
    }
}
