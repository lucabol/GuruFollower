using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Text;

public class Program {

    // Quick and dirty Console Printing
    static string cs(string s, int len) => s == null ? "".PadRight(len, ' ') : s.PadRight(len, ' ');
    static string def(bool v, string s) => v ? s : "    ";

    static string DisplayPortToString(DisplayPortfolio dp) {
        var sb = new StringBuilder();
        sb.AppendLine(String.Join(" -- ", dp.DisplayName, dp.EndQuarterDate.ToString("d"), dp.TotalValue, dp.PositionsNumber));
        sb.AppendLine(String.Join(" ", "NEW ", "SOLD", cs("NAME", 40), cs("P/C", 5), cs("SHARES", 10),
                                    cs("VALUE", 10), "%PORT ", "CHANGE", "PRICE ", "DISCR"));
        foreach (var p in dp.Positions) {
            sb.AppendLine(String.Join(" ", def(p.IsNew, "NEW "), def(p.IsSold, "SOLD"), cs(p.Name.Trim(), 40), cs(p.PutCall, 5), cs(p.Shares.ToString(), 10),
                                    cs(p.Value.ToString(), 10), cs(Math.Round(p.PercOfPortfolio * 100, 2).ToString(), 6),
                                    cs(Math.Round(p.Change * 100, 2).ToString(), 6), cs(Math.Round(p.Price, 2).ToString(), 6), cs(p.Discretion.ToString(), 10)));
        }
        return sb.ToString();
    }

    static string DisplayHyperPortToString(HyperPortfolio hp) {
        var sb = new StringBuilder();
        sb.AppendLine(String.Join(" -- ", hp.EndQuarterDate.ToString("d"), hp.NumberOfGurus));
        sb.AppendLine(String.Join(" ", cs("NAME", 40), cs("P/C", 5), "%PORT ", "OWN ", "BUY ", "SELL ", "GURUS"));
        foreach (var p in hp.Positions) {
            sb.AppendLine(String.Join(" ", cs(p.Name.Trim(), 40), cs(p.PutCall, 5), cs(Math.Round(p.PercOfPortfolio * 100, 2).ToString(), 6),
                cs(p.NumberGurusOwning.ToString(), 4), cs(p.NumberGurusBuying.ToString(), 4), cs(p.NumberGurusSelling.ToString(), 4),
                cs(String.Join(",", p.Gurus), 30)));
        }
        return sb.ToString();
    }

    static string DisplayHistory(IEnumerable<DisplayCompany> companies) {
        var sb = new StringBuilder();
        foreach (var c in companies) {
            sb.AppendLine(String.Join(" ", cs(c.Name.Trim(), 40), cs(c.PutCall, 5)));
            foreach (var p in c.ChangePositions) {
                sb.AppendLine(String.Join(" ", "\t", p.Date.ToString("d"), def(p.IsNew, "NEW "), def(p.IsSold, "SOLD"), cs(p.Shares.ToString(), 10),
                            cs(p.Value.ToString(), 10), cs(Math.Round(p.PercOfPortfolio * 100, 2).ToString(), 6),
                            cs(Math.Round(p.Change * 100, 2).ToString(), 6), cs(Math.Round(p.Price, 2).ToString(), 6)));
            }
        }
        return sb.ToString();
    }

    static IEnumerable<Tuple<string, double>> LoadCIKWeightFile(string path) {
        return System.IO.File.ReadAllLines(path)
            .Select(line => line.Split(new char[] { ',' }))
            .Select(arr => Tuple.Create(arr[0].Trim(), double.Parse(arr[1])));
    }

    static void Banner() {
        var banner = "ERRROR!\nUsage: Follower [Cik,file] [-Hist, -Hyper]";
        Console.Write(banner);
        Environment.Exit(-1);
    }
    // Try with the following ciks: 0001553733, 0001568820, 0001484148, 0001112520
    // or go to https://www.sec.gov/edgar/searchedgar/companysearch.html and put the name of the investor you are interested in
    // TODO: clean up cmd line definition and code to manage it.
    public static void Main(string[] args) {
        if (args.Count() > 2 || args.Count() == 0) Banner();
        if (args.Count() == 2 && (args[1] != "-Hist" && args[1] != "-Hyper")) Banner();

        if (args.Count() == 1) {
            var result = GuruLoader.FetchDisplayPortfolioAsync(args[0]).Result;
            Console.WriteLine(DisplayPortToString(result));
        }
        else if (args[1] == "-Hist") {
            // Printing Portfolio summary at both start and bottom
            var result = GuruLoader.FetchFullPortfolioDataAsync(args[0]).Result;
            Console.WriteLine(DisplayPortToString(result.Portfolio));
            Console.WriteLine(DisplayHistory(result.CompaniesHistory));
        }
        else {
            var cw = LoadCIKWeightFile(args[0]);
            var result = GuruLoader.FetchHyperPortfolioAsync(cw).Result;
            Console.WriteLine(DisplayHyperPortToString(result));
        }

    }
}
