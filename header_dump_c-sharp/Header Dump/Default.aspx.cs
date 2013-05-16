using System;
using System.Collections.Specialized;
using System.Configuration;
using System.Data;
using System.Linq;
using System.Web;
using System.Web.Security;
using System.Web.UI;
using System.Web.UI.HtmlControls;
using System.Web.UI.WebControls;
using System.Web.UI.WebControls.WebParts;
using System.Xml.Linq;

public partial class _Default : System.Web.UI.Page 
{
    protected void Page_Load(object sender, EventArgs e)
    {
        NameValueCollection headers = Request.Headers;
        Response.Write("<h1>HTTP Request Headers</h1><hr />");
        foreach (string key in headers)
        {
            Response.Write("<b>" + key + "</b><br />" + headers[key] + "<br /><br />");
        }      
    }
}
