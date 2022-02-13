using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;

namespace Assignment
{
    public partial class Lockout : System.Web.UI.Page
    {
        protected void Page_Load(object sender, EventArgs e)
        {
            if (Request.Cookies["Lockout"] != null)
            {
                lockout_time.Text = "60000";
            }
            else
            {
                Response.Redirect("Login.aspx", false);
            }

        }

    }
}