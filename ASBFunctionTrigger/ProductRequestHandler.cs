using Domain;
using NServiceBus;
using NServiceBus.Logging;
using System;
using System.Threading.Tasks;

namespace ASBFunctionTrigger
{
    public class ProductRequestHandler : IHandleMessages<RequestProductCommand>
    {
        static readonly ILog Log = LogManager.GetLogger<ProductRequestHandler>();
        public Task Handle(RequestProductCommand message, IMessageHandlerContext context)
        {
            Log.Info($"Recieved Message: {message}");
            Log.Warn($"Handling {nameof(RequestProductCommand)} in {nameof(ProductRequestHandler)}");
            var session = context.SynchronizedStorageSession.CosmosPersistenceSession();
            var product = new Product { Name = message.Name, Description = message.Description };
            session.Batch.CreateItem(product);
            var responseMessage = new ResponseMessage
            {
                Message = $"Product Added with product name {product.Name} and id {product.Id}."
            };
            return context.Reply(responseMessage);
        }
    }
    public class Product
    {
        public string Id { get; set; } = Guid.NewGuid().ToString();
        public string Name { get; set; }
        public string Description { get; set; }
    }
}
