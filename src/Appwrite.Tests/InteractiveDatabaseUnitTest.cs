using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Appwrite.Helpers;
using Appwrite.Services;
using Microsoft.Extensions.Configuration;
using Newtonsoft.Json;
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
    public async Task CreateDatabases()
    {
        //var databases = new Databases(_client);
        //databases.ListDatabases()
        
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

        var content = collections.Total;

        //var result = await list.ToObject<QueryDatabases>();
        
        Assert.Pass();
    }
    
    public class ClientModel
    {
        public string Name { get; set; }
        public string RegisteredNumber { get; set; }
        public List<string> Address { get; set; }
        public string EMail { get; set; }
        public string Phone { get; set; }
        public string Signer { get; set; }
        public string Type { get; set; }

        [JsonProperty("$id")]
        public string Id { get; set; }

        // [JsonProperty("$createdAt")]
        // public DateTime createdAt { get; set; }
        //
        // [JsonProperty("$updatedAt")]
        // public DateTime updatedAt { get; set; }
        //
        // [JsonProperty("$permissions")]
        // public List<string> permissions { get; set; }

        //[JsonProperty("$collectionId")]
        //public string collectionId { get; set; }

        //[JsonProperty("$databaseId")]
        //public string databaseId { get; set; }
    }

    [Test]
    public async Task TestCollections()
    {
        var databases = new Databases(_client);
        var database = await databases.GetDatabase("agreements");
        var collections = await database.ListCollections();
        var collection = await database.GetCollection<ClientModel>("Clients");

        var docs = await collection.ListDocument();
        //var doc = await collection.GetDocumentById("6377e0fcb851bd4e2eb0");
        //var content = await doc.Content.ReadAsStringAsync();
        
        var doc = await collection.GetDocument("6377e0fcb851bd4e2eb0");

        doc.Name = "zzzzs4";
        
        var created = await collection.CreateDocument(doc);
        
        //var content = await result.Content.ReadAsStringAsync();
    }
    
}