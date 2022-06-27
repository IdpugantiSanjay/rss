using System.Web;
using Microsoft.AspNetCore.Mvc;

namespace RSS.Api.SourceArticles;

[Route("api/users/{username}/sources/{source}/[controller]")]
[ApiController]
public class ArticlesController : ControllerBase
{
    private readonly ArticleService _service;

    public ArticlesController(ArticleService service)
    {
        _service = service;
    }

    [HttpPost("[action]")]
    public async Task Poll(string source)
    {
        await _service.Poll(HttpUtility.UrlDecode(source));
    }

    public record CreateRequest(string ArticleUrl);

    [HttpPost]
    public async Task<IActionResult> Create(string username, [FromBody] CreateRequest request)
    {
        return Ok(await _service.Create(username, request.ArticleUrl));
    }

    [HttpGet]
    public async Task<IActionResult> List(string source)
    {
        var decodedSource = HttpUtility.UrlDecode(source);
        var response = await _service.List(decodedSource);
        return Ok(response);
    }

    [HttpGet("{id}")]
    public async Task<IActionResult> Get(string source, string id)
    {
        var decodedSource = HttpUtility.UrlDecode(source);
        var decodedArticleId = HttpUtility.UrlDecode(id);

        var response = await _service.Get(decodedSource, decodedArticleId);
        return Ok(response);
    }


    [Route("/api/users/{username}/sources/{source}/[controller]/{id}:[action]")]
    [HttpPost]
    public async Task<IActionResult> MarkAsFavorite(string username, string source, string id)
    {

        try
        {
            var updated = await _service.MarkAsFavorite(username ,HttpUtility.UrlDecode(source), HttpUtility.UrlDecode(id));
            if (updated is null) return NotFound();
            return Ok();
        }
        catch (Exception e)
        {
            Console.WriteLine(e);
            throw;
        }
        
        
    }
}