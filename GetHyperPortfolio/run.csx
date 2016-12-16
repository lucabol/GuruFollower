#r "Microsoft.Azure.Documents.Client"
#r "Newtonsoft.Json"
#r "..\sharedBin\GuruLoader.dll"

using System;
using System.Net;
using Newtonsoft.Json;
using Microsoft.Azure.Documents;
using Microsoft.Azure.Documents.Client;

// Fetches the hyper-portfolio for a collection
public static async Task<HttpResponseMessage> Run(HttpRequestMessage req, string collection, DocumentClient client, TraceWriter log)
{
    log.Info($"C# HTTP trigger function processed a request. RequestUri={req.RequestUri}, collection={collection}");
    if (client == null) throw new ArgumentException("client is null");

    // Get all ports in a collection
    var db = (await client.ReadDatabaseFeedAsync()).Single(d => d.Id == "guru-portfolios");
    var col = (await client.ReadDocumentCollectionFeedAsync(db.CollectionsLink)).Single(c => c.Id == collection);
    var docs = client.CreateDocumentQuery(col.DocumentsLink).ToList();

    var portfolios = docs.Select(doc => JsonConvert.DeserializeObject<DisplayPortfolio>(doc.ToString()));
    // Set all weights to 1 for now
    var portsAndWeights = portfolios.Select(port => Tuple.Create(port, 1.0));

    var hyper = GuruLoader.CreateHyperPortfolio(portsAndWeights);

    return hyper == null
        ? req.CreateResponse(HttpStatusCode.BadRequest, $"No hyper-portfolio for collection {collection}")
        : req.CreateResponse(HttpStatusCode.OK, JsonConvert.SerializeObject(hyper));
}