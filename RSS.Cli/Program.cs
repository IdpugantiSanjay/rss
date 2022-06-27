// See https://aka.ms/new-console-template for more information

// username/sources

// username/articles
// :hide
// :fav
// :notes
// :readLater

using System.Net.Http.Json;
using System.Web;
using RSS.Shared;


var commandType = args[0];
var username = "sanjay";

var http = new HttpClient {BaseAddress = new($"http://localhost:5167")};

if (commandType == "LIST SOURCES")
{
    var httpResponseMessage = await http.GetAsync($"/api/users/{username}/Sources");

    var output = await httpResponseMessage.Content.ReadFromJsonAsync<ListUserSourceResponse>();
    if (output is not { }) return;

    var sourceNames = output.Entities.Select(e => e.Name);

    foreach (var sourceName in sourceNames)
    {
        Console.WriteLine(sourceName);
    }
}
else if (commandType == "LIST ARTICLES IN SOURCE")
{
    var sourceName = args[1]; // Engineering at Meta

    if (string.IsNullOrWhiteSpace(sourceName)) return;
    var encodedSourceName = HttpUtility.UrlEncode(sourceName);

    var pollResponse = await http.PostAsync($"/api/users/{username}/Sources/{encodedSourceName}/articles/Poll",
        new StringContent(""));
    pollResponse.EnsureSuccessStatusCode();

    var httpResponseMessage = await http.GetAsync($"/api/users/{username}/Sources/{encodedSourceName}/articles");

    httpResponseMessage.EnsureSuccessStatusCode();

    var output = await httpResponseMessage.Content.ReadFromJsonAsync<ListArticlesResponse>();
    if (output is not { }) return;

    var articles = output.Entities.ToList();
    foreach (var entity in articles)
    {
        Console.WriteLine($"{entity.Id}::{entity.Name}");
    }
}
else if (commandType == "GET ARTICLE LINK")
{
    var sourceName = args[1];
    var articleId = args[2];

    if (string.IsNullOrWhiteSpace(sourceName)) return;
    if (string.IsNullOrWhiteSpace(articleId)) return;


    var encodedArticleId = HttpUtility.UrlEncode(articleId);
    var encodedSourceName = HttpUtility.UrlEncode(sourceName);

    var httpResponseMessage =
        await http.GetAsync($"/api/users/{username}/Sources/{encodedSourceName}/articles/{encodedArticleId}");

    httpResponseMessage.EnsureSuccessStatusCode();

    var output = await httpResponseMessage.Content.ReadFromJsonAsync<GetArticleResponse>();
    if (output is not { }) return;
    Console.WriteLine(output.Link);
}
else if (commandType == "ARTICLE ACTIONS")
{
    Console.WriteLine("open\nfavorite\nhide");
}
else if (commandType == "FAVORITE ARTICLE")
{
    var sourceName = HttpUtility.UrlEncode(args[1]);
    var articleId = HttpUtility.UrlEncode(args[2]);

    Console.WriteLine(sourceName);
    Console.WriteLine(articleId);

    var httpResponseMessage =
        await http.PostAsync($"/api/users/{username}/Sources/{sourceName}/articles/{articleId}:markAsFavorite",
            new StringContent(""));

    httpResponseMessage.EnsureSuccessStatusCode();
}
else if (commandType == "HIDE ARTICLE")
{
    var sourceName = HttpUtility.UrlEncode(args[1]);
    var articleName = HttpUtility.UrlEncode(args[2]);

    var httpResponseMessage =
        await http.PostAsync($"/api/users/{username}/Sources/{sourceName}/articles/{articleName}:hide",
            new StringContent(""));

    httpResponseMessage.EnsureSuccessStatusCode();
}