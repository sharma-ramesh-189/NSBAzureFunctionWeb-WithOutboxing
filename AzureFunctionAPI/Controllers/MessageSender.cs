using Microsoft.AspNetCore.Mvc;
using NServiceBus;
using Domain;
namespace AzureFunctionAPI.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class MessageSender : ControllerBase
    {
        private readonly IMessageSession _messageSession;
        private readonly string _destinationEndPoint = "asbworkerendpoint";
        public MessageSender(IMessageSession messageSession)
        {
            _messageSession = messageSession;
        }

        [HttpPost("SendMessage")]
        public async Task<IActionResult> Post([FromBody] RequestDto model)
        {
            var sendOptions = new SendOptions();
            sendOptions.SetDestination(_destinationEndPoint);
            var command = new RequestProductCommand { Name=model.ProductName,Description=model.Description };
            ResponseMessage response = await _messageSession.Request<ResponseMessage>(command, sendOptions);
            return Ok(response.Message);
        }
    }
}
