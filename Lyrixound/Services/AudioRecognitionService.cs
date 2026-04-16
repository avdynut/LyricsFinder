using NAudio.Wave;
using NLog;
using ShazamIO;
using System;
using System.IO;
using System.Text.Json;
using System.Threading.Tasks;

namespace Lyrixound.Services
{
    /// <summary>
    /// Service for recognizing songs from system audio using ShazamIO.
    /// </summary>
    public class AudioRecognitionService
    {
        private readonly ILogger _logger = LogManager.GetCurrentClassLogger();
        private WasapiLoopbackCapture _capture;
        private MemoryStream _audioBuffer;
        private WaveFileWriter _waveWriter;

        public async Task<RecognizedTrackInfo> RecognizeSongFromSystemAudioAsync(int durationSeconds = 5)
        {
            _logger.Info("Starting audio recognition from system audio...");

            try
            {
                // Capture audio from default playback device (loopback)
                var audioData = await CaptureSystemAudioAsync(durationSeconds);

                if (audioData == null || audioData.Length == 0)
                {
                    _logger.Warn("No audio data captured");
                    return new RecognizedTrackInfo();
                }

                // Recognize the song using ShazamIO
                return await RecognizeAudioAsync(audioData);
            }
            catch (Exception ex)
            {
                _logger.Error(ex, "Error during audio recognition");
                throw;
            }
        }

        private async Task<byte[]> CaptureSystemAudioAsync(int durationSeconds)
        {
            _audioBuffer = new MemoryStream();

            return await Task.Run(() =>
            {
                try
                {
                    // Use loopback capture to record system audio
                    _capture = new WasapiLoopbackCapture();

                    // Create wave writer to write to memory stream
                    _waveWriter = new WaveFileWriter(_audioBuffer, _capture.WaveFormat);

                    _capture.DataAvailable += (s, e) =>
                    {
                        _waveWriter.Write(e.Buffer, 0, e.BytesRecorded);
                    };

                    _capture.StartRecording();
                    _logger.Info($"Recording system audio for {durationSeconds} seconds...");

                    // Record for specified duration
                    System.Threading.Thread.Sleep(durationSeconds * 1000);

                    _capture.StopRecording();
                    _waveWriter.Flush();

                    var audioData = _audioBuffer.ToArray();
                    _logger.Info($"Captured {audioData.Length} bytes of audio");

                    return audioData;
                }
                catch (Exception ex)
                {
                    _logger.Error(ex, "Error capturing system audio");
                    return null;
                }
                finally
                {
                    _capture?.Dispose();
                    _waveWriter?.Dispose();
                    _audioBuffer?.Dispose();
                }
            });
        }

        private async Task<RecognizedTrackInfo> RecognizeAudioAsync(byte[] audioData)
        {
            try
            {
                _logger.Info("Sending audio to Shazam for recognition...");

                using var shazam = new Shazam();
                var result = await shazam.RecognizeAsync(audioData);

                _logger.Debug($"Recognition result: {result.RootElement}");

                // Parse the JSON response
                var root = result.RootElement;
                var trackInfo = new RecognizedTrackInfo();

                // Check if there are matches
                if (root.TryGetProperty("track", out var track))
                {
                    // Basic info
                    trackInfo.Title = GetStringProperty(track, "title");
                    trackInfo.Artist = GetStringProperty(track, "subtitle");
                    trackInfo.Key = GetStringProperty(track, "key");

                    // Album info
                    if (track.TryGetProperty("sections", out var sections))
                    {
                        foreach (var section in sections.EnumerateArray())
                        {
                            if (section.TryGetProperty("type", out var sectionType) &&
                                sectionType.GetString() == "SONG")
                            {
                                if (section.TryGetProperty("metadata", out var metadata))
                                {
                                    foreach (var item in metadata.EnumerateArray())
                                    {
                                        var title = GetStringProperty(item, "title");
                                        var text = GetStringProperty(item, "text");

                                        switch (title?.ToLowerInvariant())
                                        {
                                            case "album":
                                                trackInfo.Album = text;
                                                break;
                                            case "label":
                                                trackInfo.Label = text;
                                                break;
                                            case "released":
                                                trackInfo.ReleaseDate = text;
                                                break;
                                        }
                                    }
                                }
                            }
                        }
                    }

                    // Genre
                    if (track.TryGetProperty("genres", out var genres))
                    {
                        if (genres.TryGetProperty("primary", out var primaryGenre))
                        {
                            trackInfo.Genre = primaryGenre.GetString();
                        }
                    }

                    // Cover Art
                    if (track.TryGetProperty("images", out var images))
                    {
                        // Try high quality cover art first
                        if (images.TryGetProperty("coverarthq", out var coverarthq))
                        {
                            trackInfo.CoverArtUrl = coverarthq.GetString();
                        }
                        else if (images.TryGetProperty("coverart", out var coverart))
                        {
                            trackInfo.CoverArtUrl = coverart.GetString();
                        }
                        else if (images.TryGetProperty("background", out var background))
                        {
                            trackInfo.CoverArtUrl = background.GetString();
                        }
                    }

                    // URLs and links
                    if (track.TryGetProperty("share", out var share))
                    {
                        trackInfo.ShazamUrl = GetStringProperty(share, "href");
                    }

                    if (track.TryGetProperty("hub", out var hub))
                    {
                        if (hub.TryGetProperty("actions", out var actions))
                        {
                            foreach (var action in actions.EnumerateArray())
                            {
                                var actionType = GetStringProperty(action, "type");
                                var uri = GetStringProperty(action, "uri");

                                if (!string.IsNullOrEmpty(uri))
                                {
                                    if (uri.Contains("music.apple.com") || uri.Contains("itunes.apple.com"))
                                    {
                                        trackInfo.AppleMusicUrl = uri;
                                    }
                                    else if (uri.Contains("spotify.com"))
                                    {
                                        trackInfo.SpotifyUrl = uri;
                                    }
                                    else if (uri.Contains("youtube.com") || uri.Contains("youtu.be"))
                                    {
                                        trackInfo.YouTubeUrl = uri;
                                    }
                                }
                            }
                        }
                    }

                    // ISRC (International Standard Recording Code)
                    trackInfo.Isrc = GetStringProperty(track, "isrc");

                    _logger.Info($"Song recognized: {trackInfo.Artist} - {trackInfo.Title}");

                    if (!string.IsNullOrEmpty(trackInfo.Album))
                        _logger.Info($"  Album: {trackInfo.Album}");
                    if (!string.IsNullOrEmpty(trackInfo.Genre))
                        _logger.Info($"  Genre: {trackInfo.Genre}");
                    if (!string.IsNullOrEmpty(trackInfo.ReleaseDate))
                        _logger.Info($"  Released: {trackInfo.ReleaseDate}");

                    return trackInfo;
                }
                else if (root.TryGetProperty("matches", out var matches))
                {
                    if (matches.GetArrayLength() == 0)
                    {
                        _logger.Warn("No matches found for the audio");
                        return trackInfo;
                    }
                }

                _logger.Warn("Could not parse recognition result");
                return trackInfo;
            }
            catch (Exception ex)
            {
                _logger.Error(ex, "Error recognizing audio with Shazam");
                return new RecognizedTrackInfo();
            }
        }

        private static string GetStringProperty(JsonElement element, string propertyName)
        {
            if (element.TryGetProperty(propertyName, out var property))
            {
                return property.GetString();
            }
            return null;
        }
    }
}
