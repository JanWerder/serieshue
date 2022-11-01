

using System.Diagnostics;
using serieshue.Interfaces;
using serieshue.Models;
using System.Net;
using LinearTsvParser;
using System.IO.Compression;

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

            using var ungzippedBasic = new GZipStream(basicStream, CompressionMode.Decompress);
            var tsvReader = new TsvReader(ungzippedBasic);
            tsvReader.ReadLine();

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
                    title.Genres = fields[8];

                    titles.Add(title);
                }
                else if (fields[1] == "tvEpisode")
                {
                    var episode = new EpisodeDAO();
                    episode.Tconst = fields[0];
                    episode.EpisodeTitle = fields[2];
                    episode.RuntimeMinutes = fields[7] == "\\N" ? null : int.Parse(fields[7]);
                    episode.Genres = fields[8];

                    episodeLookup.Add(episode);
                }
            }

            using var ungzippedRatings = new GZipStream(ratingsStream, CompressionMode.Decompress);
            tsvReader = new TsvReader(ungzippedRatings);
            tsvReader.ReadLine();
            while (!tsvReader.EndOfStream)
            {
                List<string> fields = tsvReader.ReadLine();

                var rating = new RatingDAO();
                rating.Tconst = fields[0];
                rating.Rating = fields[1] == "\\N" ? 0 : float.Parse(fields[1]);
                rating.Votes = fields[2] == "\\N" ? 0 : int.Parse(fields[2]);

                ratingLookup.Add(rating);
            }

            using var ungzippedEpisodes = new GZipStream(episodeStream, CompressionMode.Decompress);
            tsvReader = new TsvReader(ungzippedEpisodes);
            tsvReader.ReadLine();
            while (!tsvReader.EndOfStream)
            {
                Console.Write(".");
                List<string> fields = tsvReader.ReadLine();

                var episode = new Episode();

                var correspondingBasics = episodeLookup.Where(x => x.Tconst == fields[0]).FirstOrDefault();
                var correspondingRating = ratingLookup.Where(x => x.Tconst == fields[0]).FirstOrDefault();

                episode.Tconst = fields[0];
                episode.EpisodeTitle = correspondingBasics.EpisodeTitle;
                episode.RuntimeMinutes = correspondingBasics.RuntimeMinutes;
                episode.Genres = correspondingBasics.Genres;

                if (correspondingRating != null)
                {
                    episode.Rating = correspondingRating.Rating;
                    episode.Votes = correspondingRating.Votes;
                }

                var correspondingTitle = titles.Where(x => x.Tconst == fields[1]).FirstOrDefault();
                if (correspondingTitle != null)
                {
                    episode.Title = correspondingTitle;
                    if (correspondingTitle.Episodes == null)
                    {
                        titles[titles.IndexOf(correspondingTitle)].Episodes = new List<Episode>();
                    }
                    titles[titles.IndexOf(correspondingTitle)].Episodes.Add(episode);
                }
            }

            //Truncate all Titles and Episodes and fill from titles and epsiode list
            _context.Titles.RemoveRange(_context.Titles);
            _context.Episodes.RemoveRange(_context.Episodes);
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
    public string Genres { get; set; }
}

class RatingDAO
{
    public string Tconst { get; set; }
    public double Rating { get; set; }
    public int Votes { get; set; }
}