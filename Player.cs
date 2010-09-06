using System;
using System.Reflection;
using System.Windows;
using System.Windows.Threading;
using mshtml;

namespace Grooveshark
{
    public class Player
    {
        private IHTMLDocument2 m_document;

        public event EventHandler PlayerStatusChanged;

        public PlayerStatus Status { get; private set; }

        public Player(IHTMLDocument2 document)
        {
            if (document == null)
                throw new ArgumentNullException();

            m_document = document;

            AddWrappers();

            HideAdvertising();

            Status = PlayerStatus.None;

            var timer = new DispatcherTimer { Interval = TimeSpan.FromMilliseconds(100), IsEnabled = true };
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
                            
                            var player = document.getElementById('gsliteswf');

                            function nextWrapper() {
                                player.next();
                            }

                            function previousWrapper() {
                                player.previous();
                            }

                            function togglePlaybackWrapper() {
                                player.togglePlayback();
                            }

                            function addSongsToQueueWrapper(param) {
                                player.addSongsToQueue(param);
                            }

                            function pausePlaybackWrapper() {
                                player.pausePlayback();
                            }

                            function playAlbumWrapper(param) {
                                player.playAlbum(param);
                            }

                            function playPlaylistWrapper(param) {
                                player.playPlaylist(param);
                            }

                            function setThemeWrapper(param) {
                                player.setTheme(param);
                            }

                            function voteSongWrapper(param) {
                                player.voteSong(param);
                            }

                            function favoriteSongWrapper() {
                                player.favoriteSong();
                            }

                            function reloadUserLibraryWrapper() {
                                player.reloadUserLibrary();
                            }

                            function hideAdvertisingBarWrapper() {
                                // hideAdvertisingBar();
                                $('#sidebar').remove();
                                $('#mainContentWrapper').attr('id', 'playArea');
                                $('#playArea').css('marginRight', 0);
                            }
                
                            function getCurrentSongStatusWrapper() {
                                return player.getCurrentSongStatus();
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

        public void VoteSong(int rating)
        {
            ExecuteScript(String.Format("voteSongWrapper({0});", rating));
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

        public void PausePlayback()
        {
            ExecuteScript("pausePlaybackWrapper();");
        }

        public void SetTheme(int theme)
        {
            ExecuteScript(String.Format("setThemeWrapper({0});", theme));
        }

        public void PlayPlaylist(int playlist)
        {
            ExecuteScript(String.Format("playPlaylistWrapper({0});", playlist));
        }

        public void PlayAlbum(int album)
        {
            ExecuteScript(String.Format("playAlbumWrapper({0});", album));
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

            if (songStatus == null) return;

            var status = (PlayerStatus)Enum.Parse(typeof(PlayerStatus), songStatus.status, true);

            Status = status;
            if (PlayerStatusChanged != null)
                    PlayerStatusChanged(this, new EventArgs());
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
               /* MessageBox.Show("Could not call method: " + method +
                    Environment.NewLine +
                    "Exception: " + e.Message); */
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
                MessageBox.Show("Could not call script: " + script +
                    Environment.NewLine +
                    "Exception: " + e.Message);
#endif
            }
        }
    }
}
