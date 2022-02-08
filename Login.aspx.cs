using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.IO;
using System.Linq;
using System.Net;
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

        }

        protected void Login(object sender, EventArgs e)
        {
            if (ValidateCaptcha())
            {
                InputXSSValidation();
                AddSession();
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
            SHA512Managed hashing = new SHA512Managed();
            string dbHash = getDBHash(email);
            string dbSalt = getDBSalt(email);

            if (Session["Lockout"] != null)
            {
                Response.Redirect("Lockout.aspx", false);
            }
            else
            {
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

                        Session["LoggedIn"] = tb_email.Text.Trim();

                        // createa a new GUID and save into the session
                        string guid = Guid.NewGuid().ToString();
                        Session["AuthToken"] = guid;

                        // now create a new cookie with this guid value
                        Response.Cookies.Add(new HttpCookie("AuthToken", guid));

                        // --- MAXIMUM PASSWORD AGE ---
                        SqlCommand latestDateCmd = new SqlCommand("SELECT max(dateChanged) dateChanged FROM PasswordHistory WHERE email = @email;", con);
                        latestDateCmd.Parameters.AddWithValue("email", email);
                        SqlDataReader latestDateReader = latestDateCmd.ExecuteReader();
                        while (latestDateReader.Read())
                        {
                            if (latestDateReader["dateChanged"] != null && latestDateReader["dateChanged"] != DBNull.Value)
                            {
                                CheckMaxPasswordAge((DateTime)latestDateReader["dateChanged"]);
                            }
                            else
                            {
                                latestDateCmd = new SqlCommand("SELECT dateCreated FROM AsgnUser WHERE email = @email;", con);
                                latestDateCmd.Parameters.AddWithValue("email", email);
                                SqlDataReader latestDateReader2 = latestDateCmd.ExecuteReader();
                                while (latestDateReader2.Read())
                                {
                                    CheckMaxPasswordAge((DateTime)latestDateReader2["dateCreated"]);
                                }
  
                            }
                        }

                        SqlCommand log = new SqlCommand("INSERT INTO Log (time, email, action) VALUES (@time, @email, @action);", con);
                        log.Parameters.AddWithValue("time", DateTime.Now);
                        log.Parameters.AddWithValue("email", Session["LoggedIn"]);
                        log.Parameters.AddWithValue("action", "logged into the website");
                        using (SqlDataAdapter sda = new SqlDataAdapter())
                        {
                            log.ExecuteNonQuery();
                        }

                        con.Close();

                        Response.Redirect("HomePage.aspx", false);
                    }
                    else
                    {
                        if ((int)Session["LoginAttempts"] <= 3)
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

        private void HashPassword()
        {
            string passwordSalt;
            string passwordFinalHash;
            byte[] Key;
            byte[] IV;

            //Generate random salt
            RNGCryptoServiceProvider rng = new RNGCryptoServiceProvider();
            byte[] saltByte = new byte[8];

            //Fills array of bytes with a cryptographically strong sequence of random values.
            rng.GetBytes(saltByte);
            passwordSalt = Convert.ToBase64String(saltByte);

            SHA512Managed hashing = new SHA512Managed();

            string pwdWithSalt = password + passwordSalt;
            byte[] plainHash = hashing.ComputeHash(Encoding.UTF8.GetBytes(password));
            byte[] hashWithSalt = hashing.ComputeHash(Encoding.UTF8.GetBytes(pwdWithSalt));
            passwordFinalHash = Convert.ToBase64String(hashWithSalt);

            RijndaelManaged cipher = new RijndaelManaged();
            cipher.GenerateKey();
            Key = cipher.Key;
            IV = cipher.IV;

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
            Session["Lockout"] = true;
            Response.Redirect("Lockout.aspx", false);
            Task.Factory.StartNew(() =>
            {
                Thread.Sleep(10000);
                Session["Lockout"] = null;
            });
            
        }

        private async void CheckMaxPasswordAge(DateTime dateChanged)
        {
            if ((DateTime.Now - dateChanged).TotalMinutes > 60d)
            {
                err_msg.Text = "Your password has expired. Redirecting you to changing password in 3s ...";
                await Task.Delay(TimeSpan.FromSeconds(3));
                Response.Redirect("ChangePassword.aspx", true);
            }
        }

    }

    public class MyObject
    {
        public string success { get; set; }
        public List<string> ErrorMessage { get; set; }
    }
}