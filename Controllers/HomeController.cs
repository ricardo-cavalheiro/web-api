using Blog.Attributes;
using Microsoft.AspNetCore.Mvc;

namespace Blog.Controllers;

[Route("v1")]
[ApiController]
public class HomeController : ControllerBase {
  [HttpGet]
  [ApiKey]
  public IActionResult Get() {
    return Ok();
  }
}