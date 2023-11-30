using System.Collections.Concurrent;
using System.Net.WebSockets;
using Services.ManegerRoom;

var builder = WebApplication.CreateBuilder(args);
builder.Services.AddControllers();
builder.Services.AddScoped<ManegerRoom>();
var app = builder.Build();
var webSockets = new ConcurrentBag<WebSocket>();

app.MapGet("/", () => "Hello World!");
app.UseRouting();
app.UseWebSockets();
app.Map("/api/chat", builder =>
{
    builder.UseEndpoints(endpoints =>
    {
        endpoints.MapControllers();
    });
});

app.MapControllers();
app.UseCors(builder =>
{
    builder.AllowAnyOrigin()
           .AllowAnyMethod()
           .AllowAnyHeader();
});
app.Run();
