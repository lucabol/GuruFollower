using System;
using System.Collections.Generic;
using System.IO;
using Xunit;
using System.Linq;
using System.Reflection;

/*
 * Tests should be improved. They just test general properties of files, instead of lower granularity facts
 * (i.e. did I get this *exact* number).
 */

public class Tests {

    // Generators to create test data from the files on disk
    public static IEnumerable<object[]> GetFileWithExtension(string ext) {
        var baseDir = @"..\..";// AppContext.BaseDirectory;
        var fileDir = Path.Combine(baseDir, @"TestData\");
        return Directory.GetFiles(fileDir, ext)
                        .Select(path => new object[] { path });
    }

    public static IEnumerable<object[]> GetRssFiles() => GetFileWithExtension("*.rss");
    public static IEnumerable<object[]> GetHtmFiles() => GetFileWithExtension("*.htm");
    public static IEnumerable<object[]> GetTxtFiles() => GetFileWithExtension("*.txt");

    [Theory, MemberData(nameof(GetHtmFiles))]
    public void ParseHtmFileTests(string fileFullName) {
        var htmStream = File.OpenRead(fileFullName);
        var link = GuruLoader.ParseHtmFile(htmStream);
        Assert.False(String.IsNullOrEmpty(link));
        Assert.Equal(Path.GetExtension(link), ".txt");
    }

    [Theory, MemberData(nameof(GetRssFiles))]
    public void ParseRssTextTest(string fileFullName) {
        var rssStream = File.OpenRead(fileFullName);
        var res = GuruLoader.ParseRssText(rssStream);
        Assert.NotNull(res);
        Assert.False(String.IsNullOrEmpty(res.DisplayName));
        Assert.NotEmpty(res.Links);

        Assert.All(res.Links, link => Assert.True(Path.GetExtension(link) == ".htm" ||
                                                  Path.GetExtension(link) == ".html"));
    }

    [Theory, MemberData(nameof(GetTxtFiles))]
    public void ParseSubmissionFileTests(string fileFullName) {
        var txtStream = File.OpenRead(fileFullName);
        var port = GuruLoader.ParseSubmissionFile(txtStream);
        Assert.NotNull(port);
        Assert.True(port.PositionsNumber > 0);
        Assert.True(port.TotalValue > 0);
        Assert.NotEmpty(port.Positions);
        Assert.All(port.Positions, pos => {
            Assert.False(String.IsNullOrEmpty(pos.Cusip));
            Assert.False(String.IsNullOrEmpty(pos.Name));
            Assert.True(pos.Shares > 0);
            Assert.True(pos.Value > 0);
        });
    }

    static bool isSimilar(double expected, double actual) {
        var eps = 0.001;
        return actual < expected + eps && actual > expected - eps;
    }

    // This could be made more generic by reading from a cvs file with more info to check.
    // As it is it checks that one particular position (cusip) has changed has expected, one is new and another is sold
    [Theory,
        InlineData("Arlington1.txt", "Arlington2.txt", "N20146101", -0.1697, "949746101", null),
        InlineData("BraveWarrior1.txt", "BraveWarrior2.txt", "03674x106", 1.2081, "g5480u138", "91911k102")]
    public void CreateDisplayPortfolioTests(string newPort, string oldPort, string cusipChanged, double change, string cusipNew, string cusipSold) {

        var baseDir = @"..\..";//AppContext.BaseDirectory;
        var filePath = Path.Combine(baseDir, @"TestData\");

        var txtStream1 = File.OpenRead(Path.Combine(filePath, newPort));
        var port1 = GuruLoader.ParseSubmissionFile(txtStream1);

        var txtStream2 = File.OpenRead(Path.Combine(filePath, oldPort));
        var port2 = GuruLoader.ParseSubmissionFile(txtStream2);

        var dp = GuruLoader.CreateDisplayPortfolio("name", port1, port2);

        if (cusipChanged != null) Assert.True(isSimilar(change, dp.Positions.First(p => p.Cusip == cusipChanged).Change));
        if (cusipNew != null) Assert.True(dp.Positions.First(p => p.Cusip == cusipNew).IsNew);
        if (cusipSold != null) Assert.True(dp.Positions.First(p => p.Cusip == cusipSold).IsSold);
    }
}

