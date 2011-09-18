using System;
using System.Diagnostics;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Windows.Threading;
using mshtml;

namespace Grooveshark
{
    public class Player
    {
        private IHTMLDocument2 m_document;

        public event PlayerStatusChangedEventHandler PlayerStatusChanged;
        public event SongChangedEventHandler SongChanged;

        public PlayerStatus Status { get; private set; }
        public Song CurrentSong { get; private set; }
        
        public Player(IHTMLDocument2 document)
        {
            if (document == null)
                throw new ArgumentNullException();

            m_document = document;

            AddWrappers();

            HideAdvertising();

            Status = PlayerStatus.None;

            var timer = new DispatcherTimer { Interval = TimeSpan.FromMilliseconds(500), IsEnabled = true };
            timer.Tick += TimerTick;
            timer.Start();
        }

        private void AddWrappers()
        {
            var head = m_document.all.tags("head")[0];
            dynamic script = m_document.createElement("script");
            script.type = "text/javascript";
            script.text = @"document.body.oncontextmenu = function() {
                                return false;
                            }

                            var player = null; 
                            var statuses = { 
                                PLAY_STATUS_NONE: 0,
                                PLAY_STATUS_INITIALIZING: 1,
                                PLAY_STATUS_LOADING: 2,
                                PLAY_STATUS_PLAYING: 3,
                                PLAY_STATUS_PAUSED: 4,
                                PLAY_STATUS_BUFFERING: 5,
                                PLAY_STATUS_FAILED: 6,
                                PLAY_STATUS_COMPLETED: 7
                            }

                            $(document).ready(function() {
                                tryGetPlayer();
                            });
                            
                            function tryGetPlayer() {
                                try
                                {
                                    player = GS.player;
                                }
                                catch (e)
                                {
                                    setTimeout('tryGetPlayer()', 2000);
                                }
                            }

                            function nextWrapper() {
                                player.nextSong();
                            }

                            function previousWrapper() {
                                player.previousSong();
                            }

                            function togglePlaybackWrapper() {
                                var a = player.getPlaybackStatus();
                                if (a) {
                                    switch (a.status) {
                                    case statuses.PLAY_STATUS_NONE:
                                    case statuses.PLAY_STATUS_FAILED:
                                    case statuses.PLAY_STATUS_COMPLETED:
                                    default:
                                        a.activeSong && player.playSong.apply(GS.player, a.activeSong.queueSongID);
                                        break;
                                    case statuses.PLAY_STATUS_INITIALIZING:
                                    case statuses.PLAY_STATUS_LOADING:
                                        player.stopSong();
                                        break;
                                    case statuses.PLAY_STATUS_PLAYING:
                                    case statuses.PLAY_STATUS_BUFFERING:
                                        player.pauseSong();
                                        break;
                                    case statuses.PLAY_STATUS_PAUSED:
                                        player.resumeSong();
                                        break
                                    }
                                }
                            }

                            function favoriteSongWrapper() {
                                var activeSong = getCurrentSongStatusWrapper().activeSong;
                                GS.user.addToSongFavorites(activeSong.SongID);
                            }

                            function hideAdvertisingBarWrapper() {
                                $('#capital').remove();
                                $('#application').css('marginRight', 0);
                                
                                setTimeout('hideAdvertisingBarWrapper()', 500);
                            }
                
                            function getCurrentSongStatusWrapper() {
                                try
                                {
                                    return player.getPlaybackStatus();
                                }
                                catch (e)
                                {
                                    return null;
                                }
                            }";

            head.appendChild(script);
        }

        public void HideAdvertising()
        {
            ExecuteScript("hideAdvertisingBarWrapper();");
        }

        public void NextSong()
        {
            // also key ctrl + 39
            ExecuteScript("nextWrapper();");
        }

        public void PreviousSong()
        {
            // also key ctrl + 37
            ExecuteScript("previousWrapper();");
        }
        
        public void FavoriteSong()
        {
            ExecuteScript("favoriteSongWrapper();");
        }

        public void TogglePlayback()
        {
            // also key 32
            ExecuteScript("togglePlaybackWrapper();");
        }
        
        public void VolumeUp()
        {
            // key ctrl + 38
            throw new NotImplementedException();
        }

        public void VolumeDown()
        {
            // key ctrl + 40
            throw new NotImplementedException();
        }

        private dynamic GetSongStatus()
        {
            // song - object, status - string
            return ExecuteMethod("getCurrentSongStatusWrapper");
        }

        private void TimerTick(object sender, EventArgs e)
        {
            var songStatus = GetSongStatus();

            if (songStatus == null || songStatus is DBNull) return;

            var status = (PlayerStatus) int.Parse(songStatus.status.ToString());

            lock (this)
            {
                if (status != Status)
                {
                   if (PlayerStatusChanged != null)
                        PlayerStatusChanged(this, new PlayerStatusEventArgs(Status, status));

                    Status = status;
                }
            }

            var song = songStatus.activeSong;
            /* song object
             * albumID - int
             * trackNum - int
             * artistID - int
             * position - double
             * estimateDuration - int
             * artURL - string / url
             * albumName - string
             * calculatedDuration - double
             * songID - int
             * artistName - string
             * vote - int
             * songName - string
             */

            if (song is DBNull)
            {
                lock (this)
                {
                    if (CurrentSong != null)
                    {
                        if (SongChanged != null)
                            SongChanged(this, new SongChangedEventArgs(CurrentSong, null));
                    }

                    CurrentSong = null;
                }

                Marshal.ReleaseComObject(songStatus);
                return;
            }

            // new definition
            //AlbumID: 3944048
            //AlbumName: "Wasting"
            //ArtistID: 10152
            //ArtistName: "Luminary"
            //CoverArtFilename: "default.png"
            //EstimateDuration: 572000
            //Flags: 0
            //SongID: 24466846
            //SongName: "Wasting (Andy Moor Remix)"
            //autoplayVote: 0
            //context: Object
            //index: 169
            //parentQueueID: "7080204381316276110285"
            //queueSongID: 171
            //source: "recommended"
            //sponsoredAutoplayID: 0

            var newSong = new Song
                        {
                            AlbumID = song.AlbumID,
                            // TrackNum = song.trackNum,
                            ArtistID = song.ArtistID,
                            EstimateDuration = song.EstimateDuration,
                            CoverArtFilename = song.CoverArtFilename,
                            AlbumName = song.AlbumName,
                            ID = song.SongID,
                            ArtistName = song.ArtistName,
                            Vote = song.autoplayVote,
                            Name = song.SongName
                        };

            lock (this)
            {
                if (!newSong.Equals(CurrentSong))
                {
                    if (SongChanged != null)
                        SongChanged(this, new SongChangedEventArgs(CurrentSong, newSong));

                    CurrentSong = newSong;
                }
            }

            Marshal.ReleaseComObject(songStatus);
        }

        private dynamic ExecuteMethod(string method, params object[] args)
        {
            if (args.Length == 0) args = null; 

            try
            {
                return m_document.Script.GetType().InvokeMember(method, BindingFlags.InvokeMethod, null, m_document.Script, args);
            }
            catch (Exception e)
            {
#if DEBUG
               Debug.Write("Could not call method: " + method +
                    Environment.NewLine +
                    "Exception: " + e.Message);
#endif
            }

            return null;
        }

        private void ExecuteScript(string script)
        {
            try 
            {
                m_document.parentWindow.execScript(script);
            } 
            catch (Exception e)
            {
#if DEBUG
              Debug.Write("Could not call script: " + script +
                    Environment.NewLine +
                    "Exception: " + e.Message);
#endif
            }
        }
    }

    public delegate void SongChangedEventHandler(object sender, SongChangedEventArgs args);

    public class SongChangedEventArgs : EventArgs
    {
        public Song PreviousSong { get; private set; }
        public Song CurrentSong { get; private set; }

        public SongChangedEventArgs(Song previous, Song current)
        {
            PreviousSong = previous;
            CurrentSong = current;
        }
    }

    public delegate void PlayerStatusChangedEventHandler(object sender, PlayerStatusEventArgs args);

    public class PlayerStatusEventArgs : EventArgs
    {
        public PlayerStatus PreviousStatus { get; private set; }
        public PlayerStatus CurrentStatus { get; private set; }

        public PlayerStatusEventArgs(PlayerStatus previous, PlayerStatus current)
        {
            PreviousStatus = previous;
            CurrentStatus = current;
        }
    }
}
