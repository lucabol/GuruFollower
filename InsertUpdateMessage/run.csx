#load "..\shared\UpdateMessage.csx"
using System.Net;

// Queues requests to create portfolios. In theory we should use the location URI to give status of request as below:
// https://www.adayinthelifeof.nl/2011/06/02/asynchronous-operations-in-rest/
public static HttpResponseMessage Run(UpdateData req, out UpdateData portfolio, TraceWriter log) {
    log.Info($"updatedata = {req.collection}:{req.groups}:{req.cik}");

    if (req != null) {
        portfolio = req;
        return new HttpResponseMessage(HttpStatusCode.Accepted);
    }
    else {
        portfolio = null;
        return new HttpResponseMessage(HttpStatusCode.BadRequest);
    }
}