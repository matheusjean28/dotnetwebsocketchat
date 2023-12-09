using System;
using System.Collections.Generic;
using System.Net.WebSockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;


namespace Controller.ChatController
{
    [ApiController]
    [Route("/ws")]
    public class WebSocketController : ControllerBase
    {
        private readonly List<WebSocket> _connections = new();
        private readonly ILogger<WebSocketController> _logger;

        public WebSocketController(ILogger<WebSocketController> logger)
        {
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

       

        [HttpGet]
        public async Task Chat(HttpContext context)
        {
            var typeconect = context.Connection.GetType();
            _logger.LogInformation($"{typeconect}");
            _logger.LogInformation("Beat here .");

            Console.WriteLine(context.Connection.GetType());
            Console.WriteLine("pass here");
              if (context.Request.Path == "/ws")
            {if (HttpContext.WebSockets.IsWebSocketRequest == true)
            {
                var userName = context.Request.Query["name"];
                _logger.LogInformation($"{userName} is attempting to join the room.");
                using var webSocket = await HttpContext.WebSockets.AcceptWebSocketAsync();
                _connections.Add(webSocket);
                await Echo(webSocket);
                await Broadcast($"{userName} joined the room");
                await Broadcast($"{userName.Count} on the room");
                await ReciveMessage(webSocket,
                async (result, buffer) =>
                {
                    if (result.MessageType == WebSocketMessageType.Text)
                    {
                        string message = Encoding.UTF8.GetString(buffer, 0, result.Count);
                        await Broadcast(userName + ":" + message);
                    }
                    else if (result.MessageType == WebSocketMessageType.Close || webSocket.State == WebSocketState.Aborted)
                    {
                        _connections.Remove(webSocket);
                        await Broadcast($"{userName} left the room");
                        await Broadcast($"{_connections.Count} on the room");
                        await webSocket.CloseAsync(result.CloseStatus.Value, result.CloseStatusDescription, CancellationToken.None);
                    }
                });

            }
            else
            {
                HttpContext.Response.StatusCode = 400;
            }}
            
        }

        private static async Task Echo(WebSocket webSocket)
        {
            var buffer = new byte[1024 * 4];
            var receiveResult = await webSocket.ReceiveAsync(
                new ArraySegment<byte>(buffer), CancellationToken.None);

            while (!receiveResult.CloseStatus.HasValue)
            {
                await webSocket.SendAsync(
                    new ArraySegment<byte>(buffer, 0, receiveResult.Count),
                    receiveResult.MessageType,
                    receiveResult.EndOfMessage,
                    CancellationToken.None);


                Console.WriteLine(webSocket);

                receiveResult = await webSocket.ReceiveAsync(
                    new ArraySegment<byte>(buffer), CancellationToken.None);
            }

            await webSocket.CloseAsync(
                receiveResult.CloseStatus.Value,
                receiveResult.CloseStatusDescription,
                CancellationToken.None);
        }

        async Task Broadcast(string message)
        {
            var bytes = Encoding.UTF8.GetBytes(message);
            foreach (var socket in _connections)
            {
                if (socket.State == WebSocketState.Open)
                {
                    var arraySegment = new ArraySegment<byte>(bytes, 0, bytes.Length);
                    await socket.SendAsync(arraySegment, WebSocketMessageType.Text, true, CancellationToken.None);
                }
            }
        }

        async Task ReciveMessage(WebSocket socket, Action<WebSocketReceiveResult, byte[]> handleMessage)
        {
            var buffer = new byte[1024 * 4];
            while (socket.State == WebSocketState.Open)
            {
                var result = await socket.ReceiveAsync(new ArraySegment<byte>(buffer), CancellationToken.None);
                handleMessage(result, buffer);
            }
        }
    }

}