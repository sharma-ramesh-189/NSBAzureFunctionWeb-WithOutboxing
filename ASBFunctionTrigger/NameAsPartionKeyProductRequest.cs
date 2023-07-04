using NServiceBus.Logging;
using NServiceBus.Persistence.CosmosDB;
using NServiceBus.Pipeline;
using System.Threading.Tasks;
using System;
using Domain;
using Microsoft.Azure.Cosmos;

namespace ASBFunctionTrigger
{
    public class NameAsPartionKeyProductRequest : Behavior<IIncomingLogicalMessageContext>
    {
        static readonly ILog Log = LogManager.GetLogger<NameAsPartionKeyProductRequest>();

        public override async Task Invoke(IIncomingLogicalMessageContext context, Func<Task> next)
        {
            if (context.Message.Instance is RequestProductCommand request)
            {
                var partitionKeyValue = request.Name;
                Log.Info($"PartitionKeyBehavior Invoke:: '{partitionKeyValue}' from '{nameof(request)}'");
                context.Extensions.Set(new PartitionKey(partitionKeyValue));
            }
            await next().ConfigureAwait(false);
        }

        public class Registration : RegisterStep
        {
            public Registration() :
                base(nameof(NameAsPartionKeyProductRequest),
                    typeof(NameAsPartionKeyProductRequest),
                    "Determines the PartitionKey from the logical message",
                    b => new NameAsPartionKeyProductRequest())
            {
                InsertBeforeIfExists(nameof(LogicalOutboxBehavior));
            }
        }

    }
}
