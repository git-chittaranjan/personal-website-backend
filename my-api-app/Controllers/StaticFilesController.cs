using Microsoft.AspNetCore.Mvc;
using my_api_app.Exceptions.BusinessExceptions;
using my_api_app.Responses;
using System.Buffers.Text;

namespace my_api_app.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class StaticController : BaseApiController
    {
        private readonly IWebHostEnvironment _env;

        public StaticController(IWebHostEnvironment env, IApiResponseFactory responseFactory)
            : base(responseFactory)
        {
            _env = env;
        }

        [HttpGet("images/{fileName}")]
        public IActionResult GetImage(string fileName)
        {
            var path = Path.Combine(_env.WebRootPath, "images", fileName);

            if (!System.IO.File.Exists(path))
                throw new NotFoundException(Statuses.StaticImageNotFound);

            return PhysicalFile(path, "image/png");
        }

        [HttpGet("pages/{fileName}")]
        public IActionResult GetHtml(string fileName)
        {
            var path = Path.Combine(_env.WebRootPath, "pages", fileName);


            if (!System.IO.File.Exists(path))
                throw new NotFoundException(Statuses.StaticHtmlNotFound);

            return PhysicalFile(path, "text/html");
        }
    }
}
