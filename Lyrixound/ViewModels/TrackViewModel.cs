using LyricsFinder.Core;
using LyricsFinder.Core.LyricTypes;
using Prism.Mvvm;
using System;
using System.Collections.ObjectModel;
using System.Windows;

namespace Lyrixound.ViewModels
{
    public class TrackViewModel : BindableBase
    {
        private readonly Track _track;
        private readonly LyricsSettingsViewModel _lyricsSettings;
        private ObservableCollection<LyricLineViewModel> _lyricLines;
        private LyricLineViewModel _currentLine;
        private bool _hasSyncedLyrics;
        private TimeSpan _currentPosition;

        public string Artist
        {
            get => _track.Artist;
            set
            {
                _track.Artist = value;
                RaisePropertyChanged();
            }
        }

        public string Title
        {
            get => _track.Title;
            set
            {
                _track.Title = value;
                RaisePropertyChanged();
            }
        }

        public ILyric Lyrics
        {
            get => _track.Lyrics;
            set
            {
                _track.Lyrics = value;
                RaisePropertyChanged();
                Application.Current.Dispatcher.BeginInvoke(UpdateLyricLines);
            }
        }

        public ObservableCollection<LyricLineViewModel> LyricLines
        {
            get => _lyricLines;
            private set => SetProperty(ref _lyricLines, value);
        }

        public bool HasSyncedLyrics
        {
            get => _hasSyncedLyrics;
            private set => SetProperty(ref _hasSyncedLyrics, value);
        }

        public TimeSpan CurrentPosition
        {
            get => _currentPosition;
            set
            {
                if (SetProperty(ref _currentPosition, value))
                {
                    UpdateCurrentLine();
                }
            }
        }

        public LyricLineViewModel CurrentLine
        {
            get => _currentLine;
            private set => SetProperty(ref _currentLine, value);
        }

        public TrackViewModel(Track track, LyricsSettingsViewModel lyricsSettings)
        {
            _track = track;
            _lyricsSettings = lyricsSettings;
            LyricLines = new ObservableCollection<LyricLineViewModel>();
        }

        private void UpdateLyricLines()
        {
            LyricLines.Clear();
            CurrentLine = null;
            _currentPosition = TimeSpan.Zero;

            if (Lyrics is SyncedLyric syncedLyric && syncedLyric.Type == SyncedLyricType.Lrc)
            {
                HasSyncedLyrics = true;
                var parsedLines = LrcParser.Parse(syncedLyric.Text);

                foreach (var line in parsedLines)
                {
                    LyricLines.Add(new LyricLineViewModel(line.Timestamp, line.Text));
                }
            }
            else
            {
                HasSyncedLyrics = false;
            }

            UpdateCurrentLine();
        }

        private void UpdateCurrentLine()
        {
            if (!HasSyncedLyrics || LyricLines.Count == 0)
                return;

            // Find the line that should be active based on current position
            // Apply user-configured time offset for synced lyrics
            var timeOffset = TimeSpan.FromMilliseconds(_lyricsSettings.TimeOffsetMilliseconds);
            var effectivePosition = CurrentPosition + timeOffset;

            LyricLineViewModel activeLine = null;

            for (int i = 0; i < LyricLines.Count; i++)
            {
                var line = LyricLines[i];

                // Check if this line should be active
                // A line is active if:
                // 1. Effective position >= line's timestamp
                // 2. Effective position < next line's timestamp (or it's the last line)
                if (effectivePosition >= line.Timestamp)
                {
                    if (i == LyricLines.Count - 1 || effectivePosition < LyricLines[i + 1].Timestamp)
                    {
                        activeLine = line;
                    }
                }

                line.IsActive = false;
            }

            if (activeLine != null)
            {
                activeLine.IsActive = true;
                CurrentLine = activeLine;
            }
        }
    }
}
