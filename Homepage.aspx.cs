using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Linq;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;

namespace Assignment
{
    public partial class Homepage : System.Web.UI.Page
    {
        string MYDBConnectionString = System.Configuration.ConfigurationManager.ConnectionStrings["HRDBConnection"].ConnectionString;
        protected void Page_Load(object sender, EventArgs e)
        {
            if (Session["Lockout"] != null)
            {
                Response.Redirect("Lockout.aspx", true);
            }

            if (Session["LoggedIn"] != null && Session["AuthToken"] != null && Request.Cookies["AuthToken"] != null)
            {
                if (!Session["AuthToken"].ToString().Equals(Request.Cookies["AuthToken"].Value))
                {
                    Response.Redirect("Login.aspx", false);
                }

                SqlConnection con = new SqlConnection(MYDBConnectionString);
                SqlCommand getNameCmd = new SqlCommand("SELECT fname, lname FROM AsgnUser WHERE email = @email", con);
                getNameCmd.Parameters.AddWithValue("email", Session["LoggedIn"]);

                con.Open();
                using (SqlDataReader reader = getNameCmd.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        if (reader["fname"] != DBNull.Value && reader["lname"] != DBNull.Value)
                        {
                            lbl_fname.Text = (string)reader["fname"];
                            lbl_lname.Text = (string)reader["lname"];
                        }
                    }
                }
                con.Close();
            }
            else
            {
                Response.Redirect("Login.aspx", false);
            }

        }

        protected void Logout(object sender, EventArgs e)
        {

            SqlConnection con = new SqlConnection(MYDBConnectionString);
            SqlCommand log = new SqlCommand("INSERT INTO Log (time, email, action) VALUES (@time, @email, @action);", con);
            log.Parameters.AddWithValue("time", DateTime.Now);
            log.Parameters.AddWithValue("email", Session["LoggedIn"]);
            log.Parameters.AddWithValue("action", "signed out of the website");

            con.Open();
            using (SqlDataAdapter sda = new SqlDataAdapter())
            {
                log.ExecuteNonQuery();
            }
            con.Close();


            Session.Clear();
            Session.Abandon();
            Session.RemoveAll();

            Response.Redirect("Login.aspx", false);

            if (Request.Cookies["ASP.NET_SessionId"] != null)
            {
                Response.Cookies["ASP.NET_SessionId"].Value = string.Empty;
                Response.Cookies["ASP.NET_SessionId"].Expires = DateTime.Now.AddMonths(-20);
            }

            if (Request.Cookies["AuthToken"] != null)
            {
                Response.Cookies["AuthToken"].Value = string.Empty;
                Response.Cookies["AuthToken"].Expires = DateTime.Now.AddMonths(-20);
            }

        }
    }
}