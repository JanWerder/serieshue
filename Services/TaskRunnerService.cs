

using System.Diagnostics;
using serieshue.Interfaces;
using serieshue.Models;
using System.Net;
using LinearTsvParser;
using System.IO.Compression;
using System.Collections.Generic;
using EFCore.BulkExtensions;
using System.Globalization;

namespace serieshue.Services;

public class TaskRunnerService : ITaskRunnerService
{

    private readonly SeriesHueContext _context;

    public TaskRunnerService(SeriesHueContext context)
    {
        _context = context;
    }

    public void RunIMDBUpdate()
    {
        using (WebClient webClient = new WebClient())
        {

            var titles = new List<Title>();
            var episodeLookup = new List<EpisodeDAO>();
            var ratingLookup = new List<RatingDAO>();

            var basicStream = new System.IO.MemoryStream(webClient.DownloadData("https://datasets.imdbws.com/title.basics.tsv.gz"));
            var ratingsStream = new System.IO.MemoryStream(webClient.DownloadData("https://datasets.imdbws.com/title.ratings.tsv.gz"));
            var episodeStream = new System.IO.MemoryStream(webClient.DownloadData("https://datasets.imdbws.com/title.episode.tsv.gz"));

            using var ungzippedRatings = new GZipStream(ratingsStream, CompressionMode.Decompress);
            var tsvReader = new TsvReader(ungzippedRatings);
            tsvReader.ReadLine();

            Console.WriteLine("Reading ratings...");

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

            Console.WriteLine("Reading basics...");

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

            Console.WriteLine("Sorting Lookup Tables...");

            ratingLookup.Sort(new RatingDAOComparer());
            episodeLookup.Sort(new EpisodeDAOComparer());
            titles.Sort(new TitleComparer());

            Console.WriteLine("Reading episodes...");

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
                    episode.Title.Tconst = correspondingTitle.Tconst;
                    if (correspondingTitle.Episodes == null)
                    {
                        correspondingTitle.Episodes = new List<Episode>();
                    }
                    correspondingTitle.Episodes.Add(episode);
                }
            }

            titles.RemoveAll(t => t.Episodes != null && t.Episodes.All(e => e.Rating == 0 || e.Rating == null));
            titles.RemoveAll(t => t.Episodes != null && t.Episodes.All(e => e.EpisodeNumber == -1 && e.SeasonNumber == -1));
            titles.RemoveAll(t => t.Episodes == null);

            Console.WriteLine("Inserting titles & episodes...");

            _context.Episodes.BatchDelete();
            _context.Titles.BatchDelete();
            _context.Titles.AddRange(titles);
            _context.SaveChanges();

            Console.WriteLine("Done.");

        }
    }
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