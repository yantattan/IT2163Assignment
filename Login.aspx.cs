using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Mail;
using System.Security.Cryptography;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Web;
using System.Web.Script.Serialization;
using System.Web.UI;
using System.Web.UI.WebControls;
using MySql.Data.MySqlClient;

namespace Assignment
{
    public partial class Login1 : System.Web.UI.Page
    {
        string MYDBConnectionString = System.Configuration.ConfigurationManager.ConnectionStrings["HRDBConnection"].ConnectionString;
        string email;
        string password;
        string errorMsg;

        protected void Page_Load(object sender, EventArgs e)
        {
            if (Request.Cookies["Lockout"] != null)
            {
                Response.Redirect("Lockout.aspx", false);
            }
        }

        protected void Login(object sender, EventArgs e)
        {
            if (ValidateCaptcha())
            {
                InputXSSValidation();
                CheckCredentials();
            }
            else
            {
                err_msg.Text = "Validate captcha to prove that you are a human.";
            }

        }

        private void InputXSSValidation()
        {
            email = HttpUtility.HtmlEncode(tb_email.Text.ToString().Trim());
            password = HttpUtility.HtmlEncode(tb_password.Text.ToString().Trim());
        }

        private void CheckCredentials()
        {
            SHA512Managed hashing = new SHA512Managed();
            string dbHash = getDBHash(email);
            string dbSalt = getDBSalt(email);

            if (email.Length > 0 && password.Length > 0)
            {
                if (Session["LoginAttempts"] == null)
                {
                    Session["LoginAttempts"] = 1;
                }
                else
                {
                    int attempts = (int)Session["LoginAttempts"];
                    attempts++;
                    Session["LoginAttempts"] = attempts;
                }
                System.Diagnostics.Debug.WriteLine(Session["LoginAttempts"]);

                // Check for email and password
                string pwdWithSalt = password + dbSalt;
                byte[] hashWithSalt = hashing.ComputeHash(Encoding.UTF8.GetBytes(pwdWithSalt));
                string userHash = Convert.ToBase64String(hashWithSalt);

                if (userHash.Equals(dbHash))
                {
                    SqlConnection con = new SqlConnection(MYDBConnectionString);
                    con.Open();
                    System.Diagnostics.Debug.WriteLine("Correctly matched");

                    Response.Redirect("2FAVerify.aspx?c="+HttpUtility.UrlEncode(email), false);
                }
                else
                {
                    if ((int)Session["LoginAttempts"] < 3)
                    {
                        err_msg.Text = "Wrong email or password";
                    }
                    else
                    {
                        Lockout();
                    }

                }
            }
            
        }

        protected string getDBHash(string email)
        {
            string h = null;
            SqlConnection con = new SqlConnection(MYDBConnectionString);
            SqlCommand command = new SqlCommand("SELECT password FROM AsgnUser WHERE email=@email", con);
            command.Parameters.AddWithValue("email", email);

            try
            {                        
                con.Open();
                using (SqlDataReader reader = command.ExecuteReader())
                {

                    while (reader.Read())
                    {
                        if (reader["password"] != null)
                        {
                            if (reader["password"] != DBNull.Value)
                            {
                                h = reader["password"].ToString();
                            }
                        }
                    }

                }
            }
            catch (Exception ex)
            {
                throw new Exception(ex.ToString());
            }
            finally { con.Close(); }
            return h;
        }

        protected string getDBSalt(string email)
        {
            string s = null;
            SqlConnection con = new SqlConnection(MYDBConnectionString);
            SqlCommand command = new SqlCommand("SELECT passwordSalt FROM AsgnUser WHERE email=@email", con);
            command.Parameters.AddWithValue("email", email);
            try
            {
                con.Open();
                using (SqlDataReader reader = command.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        if (reader["passwordSalt"] != null)
                        {
                            if (reader["passwordSalt"] != DBNull.Value)
                            {
                                s = reader["passwordSalt"].ToString();
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                throw new Exception(ex.ToString());
            }
            finally { con.Close(); }
            return s;
        }

        public bool ValidateCaptcha()
        {
            bool result = true;
            string captchaResponse = Request.Form["g-recaptcha-response"];

            HttpWebRequest req = (HttpWebRequest)WebRequest.Create
            ("https://www.google.com/recaptcha/api/siteverify?secret=6Lfa4WEeAAAAANFLY1S9ajTYsC5dBQGbWuPIEQQw&response=" + captchaResponse);
            try
            {
                using (WebResponse wResponse = req.GetResponse())
                {
                    using (StreamReader readStream = new StreamReader(wResponse.GetResponseStream()))
                    {
                        //The response in JSON format
                        string jsonResponse = readStream.ReadToEnd();

                        JavaScriptSerializer js = new JavaScriptSerializer();

                        //Create jsonObject to handle the response e.g success or Error
                        //Deserialize Json
                        MyObject jsonObject = js.Deserialize<MyObject>(jsonResponse);
                        if (!Boolean.Parse(jsonObject.success))
                            lbl_gScore.Text = "Invalid captcha!";

                        //Convert the string "False" to bool false or "True" to bool true
                        result = Convert.ToBoolean(jsonObject.success);
                        Console.WriteLine(result);
                    }
                }

                return result;
            }
            catch (WebException ex)
            {
                throw ex;
            }
        }

        private void Lockout()
        {
            Response.Cookies.Add(new HttpCookie("Lockout", "True"));
            Response.Cookies["Lockout"].Expires = DateTime.Now.AddMinutes(1);
            Session["LoginAttempts"] = null;
        }

    }

    public class MyObject
    {
        public string success { get; set; }
        public List<string> ErrorMessage { get; set; }
    }
}