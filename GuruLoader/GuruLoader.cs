using HtmlAgilityPack;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Linq;


/*
 * This file contains utility functions to parse SEC filings to get out a portfolio at a particular date for a particular investor.
 *  Investors are identified by a CIK
 *  Given a CIK, you can get to an RSS feed that contains all filing for that investor
 *  Given the RSS feed, you can scan it to get links for all the 13F filings that represent the portfolio at a particular date.
 *  Given a link to a 13F filing, you need to scan the resulting HTML file for a link to the full submission file
 *  Given the full submission file, you need to scan it to extract the entries for each position in the portfolio
 *  Given the last two portfolios, you can then diff them to find the changes and return a summary of the portfolio and changes
 * The code below implements the above workflow as granular functions so that can be reused in different architecture (i.e. one actor for each portfolio
 *   coarse grain web service, other)
 */

// A position as represented in the SEC filing (not good for direct display)
public class Position {

    public string Name { get; set; }
    public string ClassTitle { get; set; }
    public string Cusip { get; set; }
    public int Value { get; set; }
    public int Shares { get; set; }
    public string SharesType { get; set; }

    public string PutCall { get; set; }
    public double Price { get; set; }
    public string Discretion { get; set; }

}

// A portfolio as represented in the SEC filing
public class Portfolio {

    public int TotalValue { get; set; }
    public int PositionsNumber { get; set; }
    public DateTime EndQuarterDate { get; set; }
    public IEnumerable<Position> Positions { get; set; }
}

// Representation of the RSS file containing links to all 13F filings
public class RssData {
    public string DisplayName { get; set; }
    public IEnumerable<string> Links { get; set; }
}

// A position in a display friendly form (i.e. directly displayable)
// It adds to what is in the SEC filings data about changes in positions and percentage of portfolio
public class DisplayPosition {
    public string Name { get; set; }
    public string ClassTitle { get; set; }
    public string Cusip { get; set; }
    public int Value { get; set; }
    public int Shares { get; set; }
    public string PutCall { get; set; }
    public double Change { get; set; }
    public double PercOfPortfolio { get; set; }
    public bool IsNew { get; set; }
    public bool IsSold { get; set; }
    public double Price { get; set; }
    public string Discretion { get; set; }
}

// A displayable portfolio
public class DisplayPortfolio {

    public string id { get; set; }
    public string[] groups { get; set; }

    public string DisplayName { get; set; }
    public int TotalValue { get; set; }
    public int PositionsNumber { get; set; }
    public DateTime EndQuarterDate { get; set; }
    public IEnumerable<DisplayPosition> Positions { get; set; }
}

public class HyperPosition {
    public string Name { get; set; }
    public string ClassTitle { get; set; }
    public string Cusip { get; set; }
    public string PutCall { get; set; }
    public double PercOfPortfolio { get; set; }
    public int NumberGurusOwning { get; set; }
    public int NumberGurusBuying { get; set; }
    public int NumberGurusSelling { get; set; }
    public IList<string> Gurus { get; set; }
}

public class HyperPortfolio {
    public DateTime EndQuarterDate { get; set; }
    public int NumberOfGurus { get; set; }
    public IEnumerable<HyperPosition> Positions { get; set; }

}

public class DisplayChangePosition {
    public int Value { get; set; }
    public int Shares { get; set; }
    public double Change { get; set; }
    public double PercOfPortfolio { get; set; }
    public bool IsNew { get; set; }
    public bool IsSold { get; set; }
    public double Price { get; set; }
    public DateTime Date { get; set; }

}

public class DisplayCompany {
    public string Name { get; set; }
    public string ClassTitle { get; set; }
    public string Cusip { get; set; }
    public string PutCall { get; set; }
    public IList<DisplayChangePosition> ChangePositions { get; set; }

}

public class FullPortfolioData {
    public DisplayPortfolio Portfolio { get; set; }
    public IEnumerable<DisplayCompany> CompaniesHistory { get; set; }
}
public static class GuruLoader {
    // Parse the rss stream into the displayable name of the guru and the links
    // to the html file pointing to data on the portfolio
    public static RssData ParseRssText(Stream rssStream) {
        var xml = XDocument.Load(rssStream);
        XNamespace xs = "http://www.w3.org/2005/Atom";
        var name = xml
                   .Descendants(xs + "company-info")
                   .First()
                   .Element(xs + "conformed-name").Value;

        var links = from feed in xml.Descendants(xs + "entry")
                    where feed.Element(xs + "content").Element(xs + "filing-type").Value == "13F-HR"
                    select feed.Element(xs + "link").Attribute("href").Value;

        return new RssData { DisplayName = name, Links = links };
    }

    // Parse the html file pointed to by the rss stream to extract the name of the text file
    // containing date of filing, date of portfolio and positions
    public static string ParseHtmFile(Stream htmlStream) {
        var html = new HtmlDocument();
        html.Load(htmlStream);
        return html.DocumentNode.Descendants("tr")
                      .Where(tr => tr.Descendants("td").Any(td => td.InnerText == "Complete submission text file"))
                      .Single()
                      .Descendants("a")
                      .Single()
                      .GetAttributeValue("href", "NO LINK FOUND");
    }

    // Given CIK, gets the link for the RSS file
    public static string ComposeGuruUrl(string cik)
        => $"https://www.sec.gov/cgi-bin/browse-edgar?action=getcompany&CIK={cik}&CIK=0001568820&type=&dateb=&owner=exclude&start=0&count=40&output=atom";
    // Links in the submission file happen to be relative, this absolutes them. Brittle, but if the below change, the world might come to end
    // as everybody and his brother has links to it
    public static string MakeSecLinkAbsolute(string relUrl) => $"https://www.sec.gov{relUrl}";

    // The submission file is divided in two parts, the first contains investor related data, the second the portfolio for that date
    // TODO: ubrittle this a bit more
    static Tuple<XDocument, XDocument> SplitSubmissionFile(Stream submissionFile) {
        using (var reader = new StreamReader(submissionFile, Encoding.UTF8)) {
            var fullTxt = reader.ReadToEnd();
            var startFirstDoc = fullTxt.IndexOf("<?xml");
            var endFirstDoc = fullTxt.IndexOf("</XML>");
            var firstDoc = fullTxt.Substring(startFirstDoc, endFirstDoc - startFirstDoc);
            var firstXml = XDocument.Parse(firstDoc);

            var startSecondDoc = fullTxt.IndexOf("<XML>", endFirstDoc) + "<XML>".Length + 1;
            var endSecondDoc = fullTxt.IndexOf("</XML>", startSecondDoc);

            var secondDoc = fullTxt.Substring(startSecondDoc, endSecondDoc - startSecondDoc).Trim();
            var secondXml = XDocument.Parse(secondDoc);
            return Tuple.Create(firstXml, secondXml);
        }
    }

    // gets a portfolio out of a submission file
    public static Portfolio ParseSubmissionFile(Stream submissionFile) {
        var xmls = SplitSubmissionFile(submissionFile);
        var firstXml = xmls.Item1;
        var secondXml = xmls.Item2;

        XNamespace xs = "http://www.sec.gov/edgar/thirteenffiler";
        var endQuarterDate = DateTime.Parse(firstXml.Descendants(xs + "reportCalendarOrQuarter").Single().Value);
        var totalValue = int.Parse(firstXml.Descendants(xs + "tableValueTotal").Single().Value);
        var positionsNumber = int.Parse(firstXml.Descendants(xs + "tableEntryTotal").Single().Value);

        XNamespace ns = "http://www.sec.gov/edgar/document/thirteenf/informationtable";

        var positions = PositionsFromXml(secondXml);

        return new Portfolio {
            EndQuarterDate = endQuarterDate,
            TotalValue = totalValue,
            PositionsNumber = positionsNumber,
            Positions = positions
        };

    }

    // Gets all positions stored in the xml portfolio part of the xml file
    static IEnumerable<Position> PositionsFromXml(XDocument xml) {

        XNamespace xs = "http://www.sec.gov/edgar/document/thirteenf/informationtable";

        return from it in xml.Descendants(xs + "infoTable")
               select new Position {
                   Name = it.Element(xs + "nameOfIssuer").Value,
                   ClassTitle = it.Element(xs + "titleOfClass").Value,
                   Discretion = it.Element(xs + "investmentDiscretion").Value,
                   Cusip = it.Element(xs + "cusip").Value,
                   Value = int.Parse(it.Element(xs + "value").Value),
                   Shares = int.Parse(it.Element(xs + "shrsOrPrnAmt").Element(xs + "sshPrnamt").Value),
                   SharesType = it.Element(xs + "shrsOrPrnAmt").Element(xs + "sshPrnamtType").Value,
                   PutCall = it.Element(xs + "putCall")?.Value,
                   Price = double.Parse(it.Element(xs + "value").Value) * 1000.0 / double.Parse(it.Element(xs + "shrsOrPrnAmt").Element(xs + "sshPrnamt").Value)
               };
    }

    // Enriches the position type for easy display
    static DisplayPosition DisplayFromPosition(Position p) {
        return new DisplayPosition() {
            Name = p.Name,
            ClassTitle = p.ClassTitle,
            Shares = p.Shares,
            Cusip = p.Cusip,
            Value = p.Value,
            PutCall = p.PutCall,
            Price = p.Price,
            Discretion = p.Discretion
        };
    }

    // Uniquely identify the position
    // btw: shoulld Discretion really be part of the key? Are they separate positions in the rare case they have separate discretion?
    static string FormKey(Position p) => p.Cusip + p.ClassTitle + p.PutCall + p.Discretion;
    static string FormKeyD(DisplayPosition p) => p.Cusip + p.ClassTitle + p.PutCall + p.Discretion;
    // Discretion doesn't make sense for hyper portfolios as we are aggregating over all positions in individual portfolios
    static string FormHyperKey(DisplayPosition p) => p.Cusip + p.PutCall;

    static Dictionary<string, Position> CreatePositionsDictionary(IEnumerable<Position> poss) {
        var d = new Dictionary<string, Position>();
        foreach (var p in poss) {
            Position i;
            var key = FormKey(p);
            if (d.TryGetValue(key, out i)) {
                i.Shares += p.Shares;
                i.Value += p.Value;
            }
            else {
                d[key] = p;
            }
        }
        return d;
    }

    // Diffs two portfolios and figure out what changed, this could perhaps be written more functionally
    public static DisplayPortfolio CreateDisplayPortfolio(string displayName, Portfolio newPort, Portfolio oldPort) {

        var positions = new List<DisplayPosition>();
        Dictionary<string, Position> oldPositions = CreatePositionsDictionary(oldPort.Positions);

        // Process existing positions
        foreach (var pn in newPort.Positions) {
            var dp = DisplayFromPosition(pn);
            dp.PercOfPortfolio = (double)dp.Value / (double)newPort.TotalValue;

            Position oldPos;
            if (oldPositions.TryGetValue(FormKey(pn), out oldPos)) {
                dp.Change = (double)dp.Shares / (double)oldPos.Shares - 1;
                dp.IsNew = false;
                oldPositions.Remove(FormKey(pn)); // remove all positions that are still there so it leaves just the ones that have been sold
            }
            else {
                dp.Change = 0; // new position, not there in the old portfolio
                dp.IsNew = true;
            }
            dp.IsSold = false;
            positions.Add(dp);
        }

        // Process sold positions
        foreach (var sold in oldPositions.Values) {
            var dp = DisplayFromPosition(sold);
            dp.Shares = 0;
            dp.Value = 0;
            dp.PercOfPortfolio = 0;
            dp.IsNew = false;
            dp.Change = -1;
            dp.IsSold = true;
            positions.Add(dp);
        }

        // Eye candy
        var sortedPos = positions.OrderByDescending(pos => pos.Value);

        return new DisplayPortfolio() {
            DisplayName = displayName,
            EndQuarterDate = newPort.EndQuarterDate,
            TotalValue = newPort.TotalValue,
            PositionsNumber = newPort.PositionsNumber,
            Positions = sortedPos
        };
    }

    // Creates an hyperportfolio from a list of portfolios and weights.
    // Weight represents how much to weight the portfolio in the hyper portfolio
    public static HyperPortfolio CreateHyperPortfolio(IEnumerable<Tuple<DisplayPortfolio, double>> ports) {
        var lastDate = ports.Max(p => p.Item1.EndQuarterDate);
        var tupleLastDate = ports.Where(p => p.Item1.EndQuarterDate == lastDate);
        var positions = new Dictionary<string, HyperPosition>();
        var totalWeight = 0.0; // Aggregated weight used to normalize end result to 100%

        foreach (var t in tupleLastDate) {
            var port = t.Item1;
            var weight = t.Item2;
            foreach (var pos in port.Positions) {
                HyperPosition hp;
                var relativeWeight = pos.PercOfPortfolio * weight;
                totalWeight += relativeWeight;
                var key = FormHyperKey(pos);
                if (positions.TryGetValue(key, out hp)) { // Already owned
                    hp.Gurus.Add(port.DisplayName);
                    hp.NumberGurusOwning += 1;
                    hp.PercOfPortfolio += relativeWeight;
                    if (pos.Change > 0) hp.NumberGurusBuying += 1;
                    if (pos.Change < 0) hp.NumberGurusSelling += 1;
                }
                else { // not yet owned
                    hp = new HyperPosition {
                        Cusip = pos.Cusip,
                        ClassTitle = pos.ClassTitle,
                        Name = pos.Name,
                        PutCall = pos.PutCall,
                        NumberGurusBuying = pos.Change > 0 ? 1 : 0,
                        NumberGurusSelling = pos.Change < 0 ? 1 : 0,
                        NumberGurusOwning = 1,
                        PercOfPortfolio = relativeWeight,
                        Gurus = new List<string>()
                    };
                    hp.Gurus.Add(port.DisplayName);
                    positions[key] = hp;
                }
            }
        }
        var resPositions = positions.Values;
        // Normalize weights, I am sure there is a way to do it without another pass over the positions, but small numbers here
        foreach (var hp in resPositions) {
            hp.PercOfPortfolio /= totalWeight;
        }

        // Check normalization is effective
        Debug.Assert(resPositions.Sum(p => p.PercOfPortfolio) < 1.01 && resPositions.Sum(p => p.PercOfPortfolio) > 0.99);
        // eye candy
        var sortedRes = resPositions.OrderByDescending(hp => hp.PercOfPortfolio);

        return new HyperPortfolio { EndQuarterDate = lastDate, NumberOfGurus = tupleLastDate.Count(), Positions = sortedRes };
    }

    // Takes a list of portfolios and returns the history of all the positions of the *most recent* portfolio
    public static IEnumerable<DisplayCompany> CreateDisplayCompanies(IEnumerable<DisplayPortfolio> ports) {
        // Could assume already sorted, but there aren't going to be a lot of them, so it's ok to sort
        var sortedPort = ports.OrderByDescending(p => p.EndQuarterDate);

        var recentCompanies = new Dictionary<string, DisplayCompany>();
        foreach (var p in ports.First().Positions)
            recentCompanies.Add(FormKeyD(p), new DisplayCompany {
                Name = p.Name, Cusip = p.Cusip, ClassTitle = p.ClassTitle, PutCall = p.PutCall, ChangePositions = new List<DisplayChangePosition>()
            });

        foreach (var port in ports) {
            foreach (var pos in port.Positions) {
                DisplayCompany dp;
                if (recentCompanies.TryGetValue(FormKeyD(pos), out dp))
                    dp.ChangePositions.Add(new DisplayChangePosition {
                        Change = pos.Change, IsNew = pos.IsNew, IsSold = pos.IsSold, Shares = pos.Shares,
                        Value = pos.Value, Price = pos.Price, PercOfPortfolio = pos.PercOfPortfolio,
                        Date = port.EndQuarterDate
                    });
            }
        }
        return recentCompanies.Values;
    }

    // Utility function to help waiting for all portfolios to be loaded
    async static Task<Portfolio> FetchPortfolio(HttpClient client, string htmlLink) {
        try {
            using (var html = await client.GetStreamAsync(htmlLink)) {
                var submissionLink = ParseHtmFile(html);
                using (var submissionStream = await client.GetStreamAsync(MakeSecLinkAbsolute(submissionLink))) {
                    return ParseSubmissionFile(submissionStream);
                }
            }
        }
        catch (Exception e) {
            if (e.GetType() != typeof(ArgumentOutOfRangeException)) Console.WriteLine(e);
            // Before a certain date (2013) the format for 13F was different, not XML, hence the code will throw
            // a different exception depending on the particular format of the text file read.
            // It is unclear if it's of any interest to look so far back in the past.
            // Until I implement support for such old files, I construct an empty portfolio in such cases
            // TODO: fix the code so that doesn't swallow exceptions
            return new Portfolio {
                EndQuarterDate = new DateTime(), Positions = new List<Position>(), PositionsNumber = 101, TotalValue = 101
            };
        }
    }

    static async Task<RssData> FetchRssDataAsync(HttpClient client, string cik) {
        if (String.IsNullOrEmpty(cik)) throw new Exception("Cik cannot be empty");

        var rssUrl = ComposeGuruUrl(cik);
        // Getting the rss stream cannot be started in parallel as it needs to be read before loading the portfolios
        var rss = await client.GetStreamAsync(rssUrl);
        var rssData = ParseRssText(rss);

        var portsNumber = rssData.Links.Count();
        if (portsNumber == 0) throw new Exception("No portfolios for this cik");
        return rssData;

    }

    public static async Task<DateTime> FetchLastQuarterDate(string cik) {
        using (var client = new HttpClient()) {
            var rssData = await FetchRssDataAsync(client, cik);
            var port = await FetchPortfolio(client, rssData.Links.First());
            return port.EndQuarterDate;
        }
    }

    public async static Task<DisplayPortfolio> FetchDisplayPortfolioAsync(string cik) {
        using (var client = new HttpClient()) {
            // Getting the rss stream cannot be started in parallel as it needs to be read before loading the portfolios
            var rssData = await FetchRssDataAsync(client, cik);
            var port1 = FetchPortfolio(client, rssData.Links.First());

            // If there is just one portfolio (i.e. investor just started investing) create an empty old one so that the logic
            // populates a displayPortfolio where all the positions are marked as new
            var port2 = rssData.Links.Count() == 1 ? Task.FromResult(new Portfolio() { Positions = new Position[] { } }) : FetchPortfolio(client, rssData.Links.ElementAt(1));

            var portfolios = await Task.WhenAll(new Task<Portfolio>[] { port1, port2 });

            return CreateDisplayPortfolio(rssData.DisplayName, portfolios[0], portfolios[1]);
        }
    }

    public async static Task<FullPortfolioData> FetchFullPortfolioDataAsync(string cik) {
        using (var client = new HttpClient()) {
            // Getting the rss stream cannot be started in parallel as it needs to be read before loading the portfolios

            var rssData = await FetchRssDataAsync(client, cik);
            var ports = rssData.Links.Select(l => FetchPortfolio(client, l));
            var portfolios = await Task.WhenAll(ports);

            var pairs = portfolios.Zip(portfolios.Skip(1), (a, b) => Tuple.Create(a, b));

            var dps = pairs.Select(t => CreateDisplayPortfolio(rssData.DisplayName, t.Item1, t.Item2));
            return new FullPortfolioData {
                Portfolio = dps.First(),
                CompaniesHistory = CreateDisplayCompanies(dps)
            };
        }
    }

    public async static Task<HyperPortfolio> FetchHyperPortfolioAsync(IEnumerable<Tuple<string, double>> cikAndWeights) {
        var displayPortsTasks = cikAndWeights.Select(cw => FetchDisplayPortfolioAsync(cw.Item1)).ToArray();
        var displayPorts = await Task.WhenAll(displayPortsTasks);
        var portsAndWeights = displayPorts.Zip(cikAndWeights, (dp, cw) => Tuple.Create(dp, cw.Item2));
        return GuruLoader.CreateHyperPortfolio(portsAndWeights);
    }
}


