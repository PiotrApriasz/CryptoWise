using CryptoWise.API.Entities;
using Microsoft.AspNetCore.Mvc;

namespace CryptoWise.API.Controllers;

[Controller]
public class BaseController : Controller
{
    public Account? Account => (Account?)HttpContext.Items["Account"];
}