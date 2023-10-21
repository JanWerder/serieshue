

using System.Diagnostics;
using serieshue.Interfaces;
using serieshue.Models;
using System.Net;
using LinearTsvParser;
using System.IO.Compression;
using System.Collections.Generic;
using EFCore.BulkExtensions;
using System.Globalization;
using System.Data.SqlClient;
using NSonic;
using System.Text.Json;
using Nager.Country;
using Microsoft.EntityFrameworkCore.Internal;

namespace serieshue.Services;

public class TaskRunnerService : ITaskRunnerService
{

    private readonly SeriesHueContext _context;

    private readonly IConfiguration _configuration;

    private string SonicHost;

    private int SonicPort;

    private string SonicSecret;

    public TaskRunnerService(SeriesHueContext context, IConfiguration config)
    {
        _context = context;
        _configuration = config;

        SonicHost = _configuration.GetValue<string>("ConnectionStrings:SonicHost");
        SonicPort = Int32.Parse(_configuration.GetValue<string>("ConnectionStrings:SonicPort"));
        SonicSecret = _configuration.GetValue<string>("ConnectionStrings:SonicSecret");
    }

    public void RunIMDBUpdate()
    {
        using (WebClient webClient = new WebClient())
        {
            var startTime = DateTime.Now;

            var titles = new List<Title>();
            var episodes = new List<Episode>();
            var episodeLookup = new List<EpisodeDAO>();
            var ratingLookup = new List<RatingDAO>();

            var basicStream = new System.IO.MemoryStream(webClient.DownloadData("https://datasets.imdbws.com/title.basics.tsv.gz"));
            var ratingsStream = new System.IO.MemoryStream(webClient.DownloadData("https://datasets.imdbws.com/title.ratings.tsv.gz"));
            var episodeStream = new System.IO.MemoryStream(webClient.DownloadData("https://datasets.imdbws.com/title.episode.tsv.gz"));

            using var ungzippedRatings = new GZipStream(ratingsStream, CompressionMode.Decompress);
            var tsvReader = new TsvReader(ungzippedRatings);
            tsvReader.ReadLine();

            Console.WriteLine("Reading ratings... {0}s", (DateTime.Now - startTime).TotalSeconds);

            while (!tsvReader.EndOfStream)
            {
                List<string> fields = tsvReader.ReadLine();

                var rating = new RatingDAO();
                rating.Tconst = fields[0];
                rating.Rating = fields[1] == "\\N" ? 0 : Double.Parse(fields[1], CultureInfo.InvariantCulture);
                rating.Votes = fields[2] == "\\N" ? 0 : int.Parse(fields[2]);

                ratingLookup.Add(rating);
            }

            using var ungzippedBasic = new GZipStream(basicStream, CompressionMode.Decompress);
            tsvReader = new TsvReader(ungzippedBasic);
            tsvReader.ReadLine();

            Console.WriteLine("Reading basics... {0}s", (DateTime.Now - startTime).TotalSeconds);

            while (!tsvReader.EndOfStream)
            {
                List<string> fields = tsvReader.ReadLine();

                if (fields[1] == "tvSeries" || fields[1] == "tvMiniSeries")
                {
                    var title = new Title();
                    title.Tconst = fields[0];
                    title.PrimaryTitle = fields[2];
                    title.OriginalTitle = fields[3];
                    title.IsAdult = fields[4] == "1" ? true : false;
                    title.StartYear = fields[5] == "\\N" ? null : int.Parse(fields[5]);
                    title.EndYear = fields[6] == "\\N" ? null : int.Parse(fields[6]);
                    title.RuntimeMinutes = fields[7] == "\\N" ? null : int.Parse(fields[7]);
                    title.Genres = fields[8] == "\\N" ? null : fields[8];

                    if (title.Genres != null)
                    {
                        title.Genres = title.Genres.Replace(",", ", ");
                    }

                    var ratingIndex = ratingLookup.BinarySearch(new RatingDAO() { Tconst = fields[0] }, new RatingDAOComparer());
                    if (ratingIndex >= 0)
                    {
                        var correspondingRating = ratingLookup[ratingIndex];
                        title.Rating = correspondingRating.Rating;
                        title.Votes = correspondingRating.Votes;
                    }

                    titles.Add(title);
                }
                else if (fields[1] == "tvEpisode")
                {
                    var episode = new EpisodeDAO();
                    episode.Tconst = fields[0];
                    episode.EpisodeTitle = fields[2];
                    episode.RuntimeMinutes = fields[7] == "\\N" ? null : int.Parse(fields[7]);
                    episode.Genres = fields[8] == "\\N" ? null : fields[8];

                    if (episode.Genres != null)
                    {
                        episode.Genres = episode.Genres.Replace(",", ", ");
                    }

                    episodeLookup.Add(episode);
                }
            }

            using var ungzippedEpisodes = new GZipStream(episodeStream, CompressionMode.Decompress);
            tsvReader = new TsvReader(ungzippedEpisodes);
            tsvReader.ReadLine();

            Console.WriteLine("Sorting lookup tables... {0}s", (DateTime.Now - startTime).TotalSeconds);

            ratingLookup.Sort(new RatingDAOComparer());
            episodeLookup.Sort(new EpisodeDAOComparer());
            titles.Sort(new TitleComparer());

            Console.WriteLine("Reading episodes... {0}s", (DateTime.Now - startTime).TotalSeconds);

            while (!tsvReader.EndOfStream)
            {
                List<string> fields = tsvReader.ReadLine();

                var episode = new Episode();

                var ratingIndex = ratingLookup.BinarySearch(new RatingDAO() { Tconst = fields[0] }, new RatingDAOComparer());
                if (ratingIndex >= 0)
                {
                    var correspondingRating = ratingLookup[ratingIndex];
                    episode.Rating = correspondingRating.Rating;
                    episode.Votes = correspondingRating.Votes;
                }

                var basicIndex = episodeLookup.BinarySearch(new EpisodeDAO() { Tconst = fields[0] }, new EpisodeDAOComparer());
                if (basicIndex >= 0)
                {
                    var correspondingBasics = episodeLookup[basicIndex];
                    episode.EpisodeTitle = correspondingBasics.EpisodeTitle;
                    episode.RuntimeMinutes = correspondingBasics.RuntimeMinutes;
                    episode.Genres = correspondingBasics.Genres;
                }

                episode.Tconst = fields[0];
                episode.SeasonNumber = fields[2] == "\\N" ? -1 : int.Parse(fields[2]);
                episode.EpisodeNumber = fields[3] == "\\N" ? -1 : int.Parse(fields[3]);

                if (episode.SeasonNumber == -1 || episode.EpisodeNumber == -1)
                {
                    continue;
                }

                var titleIndex = titles.BinarySearch(new Title() { Tconst = fields[1] }, new TitleComparer());
                if (titleIndex >= 0)
                {
                    var correspondingTitle = titles[titleIndex];
                    episode.Title = correspondingTitle;
                    episode.TitleTconst = correspondingTitle.Tconst;
                    if (correspondingTitle.Episodes == null)
                    {
                        correspondingTitle.Episodes = new List<Episode>();
                    }
                    correspondingTitle.Episodes.Add(episode);
                    episodes.Add(episode);
                }
            }

            Console.WriteLine("Removing wrong datasets... {0}s", (DateTime.Now - startTime).TotalSeconds);

            titles.RemoveAll(t => t.Episodes != null && t.Episodes.All(e => e.Rating == 0 || e.Rating == null));
            titles.RemoveAll(t => t.Episodes != null && t.Episodes.All(e => e.EpisodeNumber == -1 && e.SeasonNumber == -1));
            titles.RemoveAll(t => t.Episodes == null);
            episodes.RemoveAll(e => titles.BinarySearch(new Title() { Tconst = e.TitleTconst }, new TitleComparer()) < 0);

            Console.WriteLine("Inserting titles & episodes... {0}s", (DateTime.Now - startTime).TotalSeconds);

            _context.Episodes.BatchDelete();
            _context.Titles.BatchDelete();
            _context.SaveChanges();
            _context.BulkInsert(titles);
            _context.BulkInsert(episodes);
            _context.BulkSaveChanges();

            Console.WriteLine("Writing Search Index... {0}s", (DateTime.Now - startTime).TotalSeconds);

            try
            {
                using (var ingest = NSonicFactory.Ingest(SonicHost, SonicPort, SonicSecret))
                {
                    ingest.Connect();

                    var flushCollectionResult = ingest.FlushCollection("titles");
                    Console.WriteLine($"Flush of all titles: {flushCollectionResult}");

                    foreach (var title in titles)
                    {
                        ingest.Push("titles", "generic", title.Tconst, title.PrimaryTitle.Replace("\"", "'"));
                    }
                }
            }
            catch (Exception e)
            {
                Console.WriteLine(e.Message);
            }


            Console.WriteLine("Retrieving additional Informations... {0}s", (DateTime.Now - startTime).TotalSeconds);

            var thisYear = DateTime.Now.Year;
            var avgVotesTitles = _context.Titles
            .Where(t => t.StartYear == thisYear)
            .OrderByDescending(t => t.Rating)
            .Average(t => t.Votes);

            var topTitles = _context.Titles
            .Where(t => t.StartYear == thisYear)
            .OrderByDescending(t => t.Rating)
            .Where(t => t.Votes > avgVotesTitles)
            .ToList();
            
            topTitles.RemoveAll(t => _context.AdditionalInfos.FirstOrDefault(ai => ai.Title.Tconst == t.Tconst) != null);

            for (int i = 0; i < 25; i++)
            {
                if (topTitles.ElementAtOrDefault(i) == null)
                {
                    break;
                }
                var title = topTitles.ElementAtOrDefault(i);
                var additionalInfo = _context.AdditionalInfos.FirstOrDefault(ai => ai.Title.Tconst == title.Tconst);
                if (additionalInfo == null)
                {
                    additionalInfo = retrieveAdditionalInfo(title.Tconst);
                    if (additionalInfo != null)
                    {
                        _context.AdditionalInfos.Add(additionalInfo);
                    }
                }
            }

            _context.SaveChanges();

            Console.WriteLine("Done. {0}s", (DateTime.Now - startTime).TotalSeconds);
        }
    }

    private AdditionalInfo? retrieveAdditionalInfo(string tconst)
    {
        using (WebClient webClient = new WebClient())
        {
            try
            {
                var json = webClient.DownloadString($"https://imdb-api.projects.thetuhin.com/title/{tconst}");


                var additionalInfoJson = JsonSerializer.Deserialize<AdditionalInfoJSON>(json);

                AdditionalInfo additionalInfo = new AdditionalInfo();
                additionalInfo.Title = _context.Titles.FirstOrDefault(t => t.Tconst == tconst);
                additionalInfo.Plot = additionalInfoJson.plot;
                additionalInfo.ImageURL = additionalInfoJson.image;
                if (additionalInfoJson.releaseDetailed.originLocations.Length > 0)
                {
                    ICountryProvider countryProvider = new CountryProvider();
                    additionalInfo.Country = additionalInfoJson.releaseDetailed.originLocations[0].cca2;
                }
                return additionalInfo;
            }
            catch (Exception e)
            {
                return null;
            }
        }
    }
}

class AdditionalInfoJSON
{
    public string plot { get; set; }
    public string image { get; set; }
    public ReleaseDetailed releaseDetailed { get; set; }
}

public class ReleaseDetailed
{
    public int day { get; set; }

    public int month { get; set; }

    public int year { get; set; }

    public CountryLocation releaseLocation { get; set; }

    public CountryLocation[] originLocations { get; set; }
}

public class CountryLocation
{
    public string country { get; set; }

    public string cca2 { get; set; }
}

class CountryListKeyValue
{
    public string key { get; set; }
    public string value { get; set; }
}

class EpisodeDAO
{
    public string Tconst { get; set; }
    public string EpisodeTitle { get; set; }
    public int? RuntimeMinutes { get; set; }
    public String? Genres { get; set; }
}

class RatingDAO
{
    public string Tconst { get; set; }
    public double Rating { get; set; }
    public int Votes { get; set; }
}

class EpisodeDAOComparer : IComparer<EpisodeDAO>
{
    public int Compare(EpisodeDAO x, EpisodeDAO y)
    {
        return x.Tconst.CompareTo(y.Tconst);
    }
}

class RatingDAOComparer : IComparer<RatingDAO>
{
    public int Compare(RatingDAO x, RatingDAO y)
    {
        return x.Tconst.CompareTo(y.Tconst);
    }
}
class TitleComparer : IComparer<Title>
{
    public int Compare(Title x, Title y)
    {
        return x.Tconst.CompareTo(y.Tconst);
    }
}