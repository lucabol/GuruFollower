#r "Microsoft.Azure.Documents.Client"
#r "..\sharedBin\GuruLoader.dll"
#load "..\shared\UpdateMessage.csx"

using System;
using Microsoft.Azure.Documents;
using Microsoft.Azure.Documents.Client;

// Monitors the queue, when a new request for update arrives, it fetches the right portfolio and updates the DB
public async static Task<DisplayPortfolio> Run(UpdateData req, DocumentClient client, TraceWriter log) {

    log.Info($"updatedata = {req.collection}:{req.groups}:{req.cik}: {req.remove}");
    if (req == null) throw new ArgumentException("req is null");
    if (client == null) throw new ArgumentException("client is null");

    if (!req.remove) {
        // Add can be implemented through binding by returning the portfolio to add
        DisplayPortfolio port;

        port = await GuruLoader.FetchDisplayPortfolioAsync(req.cik);
        port.id = req.cik;
        port.groups = req.groups;

        return port;
    }
    else {
        // Remove can't be implemented in binding
        // TODO: find a way to remove guru-portfolios name for below
        Uri docUri = UriFactory.CreateDocumentUri("guru-portfolios", req.collection, req.cik);
        await client.DeleteDocumentAsync(docUri);
        return null;
    }
}