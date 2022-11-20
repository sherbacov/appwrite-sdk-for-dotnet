using System.Reflection;
using Appwrite.Helpers;
using Appwrite.Models;
using Newtonsoft.Json;

namespace Appwrite.Services
{
    public class ObjectModel
    {
        [JsonProperty("$id")]
        public string Id { get; set; }
        
        [JsonProperty("$createdAt")]
        public DateTime CreatedAt { get; set; }

        [JsonProperty("$updatedAt")]
        public DateTime UpdatedAt { get; set; }
    }
    
    public class DatabaseModel : ObjectModel
    {
        [JsonProperty("name")]
        public string Name { get; set; }
    }

    public class QueryDatabases
    {
        public int Total { get; set; }
        public List<DatabaseModel> Databases { get; set; }
    }
    
    public class CollectionAttribute
    {
        public string Key { get; set; }
        public string Type { get; set; }
        public string Status { get; set; }
        public bool Required { get; set; }
        public bool Array { get; set; }
        public int Size { get; set; }
        public object Default { get; set; }
        public List<string> Elements { get; set; }
        public string Format { get; set; }
        public int? Min { get; set; }
        public int? Max { get; set; }
    }

    public class CollectionModel
    {
        [JsonProperty("$id")]
        public string Id { get; set; }

        [JsonProperty("$createdAt")]
        public DateTime CreatedAt { get; set; }

        [JsonProperty("$updatedAt")]
        public DateTime UpdatedAt { get; set; }

        [JsonProperty("$permissions")]
        public List<object> Permissions { get; set; }
        public string DatabaseId { get; set; }
        public string Name { get; set; }
        public bool Enabled { get; set; }
        public bool DocumentSecurity { get; set; }
        public List<CollectionAttribute> Attributes { get; set; }
        public List<CollectionIndex> Indexes { get; set; }

        public override string ToString()
        {
            return $"{Name} [{Id}]";
        }
    }

    public class CollectionIndex
    {
        public string Key { get; set; }
        public string Type { get; set; }
        public string Status { get; set; }
        public List<string> Attributes { get; set; }
        public List<string> Orders { get; set; }
    }

    public class QueryCollections
    {
        public int Total { get; set; }
        public List<CollectionModel> Collections { get; set; }
    }
    
    public class QueryDocuments<T>
    {
        public int Total { get; set; }
        public List<T> Documents { get; set; }
    }
    
    public class Databases : Service
    {
        public Databases(Client client) : base(client)
        {
        }

        private QueryDatabases _cache;
        
        public async Task<QueryDatabases> ListDatabases(
            string[] queries = null, int? limit = 25,
            int? offset = 0, OrderType orderType = OrderType.ASC)
        {
            var path = "/databases";

            var objects = await ListObjects(path, queries, limit, offset, orderType);
            
            var result = await objects.ToObject<QueryDatabases>();

            _cache = result;
            
            return result;
        }

        public async Task<Database> GetDatabase(string name)
        {
            // Lookup in cache
            if (_cache != null)
            {
                var dbCache = _cache.Databases.FirstOrDefault(d => d.Name == name);
                if (dbCache != null)
                    return new Database(_client, dbCache.Id);
            }

            //Lookup database
            var result = await ListDatabases(new Query().Equal("name", name).BuildUrl());
            
            var database = result.Databases.FirstOrDefault(d => d.Name == name);
            
            return new Database(_client, database.Id);
        }
    }
    
    public class Database : Service
    {
        public Database(Client client, string databaseId) : base(client)
        {
            DatabaseId = databaseId;
        }
        
        public string DatabaseId { get; private set;}

        /// <summary>
        /// List Collections
        /// <para>
        /// Get a list of all the user collections. You can use the query params to
        /// filter your results. On admin mode, this endpoint will return a list of all
        /// of the project's collections. [Learn more about different API
        /// modes](/docs/admin).
        /// </para>
        /// </summary>
        public async Task<QueryCollections> ListCollections(
            string[] queries = null, int? limit = 25, 
            int? offset = 0, OrderType orderType = OrderType.ASC) 
        {
            var path = $"/databases/{DatabaseId}/collections";

            var objects = await ListObjects(path, queries, limit, offset, orderType);
            
            var result = await objects.ToObject<QueryCollections>();

            return result;
        }

        /// <summary>
        /// Create Collection
        /// <para>
        /// Create a new Collection.
        /// </para>
        /// </summary>
        public async Task<HttpResponseMessage> CreateCollection(string name, List<object> read, List<object> write, List<object> rules) 
        {
            var path = $"/database/{DatabaseId}/collections";

            Dictionary<string, object> parameters = new Dictionary<string, object>()
            {
                { "name", name },
                { "read", read },
                { "write", write },
                { "rules", rules }
            };

            Dictionary<string, string> headers = JsonHeaders();

            return await _client.Call("POST", path, headers, parameters);
        }

        /// <summary>
        /// Get Collection
        /// <para>
        /// Get a collection by its unique ID. This endpoint response returns a JSON
        /// object with the collection metadata.
        /// </para>
        /// </summary>
        public async Task<HttpResponseMessage> GetCollectionById(string collectionId) 
        {
            var path = $"/database/{DatabaseId}/collections/{collectionId}";

            var parameters = new Dictionary<string, object>() {};

            var headers = JsonHeaders();

            return await _client.Call("GET", path, headers, parameters);
        }

        public async Task<Collection<Type>> GetCollection<Type>(string name)
        {
            //Lookup database
            var result = await ListCollections(new Query().Equal("name", name).BuildUrl());
            
            var collection = result.Collections.FirstOrDefault(d => d.Name == name);

            if (collection == null)
                throw new ArgumentException($"Collection {name} not found.", nameof(name));
            
            return new Collection<Type>(_client, DatabaseId, collection.Id);
        }

        /// <summary>
        /// Update Collection
        /// <para>
        /// Update a collection by its unique ID.
        /// </para>
        /// </summary>
        public async Task<HttpResponseMessage> UpdateCollection(string collectionId, string name, List<object> read = null, List<object> write = null, List<object> rules = null) 
        {
            string path = "/database/collections/{collectionId}".Replace("{collectionId}", collectionId);

            Dictionary<string, object> parameters = new Dictionary<string, object>()
            {
                { "name", name },
                { "read", read },
                { "write", write },
                { "rules", rules }
            };

            Dictionary<string, string> headers = new Dictionary<string, string>()
            {
                { "content-type", "application/json" }
            };

            return await _client.Call("PUT", path, headers, parameters);
        }

        /// <summary>
        /// Delete Collection
        /// <para>
        /// Delete a collection by its unique ID. Only users with write permissions
        /// have access to delete this resource.
        /// </para>
        /// </summary>
        public async Task<HttpResponseMessage> DeleteCollection(string collectionId) 
        {
            string path = "/database/collections/{collectionId}".Replace("{collectionId}", collectionId);

            Dictionary<string, object> parameters = new Dictionary<string, object>()
            {
            };

            Dictionary<string, string> headers = new Dictionary<string, string>()
            {
                { "content-type", "application/json" }
            };

            return await _client.Call("DELETE", path, headers, parameters);
        }


    };


    public class Collection<T> : Service
    {
        public Collection(Client client, string databaseId, string collectionId) : base(client)
        {
            DatabaseId = databaseId;
            CollectionId = collectionId;
        }
        
        public string DatabaseId { get; private set;}
        public string CollectionId { get; private set;}
        
        /// <summary>
        /// List Documents
        /// <para>
        /// Get a list of all the user documents. You can use the query params to
        /// filter your results. On admin mode, this endpoint will return a list of all
        /// of the project's documents. [Learn more about different API
        /// modes](/docs/admin).
        /// </para>
        /// </summary>
        public async Task<HttpResponseMessage> ListDocuments(List<object> filters = null, int? limit = 25, int? offset = 0,
            string orderField = "", OrderType orderType = OrderType.ASC, string orderCast = "string", string search = "") 
        {
            string path = $"/databases/{DatabaseId}/collections/{CollectionId}/documents";

            Dictionary<string, object> parameters = new Dictionary<string, object>()
            {
                { "filters", filters },
                { "limit", limit },
                { "offset", offset },
                { "orderField", orderField },
                { "orderType", orderType.ToString() },
                { "orderCast", orderCast },
                { "search", search }
            };

            Dictionary<string, string> headers = new Dictionary<string, string>()
            {
                { "content-type", "application/json" }
            };

            return await _client.Call("GET", path, headers, parameters);
        }



        public async Task<QueryDocuments<T>> ListDocument()
        {
            var objects = await ListDocuments();
            
            var result = await objects.ToObject<QueryDocuments<T>>();

            return result;
        }

        /// <summary>
        /// Create Document
        /// <para>
        /// Create a new Document. Before using this route, you should create a new
        /// collection resource using either a [server
        /// integration](/docs/server/database#databaseCreateCollection) API or
        /// directly from your database console.
        /// </para>
        /// </summary>
        public async Task<HttpResponseMessage> CreateDocument(object data, 
            List<object> read = null, List<object> write = null, string parentDocument = "", 
            string parentProperty = "", string parentPropertyType = "assign") 
        {
            var path = $"/databases/{DatabaseId}/collections/{CollectionId}/documents";

            var parameters = new Dictionary<string, object>()
            {
                { "data", data },
                { "documentId", "unique()" },
                //{ "read", read },
                //{ "write", write },
                //{ "parentDocument", parentDocument },
                //{ "parentProperty", parentProperty },
                //{ "parentPropertyType", parentPropertyType }
            };

            var headers = JsonHeaders();

            return await _client.Call("POST", path, headers, parameters);
        }

        
        public async Task<T> CreateDocumentT (T data)
        {
            
            // var infos = data.GetType().GetProperties();
            //
            // var dix = new Dictionary<string,object> ();
            //
            // foreach (PropertyInfo info in infos)
            // {
            //     dix.Add(info.Name, info.GetValue(data, null).ToString());
            // }
            
            //var objectData = JsonConvert.SerializeObject(data);

            var result = await CreateDocument(data);

            var content = await result.Content.ReadAsStringAsync();

            return data;
        }


        public async Task<T> GetDocument(string documentId)
        {
            var message = await GetDocumentById(documentId);
            var doc = await message.ToObject<T>();

            return doc;
        }


        /// <summary>
        /// Get Document
        /// <para>
        /// Get a document by its unique ID. This endpoint response returns a JSON
        /// object with the document data.
        /// </para>
        /// </summary>
        public async Task<HttpResponseMessage> GetDocumentById(string documentId) 
        {
            var path = $"/databases/{DatabaseId}/collections/{CollectionId}/documents/{documentId}";

            var parameters = new Dictionary<string, object>()
            {
            };

            var headers = JsonHeaders();

            return await _client.Call("GET", path, headers, parameters);
        }

        /// <summary>
        /// Update Document
        /// <para>
        /// Update a document by its unique ID. Using the patch method you can pass
        /// only specific fields that will get updated.
        /// </para>
        /// </summary>
        public async Task<HttpResponseMessage> UpdateDocument(string collectionId, string documentId, object data, List<object> read = null, List<object> write = null) 
        {
            string path = "/database/collections/{collectionId}/documents/{documentId}".Replace("{collectionId}", collectionId).Replace("{documentId}", documentId);

            Dictionary<string, object> parameters = new Dictionary<string, object>()
            {
                { "data", data },
                { "read", read },
                { "write", write }
            };

            Dictionary<string, string> headers = new Dictionary<string, string>()
            {
                { "content-type", "application/json" }
            };

            return await _client.Call("PATCH", path, headers, parameters);
        }

        /// <summary>
        /// Delete Document
        /// <para>
        /// Delete a document by its unique ID. This endpoint deletes only the parent
        /// documents, its attributes and relations to other documents. Child documents
        /// **will not** be deleted.
        /// </para>
        /// </summary>
        public async Task<HttpResponseMessage> DeleteDocument(string collectionId, string documentId) 
        {
            string path = "/database/collections/{collectionId}/documents/{documentId}".Replace("{collectionId}", collectionId).Replace("{documentId}", documentId);

            Dictionary<string, object> parameters = new Dictionary<string, object>()
            {
            };

            Dictionary<string, string> headers = new Dictionary<string, string>()
            {
                { "content-type", "application/json" }
            };

            return await _client.Call("DELETE", path, headers, parameters);
        }
    }
}