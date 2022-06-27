using Cassandra;
using Cassandra.Mapping;
using RSS.Api.SourceArticles;
using RSS.Api.Sources;
using RSS.Api.UserSources;
using UserSourceService = RSS.Api.UserSources.Service;
using SourceService = RSS.Api.Sources.Service;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.

builder.Services.AddControllers();
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

var queryOptions = new QueryOptions().SetConsistencyLevel(ConsistencyLevel.One);

builder.Services.AddSingleton<IMapper>(_ =>
{
    var cluster = Cluster.Builder()
        .AddContactPoint("127.0.0.1")
        .WithQueryOptions(queryOptions)
        .WithDefaultKeyspace("rss")
        .Build();

    var session = cluster.Connect();
    return new Mapper(session);
});

builder.Services.AddSingleton<UserSourceService>();
builder.Services.AddSingleton<SourceService>();
builder.Services.AddSingleton<ArticleService>();

MappingConfiguration.Global.Define<UserSourceMapping>();
MappingConfiguration.Global.Define<SourceMapping>();
MappingConfiguration.Global.Define<SourceArticleMapping>();
MappingConfiguration.Global.Define<FavoriteArticleMapping>();


var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

// app.UseHttpsRedirection();

app.UseAuthorization();

app.MapControllers();

app.Run();