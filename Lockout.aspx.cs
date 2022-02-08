using System;
using System.Collections.Generic;
using System.Linq;
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

        }

        protected void UnlockAcc(object sender, EventArgs e)
        {
            if (Session["Lockout"] == null)
            {
                Response.Redirect("Login.aspx", true);
            }
            else
            {
                lbl_errorMsg.Text = "Account still in lockout. Please wait before trying again.";
            }
        }
    }
}