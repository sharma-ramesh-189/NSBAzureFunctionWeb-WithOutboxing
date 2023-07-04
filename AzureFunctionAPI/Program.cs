using NServiceBus;
var builder = WebApplication.CreateBuilder(args);

// Add services to the container.

builder.Services.AddControllers();
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();
builder.Host.UseNServiceBus(context =>
{
    var endpointConfiguration = new EndpointConfiguration("Samples.ASBS.SendReply.Endpoint1");
    endpointConfiguration.SendFailedMessagesTo("error");
    endpointConfiguration.UseSerialization<NewtonsoftSerializer>();
    endpointConfiguration.EnableInstallers();
    endpointConfiguration.MakeInstanceUniquelyAddressable("CallBack");
    endpointConfiguration.EnableCallbacks();

    var transport = endpointConfiguration.UseTransport<AzureServiceBusTransport>();
    var connectionString = builder.Configuration.GetConnectionString("AzureServiceBusConnection");
    transport.ConnectionString(connectionString);
    return endpointConfiguration;
});
var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}
app.UseHttpsRedirection();
app.UseAuthorization();
app.MapControllers();
app.Run();
