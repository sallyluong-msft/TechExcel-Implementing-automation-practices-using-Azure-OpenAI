using Azure.Identity;
using Microsoft.Azure.Cosmos;
using ContosoSuitesWebAPI.Services;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

builder.Services.AddSingleton<ICosmosService, CosmosService>();

builder.Services.AddSingleton<CosmosClient>((_) =>
{
    CosmosClient client = new(
        connectionString: builder.Configuration["AZURE_COSMOS_DB_CONNECTION_STRING"]!
    );
    return client;
});

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}



app.UseHttpsRedirection();

app.MapGet("/Customer", async (string searchCriterion, string searchValue) => 
{
    switch (searchCriterion)
    {
        case "CustomerName":
            return await app.Services.GetService<ICosmosService>()!.GetCustomersByName(searchValue);
        case "LoyaltyTier":
            return await app.Services.GetService<ICosmosService>()!.GetCustomersByLoyaltyTier(searchValue);
        case "DateOfMostRecentStay":
            return await app.Services.GetService<ICosmosService>()!.GetCustomersWithStaysAfterDate(DateTime.Parse(searchValue));
        default:
            throw new Exception("Invalid search criterion. Valid search criteria include 'CustomerName', 'LoyaltyTier', and 'DateOfMostRecentStay'.");
    }
})
    .WithName("GetCustomer")
    .WithOpenApi();

app.Run();
