using NAudio.CoreAudioApi;
using NAudio.Wave;
using NLog;
using ShazamIO;
using System;
using System.IO;
using System.Runtime.InteropServices;
using System.Text.Json;
using System.Threading.Tasks;

namespace Lyrixound.Services
{
    /// <summary>
    /// Service for recognizing songs from system audio using ShazamIO.
    /// </summary>
    public class AudioRecognitionService
    {
        /// <summary>
        /// Peak amplitude below this is treated as silence (~ -60 dBFS for float PCM).
        /// Quiet playback still clears it; digital silence and an idle output do not.
        /// </summary>
        private const float SilencePeakThreshold = 0.001f;

        private readonly ILogger _logger = LogManager.GetCurrentClassLogger();

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
            var audioBuffer = new MemoryStream();
            WasapiRecorder capture = null;
            WaveFileWriter waveWriter = null;
            var writeLock = new object();
            var recordingStopped = new TaskCompletionSource<bool>(TaskCreationOptions.RunContinuationsAsynchronously);
            var peakAmplitude = 0f;

            try
            {
                capture = new WasapiRecorderBuilder()
                    .WithLoopbackCapture()
                    .Build();
                waveWriter = new WaveFileWriter(audioBuffer, capture.WaveFormat);

                capture.DataAvailable += (buffer, flags, _, _) =>
                {
                    lock (writeLock)
                    {
                        if (waveWriter == null || buffer.IsEmpty)
                            return;

                        // WASAPI marks idle loopback packets as silent. Their bytes are not
                        // reliable audio, and a fully silent capture should not be uploaded.
                        if ((flags & AudioClientBufferFlags.Silent) == 0)
                        {
                            var packetPeak = GetPeakAmplitude(buffer, capture.WaveFormat);
                            if (packetPeak > peakAmplitude)
                                peakAmplitude = packetPeak;
                        }

                        waveWriter.Write(buffer);
                    }
                };

                capture.RecordingStopped += (s, e) =>
                {
                    if (e.Exception != null)
                        recordingStopped.TrySetException(e.Exception);
                    else
                        recordingStopped.TrySetResult(true);
                };

                capture.StartRecording();
                _logger.Info($"Recording system audio for {durationSeconds} seconds...");

                await Task.Delay(TimeSpan.FromSeconds(durationSeconds));
                capture.StopRecording();
                await recordingStopped.Task;

                lock (writeLock)
                {
                    waveWriter.Flush();
                    var audioData = audioBuffer.ToArray();

                    if (peakAmplitude < SilencePeakThreshold)
                    {
                        _logger.Info($"System audio is silent (peak {peakAmplitude:0.######}); not sending it for recognition");
                        return null;
                    }

                    _logger.Info($"Captured {audioData.Length} bytes of audio (peak {peakAmplitude:0.###})");
                    return audioData;
                }
            }
            catch (Exception ex)
            {
                _logger.Error(ex, "Error capturing system audio");
                return null;
            }
            finally
            {
                lock (writeLock)
                {
                    waveWriter?.Dispose();
                    waveWriter = null;
                }

                capture?.Dispose();
            }
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

        private static float GetPeakAmplitude(ReadOnlySpan<byte> buffer, WaveFormat format)
        {
            if (format.BitsPerSample == 32 && buffer.Length >= 4)
            {
                // Shared-mode loopback is IEEE float, including WAVE_FORMAT_EXTENSIBLE.
                var length = buffer.Length - (buffer.Length % sizeof(float));
                var samples = MemoryMarshal.Cast<byte, float>(buffer[..length]);
                var peak = 0f;
                foreach (var sample in samples)
                {
                    var abs = MathF.Abs(sample);
                    if (float.IsNaN(abs) || float.IsInfinity(abs))
                        continue;

                    // Far outside [-1, 1] is not normalized float PCM.
                    if (abs > 8f)
                        return 1f;

                    if (abs > peak)
                        peak = abs;
                }

                return peak;
            }

            if (format.BitsPerSample == 16 && buffer.Length >= 2)
            {
                var length = buffer.Length - (buffer.Length % sizeof(short));
                var samples = MemoryMarshal.Cast<byte, short>(buffer[..length]);
                var peak = 0;
                foreach (var sample in samples)
                {
                    var abs = Math.Abs((int)sample);
                    if (abs > peak)
                        peak = abs;
                }

                return peak / 32768f;
            }

            foreach (var b in buffer)
            {
                if (b != 0)
                    return 1f;
            }

            return 0f;
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
