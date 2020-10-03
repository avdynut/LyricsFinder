using LyricsFinder.Core;
using LyricsFinder.Core.LyricTypes;
using MSHTML;
using NLog;
using nucs.JsonSettings;
using System;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;
using System.Web;

namespace LyricsProviders.GoogleProvider
{
    public class GoogleTrackInfoProvider : WebTrackInfoProvider
    {
        private readonly ILogger _logger = LogManager.GetCurrentClassLogger();
        private readonly GoogleProviderSettings _settings = JsonSettings.Load<GoogleProviderSettings>();

        public const string Name = "Google";
        public override string DisplayName => Name;

        public override async Task<Track> FindTrackAsync(TrackInfo trackInfo)
        {
            string query = $"{trackInfo.Artist} {trackInfo.Title}";
            var lyricsText = await DownloadLyricsAsync(query);

            var track = new Track(trackInfo);

            if (string.IsNullOrEmpty(lyricsText))
            {
                track.Lyrics = new NoneLyric("Not Found");
                _logger.Trace("Lyrics not found");
            }
            else
            {
                track.Lyrics = new UnsyncedLyric(lyricsText);
                // todo: parse artist and title
                _logger.Trace("Found lyrics");
            }

            return track;
        }

        private async Task<string> DownloadLyricsAsync(string query)
        {
            string encodedString = HttpUtility.UrlEncode(query);
            string url = _settings.SearchUrl + encodedString;
            string lyricsText = null;

            var httpClient = new HttpClient();
            httpClient.DefaultRequestHeaders.UserAgent.ParseAdd(_settings.UserAgent);
            httpClient.DefaultRequestHeaders.AcceptLanguage.ParseAdd(_settings.AcceptLanguages);

            try
            {
                var html = await httpClient.GetStringAsync(url);

#if DEBUG
                SaveHtmlFile(encodedString, html);
#endif

                var doc = (IHTMLDocument2)new HTMLDocument();
                doc.write(html);
                lyricsText = ParseLyrics((HTMLDocument)doc);
            }
            catch (Exception e)
            {
                _logger.Error(e, "Error while perform HTTP request");
            }
            finally
            {
                httpClient.Dispose();
            }

            return lyricsText;
        }

        private string ParseLyrics(HTMLDocument doc)
        {
            try
            {
                var elementsEnumerator = doc.getElementsByClassName(_settings.LyricsDivClassName).GetEnumerator();
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
                _logger.Warn(ex, "Error while parsing lyrics");
            }
            return null;
        }

        private void SaveHtmlFile(string query, string html)
        {
            try
            {
                var googleFolder = "google-results";
                Directory.CreateDirectory(googleFolder);
                File.WriteAllText(Path.Combine(googleFolder, $"{query}.html"), html);
            }
            catch (Exception ex)
            {
                _logger.Error(ex, "Cannot save html file");
            }
        }
    }
}
