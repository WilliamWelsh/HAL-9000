// Get Movie data, TV show data, and YouTube channel data
using Newtonsoft.Json;

namespace Gideon
{
    class MediaFetchHandler
    {
        public Movie FetchMovie(string Search) => JsonConvert.DeserializeObject<Movie>(Config.Utilities.webClient.DownloadString($"http://www.omdbapi.com/?t={Search}&apikey={Config.bot.MovieTVAPIKey}"));

        public TVShow FetchShow(string Search) => JsonConvert.DeserializeObject<TVShow>(Config.Utilities.webClient.DownloadString($"http://www.omdbapi.com/?t={Search}&apikey={Config.bot.MovieTVAPIKey}"));

        public YTChannel FetchYTChannel(string ID) => JsonConvert.DeserializeObject<YTChannel>(Config.Utilities.webClient.DownloadString($"https://www.googleapis.com/youtube/v3/channels?part=statistics&id={ID}&key={Config.bot.YouTubeAPIKey}"));

        public struct Rating
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
            public Rating[] Ratings { get; set; }
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