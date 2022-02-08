using MySql.Data.MySqlClient;
using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;

namespace Assignment
{
    public partial class ChangePassword : System.Web.UI.Page
    {
        string MYDBConnectionString = System.Configuration.ConfigurationManager.ConnectionStrings["HRDBConnection"].ConnectionString;
        string oldPassword;
        string newPassword;
        string oldDBHash;
        string oldDBSalt;
        static string newPasswordFinalHash;
        static string newPasswordSalt;
        byte[] Key;
        byte[] IV;
        protected void Page_Load(object sender, EventArgs e)
        {

        }

        protected void ChangePwd(object sender, EventArgs e)
        {
            SHA512Managed hashing = new SHA512Managed();
            InputXSSValidation();

            SqlConnection con = new SqlConnection(MYDBConnectionString);
            con.Open();

            oldDBHash = getDBHash((string)Session["LoggedIn"]);
            oldDBSalt = getDBSalt((string)Session["LoggedIn"]);

            string pwdWithSalt = oldPassword + oldDBSalt;
            byte[] hashWithSalt = hashing.ComputeHash(Encoding.UTF8.GetBytes(pwdWithSalt));
            string userHash = Convert.ToBase64String(hashWithSalt);
            if (userHash.Equals(oldDBHash))
            {
                try
                {
                    // ----- PASSWORD MIN AGE -----
                    // Get latest changed datetime
                    SqlCommand latestDateCmd = new SqlCommand("SELECT max(dateChanged) dateChanged FROM PasswordHistory WHERE email = @email;", con);
                    latestDateCmd.Parameters.AddWithValue("email", Session["LoggedIn"]);
                    SqlDataReader latestDateReader = latestDateCmd.ExecuteReader();

                    while (latestDateReader.Read())
                    {
                        if (latestDateReader["dateChanged"] == null || latestDateReader["dateChanged"] == DBNull.Value)
                        {
                            UpdatePassword();                        
                        }
                        else if ((DateTime.Now - (DateTime)latestDateReader["dateChanged"]).TotalMinutes > 2d)
                        {
                            UpdatePassword();
                        }
                        else
                        {
                            lbl_errorMsg.Text = "Please wait for 2 minutes before changing your password again";
                        }
                    }

                }
                catch (Exception ex)
                {
                    Console.WriteLine(ex);
                }

            }
            else
            {
                lbl_errorMsg.Text = "Wrong password entered";
            }

            con.Close();
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
                Console.WriteLine(ex);
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

            //Generate random salt
            RNGCryptoServiceProvider rng = new RNGCryptoServiceProvider();
            byte[] saltByte = new byte[8];

            //Fills array of bytes with a cryptographically strong sequence of random values.
            rng.GetBytes(saltByte);
            newPasswordSalt = Convert.ToBase64String(saltByte);

            SHA512Managed hashing = new SHA512Managed();

            string pwdWithSalt = newPassword + newPasswordSalt;
            byte[] plainHash = hashing.ComputeHash(Encoding.UTF8.GetBytes(newPassword));
            byte[] hashWithSalt = hashing.ComputeHash(Encoding.UTF8.GetBytes(pwdWithSalt));
            newPasswordFinalHash = Convert.ToBase64String(hashWithSalt);

            RijndaelManaged cipher = new RijndaelManaged();
            cipher.GenerateKey();
            Key = cipher.Key;
            IV = cipher.IV;
        }

        private void UpdatePassword()
        {
            SqlConnection con = new SqlConnection(MYDBConnectionString);

            // Update password
            SqlCommand setPassword = new SqlCommand("UPDATE AsgnUser SET password = @newPassword, passwordSalt = @newPasswordSalt WHERE email = @email;", con);
            using (SqlDataAdapter sda = new SqlDataAdapter())
            {
                setPassword.Parameters.AddWithValue("email", Session["LoggedIn"]);
                setPassword.Parameters.AddWithValue("newPassword", newPasswordFinalHash);
                setPassword.Parameters.AddWithValue("newPasswordSalt", newPasswordSalt);
                setPassword.ExecuteNonQuery();
            }

            // Put the old password to db (Password history)
            SqlCommand totalHistory = new SqlCommand("SELECT count(*) total FROM PasswordHistory WHERE email = @email;", con);
            totalHistory.Parameters.AddWithValue("email", Session["LoggedIn"]);

            SqlCommand delLowestPassword = new SqlCommand("DELETE FROM PasswordHistory " +
                "WHERE email = @email AND dateCreated = " +
                "(SELECT min(dateCreated) dateCreated FROM AsgnUser WHERE email = @email);");
            delLowestPassword.Parameters.AddWithValue("email", Session["LoggedIn"]);

            SqlCommand addLatestPassword = new SqlCommand("INSERT INTO PasswordHistory VALUES (@email, @dateChanged, @password, @passwordSalt);", con);
            addLatestPassword.Parameters.AddWithValue("email", Session["LoggedIn"]);
            addLatestPassword.Parameters.AddWithValue("dateChanged", DateTime.Now);
            addLatestPassword.Parameters.AddWithValue("password", oldDBHash);
            addLatestPassword.Parameters.AddWithValue("passwordSalt", oldDBSalt);

            SqlDataReader total = totalHistory.ExecuteReader();
            while (total.Read())
            {
                if (total["total"] != null && total["total"] != DBNull.Value)
                {
                    if ((int)total["total"] == 2)
                    {
                        using (SqlDataAdapter sda = new SqlDataAdapter())
                        {
                            delLowestPassword.ExecuteNonQuery();
                        }
                    }
                }

                using (SqlDataAdapter sda = new SqlDataAdapter())
                {
                    addLatestPassword.ExecuteNonQuery();
                }
            }
        }

        private void InputXSSValidation()
        {
            oldPassword = HttpUtility.HtmlEncode(tb_oldPassword.Text.ToString().Trim());
            newPassword = HttpUtility.HtmlEncode(tb_newPassword.Text.ToString().Trim());
        }
    }
}