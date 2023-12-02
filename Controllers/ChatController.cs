using System.Net.WebSockets;
using System.Text;
using Microsoft.AspNetCore.Mvc;
namespace Controller.ChatController
{
    [ApiController]
    [Route("api/chat")]
    public class ChatController : ControllerBase
    {
        private readonly List<WebSocket> _connections = new();


        [HttpGet]
        public async Task Get(HttpContext context)
        {
            if (HttpContext.WebSockets.IsWebSocketRequest)
            {
                var userName = context.Request.Query["name"];
                using var webSocket = await HttpContext.WebSockets.AcceptWebSocketAsync();
                Console.WriteLine(webSocket.State);
                _connections.Add(webSocket);
                await Echo(webSocket);
                await Broadcast($"{userName} joined the room");
                await Broadcast($"{userName.Count} on the room");

            }
            else
            {
                HttpContext.Response.StatusCode = 400;
            }
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
                Thread.Sleep(1000);
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
                    await socket.SendAsync(arraySegment , WebSocketMessageType.Text, true, CancellationToken.None);
                }
            }
        }
    }

}