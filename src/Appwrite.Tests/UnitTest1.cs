using System.Threading.Tasks;
using Appwrite.Helpers;
using Appwrite.Services;
using Microsoft.Extensions.Configuration;
using NUnit.Framework;

namespace Appwrite.Tests;

public class AppwriteSettings
{
    public string Endpoint { get; set; }
    public string Project { get; set; }
    public string Key { get; set; }
}

public class Tests
{
    private Client _client;
    
    [SetUp]
    public void Setup()
    { 
        var configuration = new ConfigurationBuilder()
            .AddUserSecrets<Tests>()
            .Build();

        var endpoint = configuration["AppwriteSettings:Endpoint"];
        var project  = configuration["AppwriteSettings:Project"];
        var key      = configuration["AppwriteSettings:Key"];
        
        _client = new Client();

        _client
            // Your API Endpoint
            .SetEndPoint(endpoint)  
            // Your project ID 
            .SetProject(project)
            // Your secret API key
            .SetKey(key);
    }   

    [Test]
    public async Task TestDatabases()
    {
        var query = new Query().Equal("name","agreements");
        var queries = query.BuildUrl();
        
        var databases = new Databases(_client);
        var list = await databases.ListDatabases(queries);
        var database = await databases.GetDatabase("agreements");

        var collections = await database.ListCollections();

        var content = await collections.Content.ReadAsStringAsync();

        //var result = await list.ToObject<QueryDatabases>();
        
        Assert.Pass();
    }
}