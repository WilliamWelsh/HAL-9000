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
            public string Source;
            public string Value;
        }

        public struct Movie
        {
            public string Title;
            public string Year;
            public string Runtime;
            public string Director;
            public string BoxOffice;
            public string Poster;
            public Rates[] Ratings;
            public string imdbRating;
            public string Plot;
        }

        public struct TVShow
        {
            public string Title;
            public string Year;
            public string Runtime;
            public string Poster;
            public string imdbRating;
            public string Plot;
        }

        public struct item
        {
            public stats statistics;
        }

        public struct stats
        {
            public string viewCount;
            public string subscriberCount;
            public string videoCount;
        }

        public struct YTChannel
        {
            public item[] items;
        }
    }
}
