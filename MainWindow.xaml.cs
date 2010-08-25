using System.Windows.Navigation;
using mshtml;

namespace Grooveshark
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow
    {
        public MainWindow()
        {
            InitializeComponent();
        }

        private void HideAdvertising()
        {
            //try
            //{
            //    browser.InvokeScript("hideAdvertisingBar");
            //} 
            //catch (Exception e)
            //{
            //    var msg = "Could not call script: " + e.Message;
            //    MessageBox.Show(msg);
            //}

            if (browser.Document == null) return;

            var document = browser.Document as IHTMLDocument2;
            if (document == null) return;

            document.parentWindow.execScript("javascript:(function(){document.getElementById('mainContainer').removeChild(document.getElementById('sidebar'));document.getElementById('mainContainer').insertBefore(document.getElementById('mainContent'),document.getElementById('mainContentWrapper'));})();");
        }

        private void BrowserLoadCompleted(object sender, NavigationEventArgs e)
        {
            HideAdvertising();
        }
    }
}
