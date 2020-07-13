using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;

namespace PMSShop.AdminApp.Controllers
{
    public class ErrorHandleController : Controller
    {
        public IActionResult ErrorHandle(int? statusCode)
        {
            if (statusCode.HasValue)
            {
                if (statusCode.Value == 400)
                {
                    return View();
                }
            }
            return View();
        }
    }
}