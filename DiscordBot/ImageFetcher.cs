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

        public string GetRandomPic(ref string[] source)
        {
            return source[GetRandomNumber(0, source.Length)];
        }

        public string[] Michael = {
                "http://digitalspyuk.cdnds.net/17/29/980x490/landscape-1500763587-trek3.jpg",
                "http://fanfest.com/wp-content/uploads/2017/09/Star-Trek-Discovery-019.jpg",
                "http://cimg.tvgcdn.net/i/2017/09/22/a2f1a417-012c-4e00-b964-78c32607b480/170922-news-star-trek.jpg",
                "https://www.telegraph.co.uk/content/dam/tv/2017/09/25/StarTrekDiscovery_trans_NvBQzQNjv4BqqVzuuqpFlyLIwiB6NTmJwciSEN16FbSoBRvKGwClCS0.jpg?imwidth=450",
                "https://s3.amazonaws.com/bgn2018media/wp-content/uploads/2017/11/28224902/Michael-Burnam.jpg",
                "https://heroichollywood.b-cdn.net/wp-content/uploads/2017/06/sonequa-martin-green-star-trek-discovery.jpg?x42694",
                "https://pixel.nymag.com/imgs/daily/vulture/2017/10/04/04-star-trek-discovery.w710.h473.jpg",
                "https://cdn.discordapp.com/attachments/395101384204353539/440378801914904588/image.jpg",
                "https://cdn.discordapp.com/attachments/395101384204353539/440378765701545994/image.jpg",
                "https://cdn.discordapp.com/attachments/395101384204353539/440378677646196738/image.jpg",
                "https://cdn.discordapp.com/attachments/395101384204353539/440378876670115871/image.gif",
                "https://cdn.discordapp.com/attachments/395101384204353539/440378936573165568/image.jpg",
                "https://cdn.discordapp.com/attachments/395101384204353539/440379113132261376/image.jpg",
                "https://cdn.discordapp.com/attachments/395101384204353539/440379026859622400/image.jpg",
                "https://cdn.discordapp.com/attachments/395101384204353539/440379268745265163/image.jpg",
                "https://cdn.discordapp.com/attachments/395101384204353539/440379213883637770/image.jpg"
                };

        public string[] Constantine = {
                "http://cdn.collider.com/wp-content/uploads/2015/10/arrow-matt-ryan-constantine-image-haunted.jpg",
                "https://pmctvline2.files.wordpress.com/2015/08/constantine-arrow.jpg?w=619&h=420&crop=1",
                "https://cdn.vox-cdn.com/thumbor/397RfxMlsFlcBxDmiejVrdpBYnY=/0x0:3000x2000/1200x800/filters:focal(949x462:1429x942)/cdn.vox-cdn.com/uploads/chorus_image/image/52651047/constantine.0.jpg",
                "https://www.bleedingcool.com/wp-content/uploads/2016/11/Constantine1.jpg",
                "https://nerdist.com/wp-content/uploads/2016/07/Constantine-Featured-07032016.jpg",
                "https://vignette.wikia.nocookie.net/arrow/images/5/52/John_Constantine_first_look_promo.png/revision/latest?cb=20150910131141",
                "https://tribzap2it.files.wordpress.com/2016/07/constantine-cw-seed.jpg?w=900"
                };

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
