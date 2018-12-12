// Get Movie data, TV show data, and YouTube channel data
using System.Net;
using Newtonsoft.Json;

namespace Gideon
{
    class MediaFetchHandler
    {
        public Movie FetchMovie(string Search)
        {
            Movie media;
            using (WebClient wc = new WebClient())
            {
                var json = wc.DownloadString($"http://www.omdbapi.com/?t={Search}&apikey={Config.bot.MovieTVAPIKey}");
                media = JsonConvert.DeserializeObject<Movie>(json);
            }
            return media;
        }

        public TVShow FetchShow(string Search)
        {
            TVShow media;
            using (WebClient wc = new WebClient())
            {
                var json = wc.DownloadString($"http://www.omdbapi.com/?t={Search}&apikey={Config.bot.MovieTVAPIKey}");
                media = JsonConvert.DeserializeObject<TVShow>(json);
            }
            return media;
        }

        public YTChannel FetchYTChannel(string ID)
        {
            YTChannel media;
            using (WebClient wc = new WebClient())
            {
                var json = wc.DownloadString($"https://www.googleapis.com/youtube/v3/channels?part=statistics&id={ID}&key={Config.bot.YouTubeAPIKey}");
                media = JsonConvert.DeserializeObject<YTChannel>(json);
            }
            return media;
        }

        public struct Rates
        {
            public string Source { get; set; }
            public string Value { get; set; }
        }

        public struct Movie
        {
            public string Title { get; set; }
            public string Year { get; set; }
            public string Runtime { get; set; }
            public string Director { get; set; }
            public string BoxOffice { get; set; }
            public string Poster { get; set; }
            public Rates[] Ratings { get; set; }
            public string imdbRating { get; set; }
            public string Plot { get; set; }
        }

        public struct TVShow
        {
            public string Title { get; set; }
            public string Year { get; set; }
            public string Runtime { get; set; }
            public string Poster { get; set; }
            public string imdbRating { get; set; }
            public string Plot { get; set; }
        }

        public struct item
        {
            public stats statistics { get; set; }
        }

        public struct stats
        {
            public string viewCount { get; set; }
            public string subscriberCount { get; set; }
            public string videoCount { get; set; }
        }

        public struct YTChannel
        {
            public item[] items { get; set; }
        }
    }
}