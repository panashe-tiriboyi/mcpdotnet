using DataBalk.Mcp.Common.Helpers;
using DataBalk.Mcp.Common.Services.CRM;
using DataBalk.Mcp.Common.Services.HttpConnector;
using DataBalk.Mcp.Common.Services.MicrosoftAuth;
using Microsoft.OpenApi.Models;

var builder = WebApplication.CreateBuilder(args);

builder.Configuration.AddUserSecrets<Program>();

// Add CORS policy
builder.Services.AddCors(options =>
{
    options.AddDefaultPolicy(builder =>
    {
        builder.AllowAnyOrigin()
               .AllowAnyMethod()
               .AllowAnyHeader();
    });
});

builder.Services.AddMcpServer();


// Add MCP services
builder.Services.AddMvc();

// Add controllers with configuration
builder.Services.AddControllers()
    .AddJsonOptions(options =>
    {
        options.JsonSerializerOptions.PropertyNameCaseInsensitive = true;
    });

// Add HttpClient
builder.Services
    .AddHttpClient()
    .AddSingleton<IConfiguration>(builder.Configuration)
    .AddSingleton<IExecuteWithRetry, ExecuteWithRetry>()
    .AddSingleton<IMicrosoftAuth, MicrosoftAuth>()
    .AddSingleton<IHttpRequestConnector, HttpRequestConnector>()
    .AddSingleton<IDataverseService>(sp =>
    {
        var logger = sp.GetRequiredService<ILogger<DataverseService>>();
        var config = sp.GetRequiredService<IConfiguration>();
        var httpConnector = sp.GetRequiredService<IHttpRequestConnector>();
        var microsoftAuth = sp.GetRequiredService<IMicrosoftAuth>();

        return new DataverseService(
            logger,
            config,
            httpConnector,
            sp,
            microsoftAuth
        );
    });

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new OpenApiInfo
    {
        Title = "DataBalk MCP API",
        Version = "v1"
    });
});

var app = builder.Build();

// Configure Swagger middleware
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI(options =>
    {
        options.SwaggerEndpoint("/swagger/v1/swagger.json", "DataBalk MCP API v1");
    });
}

// Use CORS before other middleware
app.UseCors();
app.UseRouting();
app.UseHttpsRedirection();
app.UseAuthentication();
app.UseAuthorization();

app.UseEndpoints(endpoints =>
{
    endpoints.MapControllers();
});

app.Run();