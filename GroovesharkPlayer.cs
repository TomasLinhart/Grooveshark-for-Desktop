using System;
using System.Windows;
using mshtml;

namespace Grooveshark
{
    public class GroovesharkPlayer
    {
        private IHTMLDocument2 m_document;

        public GroovesharkPlayer(IHTMLDocument2 document)
        {
            if (document == null)
                throw new ArgumentNullException();

            m_document = document;

            AddWrappers();

            HideAdvertising();
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
            ExecuteScript(String.Format("voteSongWrapper({0});", theme));
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
