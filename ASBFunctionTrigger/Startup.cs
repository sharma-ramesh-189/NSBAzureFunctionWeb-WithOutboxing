using System;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using ASBFunctionTrigger;
using Microsoft.Azure.Cosmos;
using Microsoft.Azure.Functions.Extensions.DependencyInjection;
using Microsoft.Azure.ServiceBus;
using Microsoft.Azure.ServiceBus.Management;
using Microsoft.Azure.WebJobs;
using Microsoft.Extensions.Configuration;
using NServiceBus;

[assembly: FunctionsStartup(typeof(Startup))]

namespace ASBFunctionTrigger
{
    public class Startup : FunctionsStartup
    {
        public override void Configure(IFunctionsHostBuilder builder)
        {
            CreateTopology(builder.GetContext().Configuration).GetAwaiter().GetResult();
            builder.UseNServiceBus(() =>
            {
                var configuration = new ServiceBusTriggeredEndpointConfiguration(AzureServiceBusTriggerFunction.EndpointName, "AzureServiceBusConnection");
                configuration.UseSerialization<NewtonsoftSerializer>();

                configuration.AdvancedConfiguration.UsePersistence<CosmosPersistence>()
                    .CosmosClient(new CosmosClient("AccountEndpoint=https://localhost:7137/;AccountKey=9tZqCVYpaNwfKHZG4Umel5KnQ0f7xC8PG2KXj8OMwOamx1NfAffBPwUIPHXHmrSyl3KEAm96Un6OACDb7JQIkg==;"))
                    .DatabaseName("Outbox")
                    .DefaultContainer(
                        containerName: "Product",
                        partitionKeyPath: "/Name");
                configuration.LogDiagnostics();

                configuration.AdvancedConfiguration.EnableOutbox();
                configuration.AdvancedConfiguration.Pipeline.Register(new NameAsPartionKeyProductRequest.Registration());

                return configuration;
            });
        }

        static async Task CreateTopology(IConfiguration configuration, string topicName = "bundle-1", string auditQueue = "audit", string errorQueue = "error")
        {
            var connectionString = configuration.GetValue<string>("AzureServiceBusConnection");
            var managementClient = new ManagementClient(connectionString);

            var attribute = Assembly.GetExecutingAssembly().GetTypes()
                .SelectMany(t => t.GetMethods())
                .Where(m => m.GetCustomAttribute<FunctionNameAttribute>(false) != null)
                .SelectMany(m => m.GetParameters())
                .SelectMany(p => p.GetCustomAttributes<ServiceBusTriggerAttribute>(false))
                .FirstOrDefault();

            if (attribute == null)
            {
                throw new Exception("No endpoint was found");
            }

            // there are endpoints, create a topic
            if (!await managementClient.TopicExistsAsync(topicName))
            {
                await managementClient.CreateTopicAsync(topicName);
            }

            var endpointQueueName = attribute.QueueName;

            if (!await managementClient.QueueExistsAsync(endpointQueueName))
            {
                await managementClient.CreateQueueAsync(endpointQueueName);
            }

            if (!await managementClient.SubscriptionExistsAsync(topicName, endpointQueueName))
            {
                var subscriptionDescription = new SubscriptionDescription(topicName, endpointQueueName)
                {
                    ForwardTo = endpointQueueName,
                    UserMetadata = $"Events {endpointQueueName} subscribed to"
                };
                var ruleDescription = new RuleDescription
                {
                    Filter = new FalseFilter()
                };
                await managementClient.CreateSubscriptionAsync(subscriptionDescription, ruleDescription);
            }

            if (!await managementClient.QueueExistsAsync(auditQueue))
            {
                await managementClient.CreateQueueAsync(auditQueue);
            }

            if (!await managementClient.QueueExistsAsync(errorQueue))
            {
                await managementClient.CreateQueueAsync(errorQueue);
            }
        }
    }
}

