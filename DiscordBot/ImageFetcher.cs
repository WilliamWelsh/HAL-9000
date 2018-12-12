using System;

namespace Gideon
{
    class ImageFetcher
    {
        private static readonly Random getrandom = new Random();

        private static int GetRandomNumber(int min, int max)
        {
            lock (getrandom) { return getrandom.Next(min, max); }
        }

        public string GetRandomPic() => Alani[GetRandomNumber(0, Alani.Length)];

        public string[] Alani = {
                "https://i.imgur.com/pRsqdMv.jpg",
                "https://i.imgur.com/1H0aGG4.jpg",
                "https://i.imgur.com/TbcHPJU.jpg",
                "https://i.imgur.com/cqyvwXL.jpg",
                "https://i.imgur.com/PhV3h8N.jpg",
                "https://i.imgur.com/MAp3ONz.jpg",
                "https://i.imgur.com/Q4fGT5cg.jpg",
                "https://i.imgur.com/fmjkcNc.jpg",
                "https://i.imgur.com/NSwMvXQ.jpg",
                "https://i.imgur.com/4lQ7LGG.jpg",
                "https://i.imgur.com/Nqwo43U.jpg"
                };
    }
}
