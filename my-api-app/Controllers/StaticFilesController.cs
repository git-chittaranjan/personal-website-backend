using Microsoft.AspNetCore.Mvc;

namespace my_api_app.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class StaticController : ControllerBase
    {
        private readonly IWebHostEnvironment _env;

        public StaticController(IWebHostEnvironment env)
        {
            _env = env;
        }

        [HttpGet("images/{fileName}")]
        public IActionResult GetImage(string fileName)
        {
            var path = Path.Combine(_env.WebRootPath, "images", fileName);

            if (!System.IO.File.Exists(path))
                return NotFound("File not found: " + fileName);

            return PhysicalFile(path, "image/png");
        }

        [HttpGet("pages/{fileName}")]
        public IActionResult GetHtml(string fileName)
        {
            var path = Path.Combine(_env.WebRootPath, "pages", fileName);


            if (!System.IO.File.Exists(path))
                return NotFound("File not found: " + fileName);

            return PhysicalFile(path, "text/html");
        }
    }
}
