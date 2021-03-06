﻿#r "Microsoft.Azure.Documents.Client"
#r "..\sharedBin\GuruLoader.dll"

using System;
using System.Net;
using Microsoft.Azure.Documents;
using Microsoft.Azure.Documents.Client;

// Fetches details for each guru in the collection. An alternative design is to load all the portfolios upfront on the client at the start of the page instead
// of going back everytime a new portfolio is required. That might not scale well and also you'd need to refresh the whole page to see newly added 13Fs.
public static async Task<HttpResponseMessage> Run(HttpRequestMessage req, string collection, DocumentClient client, TraceWriter log) {
    log.Info($"C# HTTP trigger function processed a request. RequestUri={req.RequestUri}, collection={collection}");
    if (client == null) throw new ArgumentException("client is null");

    var db = (await client.ReadDatabaseFeedAsync()).Single(d => d.Id == "guru-portfolios");
    var col = (await client.ReadDocumentCollectionFeedAsync(db.CollectionsLink)).Single(c => c.Id == collection);
    var docs = client.CreateDocumentQuery<DisplayPortfolio>(col.DocumentsLink).Select(p => new { p.id, p.DisplayName, p.EndQuarterDate });

    return docs == null
        ? req.CreateResponse(HttpStatusCode.BadRequest, $"No hyper-portfolio for collection {collection}")
        : req.CreateResponse(HttpStatusCode.OK, docs);
}