using Microsoft.AspNetCore.Mvc;
using RSS.Api.SourceArticles;
using RSS.Shared;

namespace RSS.Api.UserSources
{
    [Route("api/users/{username}/[controller]")]
    [ApiController]
    public class SourcesController : ControllerBase
    {
        private readonly Service _service;
        private readonly ArticleService _articleService;

        public SourcesController(Service service, ArticleService articleService)
        {
            _service = service;
            _articleService = articleService;
        }

        [HttpPost]
        public async Task Create([FromBody] CreateUserSourceRequest userSourceRequest)
        {
            var created = await _service.Create(userSourceRequest);
            await _articleService.Poll(created.Name);
        }

        [HttpGet]
        public async Task<IActionResult> List()
        {
            var output = await _service.List(new ListUserSourceRequest("sanjay"));
            return Ok(output);
        }
    }
}