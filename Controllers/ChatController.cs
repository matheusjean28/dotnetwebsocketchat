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
        [Route("api")]

        public async Task<OkObjectResult> GetFiles()
        {
            return Ok("all done");
        }


        [HttpGet]
        public async Task Get(HttpContext context)
        {
            Console.WriteLine(context.Connection.GetType());
            if (HttpContext.WebSockets.IsWebSocketRequest)
            {
                var userName = context.Request.Query["name"];
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