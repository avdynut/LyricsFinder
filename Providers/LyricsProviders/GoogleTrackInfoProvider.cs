using LyricsFinder.Core;
using LyricsFinder.Core.LyricTypes;
using MSHTML;
using System;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;
using System.Web;

namespace LyricsProviders
{
    public class GoogleTrackInfoProvider : WebTrackInfoProvider
    {
        private const string SearchUrl = "https://www.google.com/search?q=";
        private const string UserAgent = "Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/77.0.3865.75 Safari/537.36";
        private const string AcceptLanguages = "ru,en";

        private const string LyricsDivClassName = "Oh5wg";

        public override string Name => "Google";

        public override async Task<Track> FindTrackAsync(TrackInfo trackInfo)
        {
            string query = $"{trackInfo.Artist} {trackInfo.Title}";
            string encodedString = HttpUtility.UrlEncode(query);
            string url = SearchUrl + encodedString;

            using var httpClient = new HttpClient();
            httpClient.DefaultRequestHeaders.UserAgent.ParseAdd(UserAgent);
            httpClient.DefaultRequestHeaders.AcceptLanguage.ParseAdd(AcceptLanguages);
            var html = await httpClient.GetStringAsync(url);
            File.WriteAllText($"{query}.html", html);

            //var html = File.ReadAllText("search.html");

            var doc = (IHTMLDocument2)new HTMLDocument();
            doc.write(html);
            var lyricsText = ParseLyrics((HTMLDocument)doc);

            var track = new Track();
            if (string.IsNullOrEmpty(lyricsText))
            {
                track.Lyrics = new NoneLyric("Not Found");
            }
            else
            {
                track.Lyrics = new UnsyncedLyric(lyricsText);
            }

            return track;
        }

        private string ParseLyrics(HTMLDocument doc)
        {
            try
            {
                var elementsEnumerator = doc.getElementsByClassName(LyricsDivClassName).GetEnumerator();
                elementsEnumerator.MoveNext();
                var divLyricsBlock = (IHTMLElement)elementsEnumerator.Current;

                var blocksEnumerator = (divLyricsBlock.children as IHTMLElementCollection).GetEnumerator();
                blocksEnumerator.MoveNext();
                var firstBlockChildren = (((IHTMLElement)blocksEnumerator.Current).children as IHTMLElementCollection).Cast<IHTMLElement>();

                var verses = (firstBlockChildren.ElementAt(4).children as IHTMLElementCollection).Cast<IHTMLElement>();

                var textVerses = verses.Select(x => x.innerText);

                string twoNewLines = Environment.NewLine + Environment.NewLine;
                var lyrics = string.Join(twoNewLines, textVerses);

                return lyrics;
            }
            catch (Exception ex)
            {
                Trace.WriteLine(ex);
            }
            return null;
        }
    }
}
