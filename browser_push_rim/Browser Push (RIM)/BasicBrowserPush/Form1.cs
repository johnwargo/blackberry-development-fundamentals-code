/* ******************************************************************
 * RIM Push Sample Application
 * by John M. Wargo
 * www.bbdevfundamentals.com
 * 
 * Special thanks to Brent Thornton for his help with this 
 * application. 
 * 
 * The purpose of this sample application is to highlight the steps
 * required to push data to a BlackBerry device using RIM Push. The 
 * application contains code that illustrates each of the possible
 * browser push types (cache push, channel push, channel delete and 
 * message push). 
 * 
 * As you read through the code, you will quickly see that there are
 * much easier ways to do the push than what is shown here. It's 
 * important to note that this example is provided to illustrate 
 * what an application has to do to perform each of the the pushes. 
 * Rather than have a bunch of if/then statements to build the data
 * being pushed to MDS, I chose instead to isolate each example as 
 * a complete segment of code - making, I believe, it easier to 
 * understand what the application is doing. 
 * 
 * In all of RIM's examples, the parameters for the push request 
 * are all read from a property file and a single push function 
 * (with a bunch of conditional expressions) is used to illustrate
 * how to do the push. I thought this approach would be easier
 * from a learning standpoint. Feel free to rewrite this in a more 
 * efficient way for your own push applications. 
 *********************************************************************/
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using System.Net;
using System.IO;

namespace BasicBrowserPush
{
    public partial class Form1 : Form
    {
        //Some constants used by the application. These values are all smooshed together
        //to make the push URL that gets passed to MDS.
        //Replace the following values with the appropriate values for your environment
        private static string pushRecipient = "someuser1%40somecompany.com";
        //besName needs to be a fully resolvable host name for the BES or the system
        //running the MDS simulator
        private static string besName = "dennis.neo.rr.com";
        //This is the listen port MDS is listening on for push requests. You should only
        //need to change from the default if your instance of MDS is configured for another
        //listen port than the default.
        private static string besPort = "8080";
        //This is the URL pointing to the web page (hosted on a web server) being pushed
        //to the device in this example.
        private static string urlToPush = "http://dennis.neo.rr.com/contacts.html";
        //Create the push URL for MDS this is of the format
        //http://<BESName>:<BESPort>/push?DESTINATTION=<PIN/EMAIL>&PORT=<PushPort>&REQUESTURI=/
        string httpURL = "http://" + besName + ":" + besPort
            + "/push?DESTINATION=" + pushRecipient + "&PORT=7874"
            + "&REQUESTURI=/";

        public Form1()
        {
            InitializeComponent();
            //Set the default push type in the dialog
            editPushType.SelectedIndex = 2;
        }

        private void button1_Click(object sender, EventArgs e)
        {
            //When the button is clicked, the application calls the appropriate
            //method for the push type being performed.
            switch (editPushType.SelectedIndex)
            {
                case 0: textBox1.Text = doBrowserCachePush();
                    break;
                case 1: textBox1.Text = doBrowserChannelDelete();
                    break;
                case 2: textBox1.Text = doBrowserChannelPush();
                    break;
                case 3: textBox1.Text = doBrowserMessagePush();
                    break;
            }            
        }

        private string doBrowserCachePush()
        {
            HttpWebResponse HttpWRes = null;
            HttpWebRequest HttpWReq = null;

            //Create the HTTP request
            HttpWReq = (HttpWebRequest)WebRequest.Create(httpURL);
            HttpWReq.Method = ("POST");

            //Add the headers to the HTTP request
            HttpWReq.Headers.Add("Content-Location", urlToPush);
            HttpWReq.Headers.Add("X-RIM-Push-Type", "Browser-Content");
            HttpWReq.Headers.Add("X-Rim-Push-Priority", "medium");

            //Set the page to expire in a week. 
            DateTime d = DateTime.Now;
            d = d.AddDays(7);
            HttpWReq.Headers.Add("Expires", d.ToString("r"));

            //Put the pageData into the request stream to push to the users cache
            Stream requestStream = HttpWReq.GetRequestStream();
            byte[] pageData = getInputPage(urlToPush);
            requestStream.Write(pageData, 0, pageData.Length);
            requestStream.Close();

            //Initialize the push result string
            string pushResult = "";
            //Do the push
            try
            {
                HttpWRes = (HttpWebResponse)HttpWReq.GetResponse();
            }
            catch (Exception ex)
            {
                //push failed
                pushResult = "Push failed! " + ex.ToString();

            }
            if ((HttpWRes != null) && (HttpWRes.StatusCode == HttpStatusCode.OK || HttpWRes.StatusCode == HttpStatusCode.Accepted))
            {
                //successful push
                pushResult = "Successful Push!";
            }

            //clean up
            if (HttpWRes != null)
            {
                HttpWRes.Close();
            }
            return pushResult;
        }

        private string doBrowserChannelPush()
        {
            HttpWebResponse HttpWRes = null;
            HttpWebRequest HttpWReq = null;

            //Create the HTTP request
            HttpWReq = (HttpWebRequest)WebRequest.Create(httpURL);
            HttpWReq.Method = ("POST");

            //Add the headers to the HTTP request
            HttpWReq.Headers.Add("Content-Location", urlToPush);
            HttpWReq.Headers.Add("X-RIM-Push-Title", "Contacts");
            HttpWReq.Headers.Add("X-RIM-Push-Type", "Browser-Channel");
            HttpWReq.Headers.Add("X-RIM-Push-Channel-ID", "contacts-channel-id");
            HttpWReq.Headers.Add("X-RIM-Push-UnRead-Icon-URL", "http://dennis.neo.rr.com/contacts_unread.png");
            HttpWReq.Headers.Add("X-RIM-Push-Read-Icon-URL", "http://dennis.neo.rr.com/contacts_read.png");
            HttpWReq.Headers.Add("X-Rim-Push-Priority", "medium");

            //Set the page to expire in a week. 
            DateTime d = DateTime.Now;
            d = d.AddDays(7);
            HttpWReq.Headers.Add("Expires", d.ToString("r"));

            //Put the pageData into the request stream to push to the users cache
            Stream requestStream = HttpWReq.GetRequestStream();
            byte[] pageData = getInputPage(urlToPush);
            requestStream.Write(pageData, 0, pageData.Length);
            requestStream.Close();

            string pushResult = "";
            //Do the push
            try
            {
                HttpWRes = (HttpWebResponse)HttpWReq.GetResponse();
            }
            catch (Exception ex)
            {
                //push failed
                pushResult = "Push failed! " + ex.ToString();

            }
            if ((HttpWRes != null) && (HttpWRes.StatusCode == HttpStatusCode.OK || HttpWRes.StatusCode == HttpStatusCode.Accepted))
            {
                //successful push
                pushResult = "Successful Push!";
            }

            //clean up
            if (HttpWRes != null)
            {
                HttpWRes.Close();
            }
            return pushResult;
        }

        private string doBrowserChannelDelete()
        {
            HttpWebResponse HttpWRes = null;
            HttpWebRequest HttpWReq = null;

            //Create the HTTP request
            HttpWReq = (HttpWebRequest)WebRequest.Create(httpURL);
            HttpWReq.Method = ("POST");

            //Add the headers to the HTTP request
            HttpWReq.Headers.Add("Content-Location", urlToPush);
            HttpWReq.Headers.Add("X-RIM-Push-Type", "Browser-Channel-Delete");
            HttpWReq.Headers.Add("X-RIM-Push-Channel-ID", "contacts-channel-id");

            //Put the pageData into the request stream to push to the users cache
            Stream requestStream = HttpWReq.GetRequestStream();
            byte[] pageData = getInputPage(urlToPush);
            requestStream.Write(pageData, 0, pageData.Length);
            requestStream.Close();

            string pushResult = "";
            //Do the push
            try
            {
                HttpWRes = (HttpWebResponse)HttpWReq.GetResponse();
            }
            catch (Exception ex)
            {
                //push failed
                pushResult = "Push failed! " + ex.ToString();

            }
            if ((HttpWRes != null) && (HttpWRes.StatusCode == HttpStatusCode.OK || HttpWRes.StatusCode == HttpStatusCode.Accepted))
            {
                //successful push
                pushResult = "Successful Push!";
            }

            //clean up
            if (HttpWRes != null)
            {
                HttpWRes.Close();
            }
            return pushResult;
        }

        private string doBrowserMessagePush()
        {
            HttpWebResponse HttpWRes = null;
            HttpWebRequest HttpWReq = null;

            //Create the HTTP request
            HttpWReq = (HttpWebRequest)WebRequest.Create(httpURL);
            HttpWReq.Method = ("POST");

            //Add the headers to the HTTP request
            HttpWReq.Headers.Add("Content-Location", urlToPush);
            HttpWReq.Headers.Add("X-RIM-Push-Title", "Contacts");
            HttpWReq.Headers.Add("X-RIM-Push-Type", "Browser-Message");
            HttpWReq.Headers.Add("X-RIM-Push-Channel-ID", "contacts-channel-id");
            HttpWReq.Headers.Add("X-Rim-Push-Priority", "medium");

            //Set the page to expire in a week. 
            DateTime d = DateTime.Now;
            d = d.AddDays(7);
            HttpWReq.Headers.Add("Expires", d.ToString("r"));

            //Put the pageData into the request stream to push to the users cache
            Stream requestStream = HttpWReq.GetRequestStream();
            byte[] pageData = getInputPage(urlToPush);
            requestStream.Write(pageData, 0, pageData.Length);
            requestStream.Close();

            string pushResult = "";
            //Do the push
            try
            {
                HttpWRes = (HttpWebResponse)HttpWReq.GetResponse();
            }
            catch (Exception ex)
            {
                //push failed
                pushResult = "Push failed! " + ex.ToString();

            }
            if ((HttpWRes != null) && (HttpWRes.StatusCode == HttpStatusCode.OK || HttpWRes.StatusCode == HttpStatusCode.Accepted))
            {
                //successful push
                pushResult = "Successful Push!";
            }

            //clean up
            if (HttpWRes != null)
            {
                HttpWRes.Close();
            }
            return pushResult;
        }

        /// <summary>
        /// Get the data of the page you are trying to push
        /// </summary>
        /// <param name="location">URL of page to push.</param>
        /// <returns></returns>
        private byte[] getInputPage(string location)
        {
            byte[] pageData = null;
            try
            {
                HttpWebRequest contentRequest = (HttpWebRequest)WebRequest.Create(location);
                //set the header to blackberry incase end site is checking where the request is coming from
                contentRequest.UserAgent = "BlackBerry";

                //make the HTTP request
                HttpWebResponse contentResponse = (HttpWebResponse)contentRequest.GetResponse();
                if (contentResponse.StatusCode == HttpStatusCode.OK)
                {
                    Stream responseStream = contentResponse.GetResponseStream();
                    StreamReader reader = new StreamReader(responseStream);

                    //get the page data and convert to byte[]
                    string strPageData = reader.ReadToEnd();
                    pageData = new ASCIIEncoding().GetBytes(strPageData);
                    reader.Close();
                    responseStream.Close();

                }
                contentResponse.Close();
            }
            catch (Exception e)
            { }
            return pageData;
        }

        /// <summary>
        /// Get the data of the page you are trying to push in string format
        /// </summary>
        /// <param name="location">URL of page to push.</param>
        /// <returns></returns>
        private string getInputPageStr(string location)
        {
            string pageData = String.Empty;
            try
            {
                HttpWebRequest contentRequest = (HttpWebRequest)WebRequest.Create(location);
                //set the header to blackberry incase end site is checking where the request is coming from
                contentRequest.UserAgent = "BlackBerry";

                //make the HTTP request
                HttpWebResponse contentResponse = (HttpWebResponse)contentRequest.GetResponse();
                if (contentResponse.StatusCode == HttpStatusCode.OK)
                {
                    Stream responseStream = contentResponse.GetResponseStream();
                    StreamReader reader = new StreamReader(responseStream);

                    //get the page data
                    pageData = reader.ReadToEnd();
                    
                    reader.Close();
                    responseStream.Close();
                }
                contentResponse.Close();
            }
            catch (Exception e)
            { }
            return pageData;
        }
    }
}
