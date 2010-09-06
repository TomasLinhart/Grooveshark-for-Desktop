using System.ComponentModel;
using System.Windows;
using System.Windows.Interop;
using System.Windows.Navigation;
using Microsoft.WindowsAPICodePack.Taskbar;
using mshtml;

namespace Grooveshark
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow
    {
        private GroovesharkPlayer m_player;

        public MainWindow()
        {
            InitializeComponent();
        }

        private void BrowserLoadCompleted(object sender, NavigationEventArgs e)
        {
            var document = browser.Document as IHTMLDocument2;
            if (document == null) return;
            
            m_player = new GroovesharkPlayer(document);

            CreateToolbarButtons();
        }

        private void CreateToolbarButtons()
        {
            var favorite = new ThumbnailToolbarButton(Properties.Resources.Favorite, "Add to Favorites");
            favorite.Click += (sender, args) => m_player.FavoriteSong();

            var next = new ThumbnailToolbarButton(Properties.Resources.Next, "Next Song");
            next.Click += (sender, args) => m_player.NextSong();

            var previous = new ThumbnailToolbarButton(Properties.Resources.Previous, "Previous Song");
            previous.Click += (sender, args) => m_player.PreviousSong();

            var playback = new ThumbnailToolbarButton(Properties.Resources.Play, "Play");
            playback.Click += (sender, args) => m_player.TogglePlayback();

            m_player.PlayerStatusChanged += (sender, args) =>
                                                {
                                                    if (m_player.Status == PlayerStatus.Playing ||
                                                        m_player.Status == PlayerStatus.Loading)
                                                    {
                                                        playback.Icon = Properties.Resources.Pause;
                                                        playback.Tooltip = "Pause";
                                                    }
                                                    else
                                                    {
                                                        playback.Icon = Properties.Resources.Play;
                                                        playback.Tooltip = "Play";
                                                    }
                                                };

            TaskbarManager.Instance.ThumbnailToolbars.AddButtons(
                new WindowInteropHelper(Application.Current.MainWindow).Handle,
                previous,
                playback,
                favorite,
                next);
        }

        protected override void OnClosing(CancelEventArgs e)
        {
            e.Cancel = true;
            WindowState = WindowState.Minimized;

            base.OnClosed(e);
        }
    }
}
