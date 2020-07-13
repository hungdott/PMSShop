using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;

namespace PMSShop.BackendApi.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class ErrorsController : Controller
    {
        [HttpGet("error/404")]
        public IActionResult Error404Handle()
        {
            return View();
        }
    }
}