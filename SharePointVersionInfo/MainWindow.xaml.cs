//Author: Shantha Kumar T
//Website: http://www.ktskumar.com
//Twitter @ktskumar

using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Net;
using System.Security.Authentication;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace SharePointVersionInfo
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        BackgroundWorker backgroundWorker1;
        
        public MainWindow()
        {
            InitializeComponent();
            
            txtResult.Visibility = Visibility.Hidden;
            InitializeBackgroundWorker();
        }

        private void InitializeBackgroundWorker()
        {
            backgroundWorker1 = new BackgroundWorker();
            backgroundWorker1.DoWork += new DoWorkEventHandler(backgroundWorker1_DoWork);
            backgroundWorker1.RunWorkerCompleted += new RunWorkerCompletedEventHandler(backgroundWorker1_RunWorkerCompleted);
            backgroundWorker1.ProgressChanged += new ProgressChangedEventHandler(backgroundWorker1_ProgressChanged);
        }

        private void backgroundWorker1_ProgressChanged(object sender, ProgressChangedEventArgs e)
        { }

        private void backgroundWorker1_RunWorkerCompleted(object sender, RunWorkerCompletedEventArgs e)
        {
            this.Dispatcher.BeginInvoke((Action)delegate ()
            {
                this.btnGetVersion.IsEnabled = true;
            });
        }

        private void backgroundWorker1_DoWork(object sender, DoWorkEventArgs e)
        {
            RefreshForm();
        }

        private void RefreshForm()
        {
            string strUrl = "";
            string strVersion = null;
            string strAuthenticate = null;
            this.Dispatcher.BeginInvoke((Action)delegate ()
            {

                try
                {
                    this.btnGetVersion.IsEnabled = false;
                    txtResult.Visibility = Visibility.Hidden;
                    txtResult.Text = "";
                    strUrl = txtSiteUrl.Text;
                    if (strUrl != null && strUrl != "")
                    {

                        Uri url = new Uri(strUrl);
                        string formattedUrl = url.Scheme + "://" + url.Host + ":" + url.Port;
                        string serviceCNFurl = formattedUrl + "/_vti_pvt/service.cnf";
                        try
                        {
                            HttpWebRequest myRequest = (HttpWebRequest)WebRequest.Create(serviceCNFurl);
                            myRequest.Method = "GET";

                            myRequest.UserAgent = "Mozilla/5.0 (compatible; MSIE 9.0; Windows NT 6.1; WOW64; Trident/5.0)";
                            myRequest.Accept = "text/html, application/xhtml+xml, */*";

                            WebResponse myResponse = null;
                            string urlResult = "";
                            try
                            {
                                myResponse = myRequest.GetResponse();
                                StreamReader sr = new StreamReader(myResponse.GetResponseStream(), System.Text.Encoding.UTF8);
                                string result = sr.ReadToEnd();
                                sr.Close();
                                myResponse.Close();
                                urlResult = result;
                            }
                            catch (WebException ex)
                            {
                                HttpWebResponse response = (HttpWebResponse)ex.Response;
                                if (response != null)
                                {
                                    if (response.StatusCode == HttpStatusCode.Unauthorized)
                                    {
                                        strAuthenticate = response.GetResponseHeader("WWW-Authenticate");
                                        strVersion = response.GetResponseHeader("MicrosoftSharePointTeamServices");
                                    }
                                    else
                                    {
                                        txtResult.Text = string.Format("The following WebException was raised : {0}", ex.Message);
                                    }

                                }
                                else
                                {
                                    txtResult.Text = string.Format("Response Received from server was null");
                                }
                                goto unauthorizederror;
                            }


                            string[] splitResults = urlResult.Split(new[] { "\r\n" }, StringSplitOptions.RemoveEmptyEntries);

                            Dictionary<string, string> dictionary =
                                                  splitResults.ToDictionary(s => s.Split('|')[0], s => s.Split('|')[1]);
                            strVersion = dictionary["vti_extenderversion:SR"].Trim();


                            unauthorizederror:
                            Version ver = Version.Parse(strVersion);



                            string strEdition = "", strDesigner = "";
                            int majorVersion = ver.Major;
                            int minorVersion = ver.MinorRevision;
                            string updatePackage = "";
                            if (majorVersion >= 12 && majorVersion < 14)
                            {
                                //RTM 12.0.0.4518.1016
                                //SP1 12.0.0.6219
                                //SP2 12.0.0.6421
                                //SP3 12.0.0.6606

                                if (minorVersion >= 6606)
                                    updatePackage = "SP3";
                                else if (minorVersion >= 6421)
                                    updatePackage = "SP2";
                                else if (minorVersion >= 6219)
                                    updatePackage = "SP1";
                                else if (minorVersion <= 4518)
                                    updatePackage = "RTM";

                                strEdition = "SharePoint 2007" + " " + updatePackage;
                                strDesigner = "SharePoint Designer 2007";
                            }
                            else if (majorVersion >= 14.0 && majorVersion < 15)
                            {
                                //RTM 14.0.4762.1000
                                //SP1 14.0.6029.1000
                                //SP2 14.0.7015.1000


                                if (minorVersion >= 7015)
                                    updatePackage = "SP2";
                                else if (minorVersion >= 6029)
                                    updatePackage = "SP1";
                                else if (minorVersion <= 4762)
                                    updatePackage = "RTM";

                                strEdition = "SharePoint 2010" + " " + updatePackage;
                                strDesigner = "SharePoint Designer 2010";
                            }
                            else if (majorVersion >= 15.0 && majorVersion < 16)
                            {
                                //RTM 15.0.4420.1017
                                //SP1 15.0.4571.1502

                                if (minorVersion >= 4571)
                                    updatePackage = "SP1";
                                else if (minorVersion <= 4420)
                                    updatePackage = "RTM";


                                strEdition = "SharePoint 2013";
                                strDesigner = "SharePoint Designer 2013";
                            }
                            else if (majorVersion == 16.0)
                            {
                                if (ver.MinorRevision >= 1200)
                                    strEdition = "Office 365";
                                else
                                    strEdition = "SharePoint 2016";
                                strDesigner = "SharePoint Designer 2013";
                            }
                            else
                            {
                                strEdition = "SharePoint Team Services";
                                strDesigner = "Frontpage";
                            }



                            lblVersion.Content = strVersion;
                            lblUrl.Content = formattedUrl;
                            lblEdition.Content = strEdition;
                            lblEditor.Content = strDesigner;




                        }
                        catch (Exception ex)
                        {

                            txtResult.Visibility = Visibility.Visible;
                            txtResult.Text = "The Site not found or the given url is not a SharePoint Site.";

                        }
                    }
                    else
                    {
                        txtResult.Visibility = Visibility.Visible;
                        txtResult.Text = "Enter URL to get the SharePoint version";
                    }


                }
                catch (Exception ex)
                {
                    //this.Dispatcher.BeginInvoke((Action)delegate ()
                    //{
                    txtResult.Visibility = Visibility.Visible;
                    txtResult.Text = "Entered text is not a site url" + ex.Message;
                    //});

                }
            });
        }

        

        private void btnGetVersion_Click(object sender, RoutedEventArgs e)
        {
            if (backgroundWorker1.IsBusy != true)
            {
                backgroundWorker1.RunWorkerAsync();
            }
        }

        /// <summary>
        /// clcik event to minimize window
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void buttonMinimize_Click(object sender, RoutedEventArgs e)
        {
            // Set Window State to Minimized
            this.WindowState = WindowState.Minimized;
        }

        private void buttonRestore_Click(object sender, RoutedEventArgs e)
        {
            // If Window state is Maximized then set it to normal
            // If window state is Normal then maximize the window
            this.WindowState = this.WindowState == WindowState.Maximized ? WindowState.Normal : WindowState.Maximized;
        }

        private void buttonClose_Click(object sender, RoutedEventArgs e)
        {
            // Shutdown current application
            Application.Current.Shutdown();
        }

        private void labelApplicationTitle_MouseDown(object sender, MouseButtonEventArgs e)
        {
            // Maximize or restore window if left button is clicked two times
            // Allow window to be dragged if left button is clicked and hold only one time
            if (e.ChangedButton == MouseButton.Left)
            {
                if (e.ClickCount == 2)
                {
                    //this.WindowState = this.WindowState == WindowState.Maximized ? WindowState.Normal : WindowState.Maximized;
                }
                else
                {
                    Application.Current.MainWindow.DragMove();
                }
            }
        }
    }
}
