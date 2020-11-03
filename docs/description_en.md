## Lyrixound

**Lyrixound** - simple application to find and display lyrics.

### Application features
* auto detecting current playing track from any music player or browser
* lyrics searching in various sources
* manual search by song's title and artist
* auto saving found lyrics to text file
* hiding all elements besides lyrics when other window receives focus and display it topmost
* ability to set text colors, size, font and so on
* ability to open lyrics source

### Main window
<img src="images/main.png"/>

* the panel to display found lyrics
* the button to find lyrics by artist and title specified in text fields
* title bar with general information and configuration buttons
* expanding panel at the bottom with lyrics style settings

### App settings
<img src="images/settings_players.png"/>

#### Player watchers
You can select players and change ther order by mouse's drag'n'drop. Selected players are checked for current playing track once in every specified time interval.

**Supported watchers:**
1. ***SystemMediaTransportControls*** - current track info is received on appearing system window with media info. The window is displayed in *Windows 10* when you press keys to change sound level, or to change track, or to pause music etc.
2. ***Yandex.Music*** - check state of [Yandex.Music](https://www.microsoft.com/store/apps/9NBLGGH0CB6D) player. *Note.* The app cannot receive track info if the player is minimized.

#### Lyrics providers
On this tab you can select lyrics providers and change the order to find lyrics.

**Supported providers:**
1. ***Directories*** - search in local folders. Settings for the directories are placed on corresponding tab.
2. ***Google*** - search lyrics on [Google](http://google.com/).

#### Directories settings
Set the folder to save all found lyrics. Every file name is named according to specified template.
