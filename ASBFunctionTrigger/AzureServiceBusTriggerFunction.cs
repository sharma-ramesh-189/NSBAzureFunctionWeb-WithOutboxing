using System;
using Microsoft.Azure.ServiceBus;
using System.Threading.Tasks;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Host;
using Microsoft.Extensions.Logging;
using NServiceBus;

namespace ASBFunctionTrigger
{
    public class AzureServiceBusTriggerFunction
    {
        internal const string EndpointName = "asbworkerendpoint";
        readonly IFunctionEndpoint endpoint;

        public AzureServiceBusTriggerFunction(IFunctionEndpoint endpoint)
        {
            this.endpoint = endpoint;
        }

        [FunctionName(EndpointName)]
        public async Task Run(
            [ServiceBusTrigger(queueName: EndpointName,Connection ="AzureServiceBusConnection")]
            Message message,
            ILogger logger,
            ExecutionContext executionContext
            )
        {
            await endpoint.Process(message, executionContext, logger);
        }

    }
}
