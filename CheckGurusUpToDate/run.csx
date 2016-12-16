#r "..\sharedBin\GuruLoader.dll"
#r "Microsoft.Azure.Documents.Client"
#load "..\shared\UpdateMessage.csx"

using System;
using Microsoft.Azure.Documents;
using Microsoft.Azure.Documents.Client;
using System.Threading.Tasks;

// Check that the date that we have in the database is the same as the last date of the filings on the SEC website, if not queue a request to update
public async static Task Run(TimerInfo myTimer, DocumentClient client, IAsyncCollector<UpdateData> messages, TraceWriter log) {
    log.Info($"C# Timer trigger function executed at: {DateTime.Now}");

    if (client == null) throw new Exception("Error documentDB handle is null");
    if (messages == null) throw new Exception("The update message queue is null");

    // TODO: rewrite to perform less queries. Now it queries once for the list of collections and one time for each collection. Perhaps it could be done with 1 or 2 queries
    Database database = client.CreateDatabaseQuery(@"SELECT * FROM d WHERE d.id = ""guru-portfolios""").AsEnumerable().First();
    List<DocumentCollection> collections = client.CreateDocumentCollectionQuery((String)database.SelfLink).ToList();

    // For each collection generates (colId, cik, quarterEndDateInDB)
    var colIdDates = collections
                    .Select(col => Tuple.Create(col.Id, col.DocumentsLink))
                    .SelectMany(tup => {
                        var docs = client.CreateDocumentQuery(tup.Item2).ToList();
                        //var idDates = docs.Select(d => Tuple.Create(tup.Item1, ((dynamic)d).id, ((dynamic)d).EndQuarterDate)).ToArray();
                        var idDates = docs.Select(d => { dynamic d1 = d; return Tuple.Create((string)tup.Item1, (string)d1.id, (DateTime)d1.EndQuarterDate); });
                        return idDates;
                    });

    // Generates asynchronously the dates for the last 13F stored on the SEC site
    var secEndTasks = colIdDates
                      .Select(tup => tup.Item2)
                      .Select(cik => GuruLoader.FetchLastQuarterDate(cik))
                      .ToArray();
    var secEndDates = await Task.WhenAll(secEndTasks);

    // If the date in the database is less then the date on the SEC site, then we need to update the portfolio in the database
    var mess = colIdDates
                   .Zip(secEndDates, (tup, secDate) => tup.Item3 < secDate ? new UpdateData { collection = tup.Item1, groups = null, cik = tup.Item2 } : null)
                   .Where(o => o != null);

    // We do that by adding messages to the queue
    foreach (var m in mess) {
        log.Info($"Updated {m.collection}:{m.groups}:{m.cik}");
        await messages.AddAsync(m);
    }
    
}