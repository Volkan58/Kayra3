using Yarp.ReverseProxy.Configuration;
using Serilog;

var builder = WebApplication.CreateBuilder(args);

// Serilog
Log.Logger = new LoggerConfiguration()
    .ReadFrom.Configuration(builder.Configuration)
    .Enrich.FromLogContext()
    .WriteTo.Console()
    .WriteTo.File("logs/api-gateway-.txt", rollingInterval: RollingInterval.Day)
    .WriteTo.Seq("http://localhost:5341")
    .CreateLogger();

builder.Host.UseSerilog();

// Services
builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

// Reverse Proxy
builder.Services.AddReverseProxy()
    .LoadFromConfig(builder.Configuration.GetSection("ReverseProxy"));

// CORS
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowAll", policy =>
    {
        policy.AllowAnyOrigin()
              .AllowAnyMethod()
              .AllowAnyHeader();
    });
});

var app = builder.Build();

// Swagger UI Gateway üzerinden
app.UseSwagger();
app.UseSwaggerUI(c =>
{
    c.SwaggerEndpoint("/api/auth/swagger/v1/swagger.json", "Auth API");
    c.SwaggerEndpoint("/api/product/swagger/v1/swagger.json", "Product API");
    c.SwaggerEndpoint("/api/log/swagger/v1/swagger.json", "Log API");
});

// Middleware
app.UseHttpsRedirection();
app.UseCors("AllowAll");

// YARP
app.MapReverseProxy();

app.MapControllers();

try
{
    Log.Information("Starting API Gateway");
    app.Run();
}
catch (Exception ex)
{
    Log.Fatal(ex, "API Gateway terminated unexpectedly");
}
finally
{
    Log.CloseAndFlush();
}
