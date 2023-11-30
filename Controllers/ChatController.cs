using Microsoft.AspNetCore.Mvc;
using Services.ManegerRoom;
namespace Controller.ChatController
{   [ApiController]
    [Route("api/chat")]
    public class ChatController : ControllerBase
    {
        private readonly ManegerRoom _manager;

        public ChatController(ManegerRoom manegerRoom)
        {
            _manager = manegerRoom;
        }

        [HttpGet]
        public async Task Get()
        {
            if (HttpContext.WebSockets.IsWebSocketRequest)
            {
                var webSocket = await HttpContext.WebSockets.AcceptWebSocketAsync();
                await _manager.HandleWebSocketAsync(webSocket);
            }
            else
            {
                HttpContext.Response.StatusCode = 400;
            }
        }
    }

}