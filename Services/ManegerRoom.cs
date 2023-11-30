using System.Net.WebSockets;
using System.Text;

namespace Services.ManegerRoom
{
    public class ManegerRoom
    {
        private readonly Dictionary<string, List<WebSocket>> _rooms;

        public ManegerRoom()
        {
            _rooms = new Dictionary<string, List<WebSocket>>();
        }

        public async Task HandleWebSocketAsync(WebSocket webSocket)
        {
            var room = "sala_geral";

            if (!_rooms.ContainsKey(room))
            {
                _rooms[room] = new List<WebSocket>();
            }

            _rooms[room].Add(webSocket);

            var buffer = new byte[1024 * 4];

            try
            {
                while (webSocket.State == WebSocketState.Open)
                {
                    var result = await webSocket.ReceiveAsync(buffer, CancellationToken.None);

                    if (result.MessageType == WebSocketMessageType.Text)
                    {
                        var message = Encoding.UTF8.GetString(buffer, 0, result.Count);
                    }
                    else if (result.MessageType == WebSocketMessageType.Close)
                    {
                        break;
                    }
                }
            }
            catch (Exception ex)
            {
                Console.Error.WriteLine($"Erro no WebSocket: {ex.Message}");
            }
            finally
            {
                _rooms[room].Remove(webSocket);
            }
        }
    }
}
