/* ******************************************************************
 * PAP Push Sample Application
 * by John M. Wargo
 * www.bbdevfundamentals.com
 * 
 * The purpose of this sample application is to highlight the steps
 * required to push data to a BlackBerry device using PAP Push. The 
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
        //================================
        //Some constants used by the application. These values are all smooshed together
        //to make the push URL that gets passed to MDS. Replace the following values 
        //with the appropriate values for your environment        
        //================================
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
        //http://<BESName>:<BESPort>/pap
        //Build the URL to define our connection to the BES.
        string httpURL = "http://" + besName + ":" + besPort + "/pap";

        //Used to separate sections of the push message
        string BOUNDARY = "hajhakjdhakjyuwlanaje";
        
        //The email address for the destination device
        private static string pushRecipient = "someuser1%40somecompany.com";
     
        public Form1()
        {
            InitializeComponent();
            //Set the default push type in the dialog
            editPushType.SelectedIndex = 2;
        }

        private void button1_Click(object sender, EventArgs e)
        {
            textBox1.Text = "Beginning push process\n";
            //When the button is clicked, the application calls the appropriate
            //method for the push type being performed.
            switch (editPushType.SelectedIndex)
            {
                case 0: textBox1.AppendText(doBrowserCachePush());
                    break;
                case 1: textBox1.AppendText(doBrowserChannelDelete());
                    break;
                case 2: textBox1.AppendText(doBrowserChannelPush());
                    break;
                case 3: textBox1.AppendText(doBrowserMessagePush());
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

            //Set the Content-Type and Boundary
            HttpWReq.ContentType = "multipart/related; type=\"application/xml\"; boundary=" + BOUNDARY;

            StringBuilder dataToSend = new StringBuilder();

            //Build the XML section
            dataToSend.AppendLine("--" + BOUNDARY);
            dataToSend.AppendLine("Content-Type: application/xml; charset=UTF-8");

            //The following blank line is required - if you do not include this empty line the push does not work
            dataToSend.AppendLine("");
            dataToSend.AppendLine("<?xml version=\"1.0\"?>");
            dataToSend.AppendLine("<!DOCTYPE pap PUBLIC \"-//WAPFORUM//DTD PAP 2.0//EN\" ");
            dataToSend.AppendLine("\"http://www.wapforum.org/DTD/pap_2.0.dtd\" ");
            dataToSend.AppendLine("[<?wap-pap-ver supported-versions=\"2.0,1.*\"?>]>");
            dataToSend.AppendLine("<pap>");
            //every new push has to have a unique push-id, even if you are overwriting a previous push
            //to replace a previous push use a new push=id and push-replace-id="previousId" and replace-method="all" attributes            
            string myPushId = DateTime.Now.ToFileTime().ToString();
            dataToSend.AppendLine("<push-message push-id=\"my_push_app_" + myPushId + "\">");
            dataToSend.AppendLine("<address address-value=\"WAPPUSH=" + pushRecipient + "%3A7874/TYPE=USER@rim.net\"/>");
            dataToSend.AppendLine("</push-message>");
            dataToSend.AppendLine("</pap>");
            dataToSend.AppendLine("--" + BOUNDARY);

            //Build the Headers section
            dataToSend.AppendLine("Content-Type: text/html");
            dataToSend.AppendLine("X-RIM-Push-Type: Browser-Content");
            dataToSend.AppendLine("Content-Location: " + urlToPush);
            dataToSend.AppendLine("X-Rim-Push-Priority: medium");
            dataToSend.AppendLine("X-Rim-Push-Reliability: application-preferred");

            //Get the content of the page we are trying to push and add it to the dataToSend
            dataToSend.AppendLine(getInputPageStr(urlToPush));

            dataToSend.AppendLine("--" + BOUNDARY + "--");
            dataToSend.AppendLine("");

            dataToSend.Replace("\r\n", "EOL");
            dataToSend.Replace("\n", "EOL");

            dataToSend.Replace("EOL", "\r\n");

            Stream requestStream = null;
            string pushResult = "";
            //Put the dataToSend into the request stream to push to the user 
            try
            {
                requestStream = HttpWReq.GetRequestStream();
            }
            catch (Exception ex)
            {
                pushResult = "Push failed! " + ex.ToString();
            }

            byte[] outStr = new ASCIIEncoding().GetBytes(dataToSend.ToString());
            requestStream.Write(outStr, 0, outStr.Length);
            requestStream.Close();

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

            //Set the Content-Type and Boundary
            HttpWReq.ContentType = "multipart/related; type=\"application/xml\"; boundary=" + BOUNDARY;

            StringBuilder dataToSend = new StringBuilder();

            //Build the XML section
            dataToSend.AppendLine("--" + BOUNDARY);
            dataToSend.AppendLine("Content-Type: application/xml; charset=UTF-8");

            //The following blank line is required - if you do not include this empty line the push does not work
            dataToSend.AppendLine("");
            dataToSend.AppendLine("<?xml version=\"1.0\"?>");
            dataToSend.AppendLine("<!DOCTYPE pap PUBLIC \"-//WAPFORUM//DTD PAP 2.0//EN\" ");
            dataToSend.AppendLine("\"http://www.wapforum.org/DTD/pap_2.0.dtd\" ");
            dataToSend.AppendLine("[<?wap-pap-ver supported-versions=\"2.0,1.*\"?>]>");
            dataToSend.AppendLine("<pap>");
            //every new push has to have a unique push-id, even if you are overwriting a previous push
            //to replace a previous push use a new push=id and push-replace-id="previousId" and replace-method="all" attributes            
            string myPushId = DateTime.Now.ToFileTime().ToString();
            dataToSend.AppendLine("<push-message push-id=\"my_push_app_" + myPushId + "\">");
            dataToSend.AppendLine("<address address-value=\"WAPPUSH=" + pushRecipient + "%3A7874/TYPE=USER@rim.net\"/>");
            dataToSend.AppendLine("</push-message>");
            dataToSend.AppendLine("</pap>");
            dataToSend.AppendLine("--" + BOUNDARY);

            //Build the Headers section
            dataToSend.AppendLine("Content-Type: text/html");
            dataToSend.AppendLine("X-RIM-Push-Title: Contacts");
            dataToSend.AppendLine("X-RIM-Push-Type: Browser-Channel");
            dataToSend.AppendLine("X-RIM-Push-Channel-ID: Contacts-channel-id");
            dataToSend.AppendLine("Content-Location: " + urlToPush);
            //dataToSend.AppendLine("X-RIM-Push-UnRead-Icon-URL: http://dennis.neo.rr.com/contacts_unread.png");
            //dataToSend.AppendLine("X-RIM-Push-Read-Icon-URL: http://dennis.neo.rr.com/contacts_read.png");
            dataToSend.AppendLine("X-Rim-Push-Priority: medium");
            dataToSend.AppendLine("X-Rim-Push-Reliability: application-preferred");

            //Get the content of the page we are trying to push and add it to the dataToSend
            dataToSend.AppendLine(getInputPageStr(urlToPush));

            dataToSend.AppendLine("--" + BOUNDARY + "--");
            dataToSend.AppendLine("");

            dataToSend.Replace("\r\n", "EOL");
            dataToSend.Replace("\n", "EOL");

            dataToSend.Replace("EOL", "\r\n");

            Stream requestStream = null;
            string pushResult = "";
            //Put the dataToSend into the request stream to push to the user 
            try
            {
                requestStream = HttpWReq.GetRequestStream();
            }
            catch (Exception ex)
            {
                pushResult = "Push failed! " + ex.ToString();
            }

            byte[] outStr = new ASCIIEncoding().GetBytes(dataToSend.ToString());
            requestStream.Write(outStr, 0, outStr.Length);
            requestStream.Close();

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

            //Set the Content-Type and Boundary
            HttpWReq.ContentType = "multipart/related; type=\"application/xml\"; boundary=" + BOUNDARY;

            StringBuilder dataToSend = new StringBuilder();

            //Build the XML section
            dataToSend.AppendLine("--" + BOUNDARY);
            dataToSend.AppendLine("Content-Type: application/xml; charset=UTF-8");

            //The following blank line is required - if you do not include this empty line the push does not work
            dataToSend.AppendLine("");
            dataToSend.AppendLine("<?xml version=\"1.0\"?>");
            dataToSend.AppendLine("<!DOCTYPE pap PUBLIC \"-//WAPFORUM//DTD PAP 2.0//EN\" ");
            dataToSend.AppendLine("\"http://www.wapforum.org/DTD/pap_2.0.dtd\" ");
            dataToSend.AppendLine("[<?wap-pap-ver supported-versions=\"2.0,1.*\"?>]>");
            dataToSend.AppendLine("<pap>");
            //every new push has to have a unique push-id, even if you are overwriting a previous push
            //to replace a previous push use a new push=id and push-replace-id="previousId" and replace-method="all" attributes            
            string myPushId = DateTime.Now.ToFileTime().ToString();
            dataToSend.AppendLine("<push-message push-id=\"my_push_app_" + myPushId + "\">");
            dataToSend.AppendLine("<address address-value=\"WAPPUSH=" + pushRecipient + "%3A7874/TYPE=USER@rim.net\"/>");
            dataToSend.AppendLine("</push-message>");
            dataToSend.AppendLine("</pap>");
            dataToSend.AppendLine("--" + BOUNDARY);

            //Build the Headers section
            dataToSend.AppendLine("Content-Type: text/html");
            dataToSend.AppendLine("X-RIM-Push-Type: Browser-Channel-Delete");
            dataToSend.AppendLine("X-RIM-Push-Channel-ID: Contacts-channel-id");
            dataToSend.AppendLine("X-Rim-Push-Priority: medium");
            dataToSend.AppendLine("X-Rim-Push-Reliability: application-preferred");

            //Get the content of the page we are trying to push and add it to the dataToSend
            dataToSend.AppendLine(getInputPageStr(urlToPush));

            dataToSend.AppendLine("--" + BOUNDARY + "--");
            dataToSend.AppendLine("");

            dataToSend.Replace("\r\n", "EOL");
            dataToSend.Replace("\n", "EOL");

            dataToSend.Replace("EOL", "\r\n");

            Stream requestStream = null;
            string pushResult = "";
            //Put the dataToSend into the request stream to push to the user 
            try {
                requestStream = HttpWReq.GetRequestStream();
            } catch (Exception ex) {
                    pushResult = "Push failed! " + ex.ToString();
            }

            byte[] outStr = new ASCIIEncoding().GetBytes(dataToSend.ToString());
            requestStream.Write(outStr, 0, outStr.Length);
            requestStream.Close();

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

            //Set the Content-Type and Boundary
            HttpWReq.ContentType = "multipart/related; type=\"application/xml\"; boundary=" + BOUNDARY;

            StringBuilder dataToSend = new StringBuilder();

            //Build the XML section
            dataToSend.AppendLine("--" + BOUNDARY);
            dataToSend.AppendLine("Content-Type: application/xml; charset=UTF-8");

            //The following blank line is required - if you do not include this empty line the push does not work
            dataToSend.AppendLine("");
            dataToSend.AppendLine("<?xml version=\"1.0\"?>");
            dataToSend.AppendLine("<!DOCTYPE pap PUBLIC \"-//WAPFORUM//DTD PAP 2.0//EN\" ");
            dataToSend.AppendLine("\"http://www.wapforum.org/DTD/pap_2.0.dtd\" ");
            dataToSend.AppendLine("[<?wap-pap-ver supported-versions=\"2.0,1.*\"?>]>");
            dataToSend.AppendLine("<pap>");
            //every new push has to have a unique push-id, even if you are overwriting a previous push
            //to replace a previous push use a new push=id and push-replace-id="previousId" and replace-method="all" attributes            
            string myPushId = DateTime.Now.ToFileTime().ToString();
            dataToSend.AppendLine("<push-message push-id=\"my_push_app_" + myPushId + "\">");
            dataToSend.AppendLine("<address address-value=\"WAPPUSH=" + pushRecipient + "%3A7874/TYPE=USER@rim.net\"/>");
            dataToSend.AppendLine("</push-message>");
            dataToSend.AppendLine("</pap>");
            dataToSend.AppendLine("--" + BOUNDARY);

            //Build the Headers section
            dataToSend.AppendLine("Content-Type: text/html");
            dataToSend.AppendLine("X-RIM-Push-Title: Contacts");
            dataToSend.AppendLine("X-RIM-Push-Type: Browser-Message");
            dataToSend.AppendLine("X-RIM-Push-Channel-ID: Contacts-channel-id");
            dataToSend.AppendLine("Content-Location: " + urlToPush);
            dataToSend.AppendLine("X-Rim-Push-Priority: medium");
            dataToSend.AppendLine("X-Rim-Push-Reliability: application-preferred");

            //Get the content of the page we are trying to push and add it to the dataToSend
            dataToSend.AppendLine(getInputPageStr(urlToPush));

            dataToSend.AppendLine("--" + BOUNDARY + "--");
            dataToSend.AppendLine("");

            dataToSend.Replace("\r\n", "EOL");
            dataToSend.Replace("\n", "EOL");

            dataToSend.Replace("EOL", "\r\n");

            Stream requestStream = null;
            string pushResult = "";
            //Put the dataToSend into the request stream to push to the user 
            try
            {
                requestStream = HttpWReq.GetRequestStream();
            }
            catch (Exception ex)
            {
                pushResult = "Push failed! " + ex.ToString();
            }

            byte[] outStr = new ASCIIEncoding().GetBytes(dataToSend.ToString());
            requestStream.Write(outStr, 0, outStr.Length);
            requestStream.Close();

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
            {

            }
            return pageData;
        }
    }
}
