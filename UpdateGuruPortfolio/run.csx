#r "..\sharedBin\GuruLoader.dll"
#load "..\shared\UpdateMessage.csx"
using System;

public async static Task<DisplayPortfolio> Run(UpdateData req, TraceWriter log) {

    log.Info($"updatedata = {req.collection}:{req.groups}:{req.cik}");
    if (req != null) {
        DisplayPortfolio port;

        port = await GuruLoader.FetchDisplayPortfolioAsync(req.cik);
        port.id = req.cik;
        port.groups = req.groups;

        return port;
    }
    else
        return null;

}