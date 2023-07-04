using NServiceBus;

namespace Domain
{
    public class ResponseMessage:IMessage
    {
        public string Message { get; set; }
    }
}