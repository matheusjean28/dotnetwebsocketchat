using System.Collections.Concurrent;
using System.Net.WebSockets;
using Microsoft.OpenApi.Models;


var builder = WebApplication.CreateBuilder(args);
builder.Services.AddControllers();
builder.Services.AddSwaggerGen(c =>
  {
      c.SwaggerDoc("v1", new OpenApiInfo { Title = "DotNetWebSocketChat", Version = "v1" });
  });
var app = builder.Build();
var webSockets = new ConcurrentBag<WebSocket>();
app.UseSwagger();
app.UseSwaggerUI(c =>
  {
      c.SwaggerEndpoint("/swagger/v1/swagger.json", "Nome da Sua API V1");
      c.RoutePrefix = string.Empty;
  });
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
await app.RunAsync();
