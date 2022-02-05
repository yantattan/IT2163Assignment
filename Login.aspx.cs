using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.IO;
using System.Linq;
using System.Net;
using System.Web;
using System.Web.Script.Serialization;
using System.Web.UI;
using System.Web.UI.WebControls;
using MySql.Data.MySqlClient;

namespace Assignment
{
    public partial class Login1 : System.Web.UI.Page
    {
        MySqlConnection con = new MySqlConnection(@"DataSource=(localdb)\MSSQLLocalDB;Initial Catalog=HRDB;Integrated Security=True;Connect Timeout=30;Encrypt=False;TrustServerCertificate=False;ApplicationIntent=ReadWrite;MultiSubnetFailover=False");

        string email;
        string password;
        private int tries = 0;

        protected void Page_Load(object sender, EventArgs e)
        {
            if (Session["LoggedIn"] != null)
            {
                
            }
        }

        protected void Login(object sender, EventArgs e)
        {
            if (ValidateCaptcha())
            {
                InputXSSValidation();
                AddSession();
                Response.Redirect("Homepage.aspx", false);
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

        private void AddSession()
        {
            email = tb_email.Text.ToString().Trim();
            password = tb_password.Text.ToString().Trim();

            if (email.Length > 0 && password.Length > 0)
            {
                tries++;
                // Check for email and password
                MySqlCommand getCreds = new MySqlCommand("SELECT * FROM AsgnUser WHERE email = @email AND password = @password", con);
                getCreds.Parameters.AddWithValue("@email", email);
                getCreds.Parameters.AddWithValue("@password", password);

                MySqlDataReader reader = getCreds.ExecuteReader();

                if (reader.Read())
                {
                    Session["LoggedIn"] = tb_email.Text.Trim();

                    // createa a new GUID and save into the session
                    string guid = Guid.NewGuid().ToString();
                    Session["AuthToken"] = guid;

                    // now create a new cookie with this guid value
                    Response.Cookies.Add(new HttpCookie("AuthToken", guid));

                    Response.Redirect("HomePage.aspx", false);
                }
                else
                {
                    if (tries < 3)
                    {
                        err_msg.Text = "Wrong username or password";
                    }
                    else
                    {

                    }
                    
                }
            }
            
        }
            
        

        public bool ValidateCaptcha()
        {
            bool result = true;
            string captchaResponse = Request.Form["g-recaptcha-response"];

            HttpWebRequest req = (HttpWebRequest)WebRequest.Create
            ("https://www.google.com/recaptcha/api/siteverify?secret=6LcTCDkdAAAAAA6AvYKZO2Im59qW7SzVICqtPyTB &response-" + captchaResponse);
            try
            {
                using (WebResponse wResponse = req.GetResponse())
                {
                    using (StreamReader readStream = new StreamReader(wResponse.GetResponseStream()))
                    {
                        //The response in JSON format
                        string jsonResponse = readStream.ReadToEnd();

                        //To show the JSON response string for learning purpose
                        lbl_gScore.Text = jsonResponse.ToString();

                        JavaScriptSerializer js = new JavaScriptSerializer();

                        //Create jsonObject to handle the response e.g success or Error
                        //Deserialize Json
                        MyObject jsonObject = js.Deserialize<MyObject>(jsonResponse);

                        //Convert the string "False" to bool false or "True" to bool true
                        result = Convert.ToBoolean(jsonObject.success);//

                    }
                }

                return result;
            }
            catch (WebException ex)
            {
                throw ex;
            }
        }


    }

    public class MyObject
    {
        public string success { get; set; }
        public List<string> ErrorMessage { get; set; }
    }
}