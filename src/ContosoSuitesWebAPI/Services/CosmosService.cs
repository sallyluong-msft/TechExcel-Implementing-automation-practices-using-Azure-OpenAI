using System.Runtime.CompilerServices;
using Microsoft.AspNetCore.Localization;
using Microsoft.Azure.Cosmos;
using Microsoft.Azure.Cosmos.Linq;

namespace ContosoSuitesWebAPI.Services;

public class CosmosService : ICosmosService
{
    private readonly CosmosClient _client;
    private Container container
    {
        get => _client.GetDatabase("ContosoSuites").GetContainer("Customers");
    }

    public CosmosService()
    {
        _client = new CosmosClient(
            //connectionString: Environment.GetEnvironmentVariable("AZURE_COSMOS_DB_CONNECTION_STRING")!
            connectionString: "AccountEndpoint=https://v7xbn5ajsfu56-cosmosdb.documents.azure.com:443/;AccountKey=AewVCpxhqbdtltUCTVjYbT5pVbhZw9SEJsWcWK3KhSqH1eb1HMiAJiZ4ymKX8E8SZQhiMMT5yhcMACDb5TqjrQ==;"
        );
    }

    public async Task<IEnumerable<Customer>> GetCustomersByName(string name)
    {
        var queryable = container.GetItemLinqQueryable<Customer>();
        using FeedIterator<Customer> feed = queryable.Where(c => c.FullName == name).ToFeedIterator();
        return await ExecuteQuery(feed);
    }

    public async Task<IEnumerable<Customer>> GetCustomersByLoyaltyTier(string loyaltyTier)
    {
        LoyaltyTier lt = Enum.Parse<LoyaltyTier>(loyaltyTier);
        var queryable = container.GetItemLinqQueryable<Customer>();
        using FeedIterator<Customer> feed = queryable.Where(c => c.LoyaltyTier.ToString() == loyaltyTier).ToFeedIterator<Customer>();
        return await ExecuteQuery(feed);

    }

    public async Task<IEnumerable<Customer>> GetCustomersWithStaysAfterDate(DateTime dt)
    {
        var queryable = container.GetItemLinqQueryable<Customer>();
        using FeedIterator<Customer> feed = queryable.Where(c => c.DateOfMostRecentStay > dt).ToFeedIterator<Customer>();
        return await ExecuteQuery(feed);
    }

    private async Task<IEnumerable<Customer>> ExecuteQuery(FeedIterator<Customer> feed)
    {
        List<Customer> results = new();
        while (feed.HasMoreResults)
        {
            var response = await feed.ReadNextAsync();
            foreach (Customer c in response)
            {
                results.Add(c);
            }
        }
        return results;
    }
    
}