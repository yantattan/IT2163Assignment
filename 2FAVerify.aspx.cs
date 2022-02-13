using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Linq;
using System.Net;
using System.Net.Mail;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;

namespace Assignment
{
    public partial class _2FAVerify : System.Web.UI.Page
    {
        string MYDBConnectionString = System.Configuration.ConfigurationManager.ConnectionStrings["HRDBConnection"].ConnectionString;
        string twoFACode;
        string verifyCode;
        string email;
        protected void Page_Load(object sender, EventArgs e)
        {
            if (Request.Cookies["Lockout"] != null)
            {
                Response.Redirect("Lockout.aspx", true);
            }

            if (Session["LoggedIn"] != null && Session["AuthToken"] != null && Request.Cookies["AuthToken"] != null)
            {
                if (Session["AuthToken"].ToString().Equals(Request.Cookies["AuthToken"].Value))
                {
                    Response.Redirect("Homepage.aspx", false);
                }
            }

            email = Request.QueryString["c"];
            if (Session["Generated2FAOTP"] == null)
            {
                Gen2FA();
            }
        }

        protected void AddSession(object sender, EventArgs e)
        {
            InputXSSValidation();

            SqlConnection con = new SqlConnection(MYDBConnectionString);
            twoFACode = (string)Session["Generated2FAOTP"];
            if (verifyCode.Equals(twoFACode))
            {
                System.Diagnostics.Debug.WriteLine("Correctly matched");
                Session["Generated2FAOTP"] = null;

                SqlCommand checkEmailCmd = new SqlCommand("SELECT email FROM AsgnUser WHERE email = @email;", con);
                checkEmailCmd.Parameters.AddWithValue("email", email);
                con.Open();

                using (SqlDataReader checkEmailReader = checkEmailCmd.ExecuteReader())
                {
                    while (checkEmailReader.Read())
                    {
                        System.Diagnostics.Debug.WriteLine("while (checkEmailReader.Read())");
                        if (checkEmailReader["email"] != null && checkEmailReader["email"] != DBNull.Value)
                        {
                            System.Diagnostics.Debug.WriteLine("Found in DB");
                            Session["LoggedIn"] = email;
                            System.Diagnostics.Debug.WriteLine(Session["LoggedIn"]);

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

                            Response.Redirect("HomePage.aspx", false);
                        }
                        else
                        {
                            lbl_errMsg.Text = "An error occurred. Please go back to login page and try logging in again";
                        }
                    }
                }
                con.Close();

            }
            else
            {
                lbl_errMsg.Text = "Wrong verification code entered";
            }

        }

        protected void ResendOTP(object sender, EventArgs e)
        {
            email = Request.QueryString["c"];
            Gen2FA();
        }

        private void Gen2FA()
        {
            // Gen 6 digit code
            Random rng = new Random();
            twoFACode = rng.Next(0, 1000000).ToString("000000");
            Session["Generated2FAOTP"] = twoFACode;

            // Mailing Service
            SqlConnection con = new SqlConnection(MYDBConnectionString);
            con.Open();
            SqlCommand getMailServiceCreds = new SqlCommand("SELECT * FROM MailServiceCredentials", con);
            
            using (SqlDataReader getMailServiceCredsReader = getMailServiceCreds.ExecuteReader()) 
            {
                while (getMailServiceCredsReader.Read())
                {
                    MailMessage mail = new MailMessage();
                    mail.To.Add(email);
                    mail.From = new MailAddress((string)getMailServiceCredsReader["mailEmail"], "SITConnect Online Stationary Store", Encoding.UTF8);
                    mail.Subject = "2FA Code for logging in";
                    mail.SubjectEncoding = Encoding.UTF8;
                    mail.Body = $"Your verification code:<br/> <h3>{twoFACode}<h3>";
                    mail.BodyEncoding = Encoding.UTF8;
                    mail.IsBodyHtml = true;

                    SmtpClient client = new SmtpClient();
                    client.Credentials = new NetworkCredential((string)getMailServiceCredsReader["mailEmail"], (string)getMailServiceCredsReader["mailPassword"]);
                    client.Port = 587;
                    client.Host = "smtp.gmail.com";
                    client.EnableSsl = true;
                    try
                    {
                        client.Send(mail);
                        System.Diagnostics.Debug.WriteLine("Mail sent");
                    }
                    catch (Exception ex)
                    {
                        System.Diagnostics.Debug.WriteLine(ex);
                    }
                }
            }
            
            con.Close();
        }

        private void InputXSSValidation()
        {
            verifyCode = HttpUtility.HtmlEncode(tb_verificationCode.Text.ToString().Trim());
            System.Diagnostics.Debug.WriteLine(verifyCode);
        }

        private async void CheckMaxPasswordAge(DateTime dateChanged)
        {
            if ((DateTime.Now - dateChanged).TotalMinutes > 5d)
            {
                System.Diagnostics.Debug.WriteLine("Checking max password age");
                await Task.Delay(TimeSpan.FromSeconds(3));
                Response.Redirect("ChangePassword.aspx", false);
            }
        }
    }
}