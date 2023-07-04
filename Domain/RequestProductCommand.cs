using NServiceBus;

namespace Domain
{
    public class RequestProductCommand:ICommand
    {
        public string Name { get; set; }
        public string Description { get; set; }
    }
}
