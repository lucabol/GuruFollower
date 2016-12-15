#r "..\sharedBin\GuruLoader46.dll"
#load "..\shared\UpdateMessage.csx"
using System;

public async static Task<DisplayPortfolio> Run(UpdateData req, TraceWriter log) {

    log.Info($"updatedata = {req.collection}:{req.groups}:{req.cik}");
    if (req != null) {
        DisplayPortfolio port;

        port = await GuruLoader.FetchDisplayPortfolioAsync(req.cik);
        log.Info(port.DisplayName);
        log.Info(port.TotalValue.ToString());
        log.Info(port.PositionsNumber.ToString());
        log.Info(port.Positions.Count().ToString());
        port.id = req.cik;
        port.groups = req.groups;

        return port;
    }
    else
        return null;

}