using CryptoWise.API.Entities;
using Microsoft.AspNetCore.Mvc;

namespace CryptoWise.API.Controllers;

[Controller]
public class BaseController : Controller
{
    protected Account? Account => (Account?)HttpContext.Items["Account"];
}