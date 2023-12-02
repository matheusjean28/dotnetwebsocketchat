using System.Collections.Concurrent;
using System.Net.WebSockets;

var builder = WebApplication.CreateBuilder(args);
builder.Services.AddControllers();
var app = builder.Build();
var webSockets = new ConcurrentBag<WebSocket>();

app.MapGet("/", () => "Hello World!");
app.UseRouting();
var webSocketOptions = new WebSocketOptions
{
    KeepAliveInterval = TimeSpan.FromMinutes(2)
};
app.UseWebSockets(webSocketOptions);
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
