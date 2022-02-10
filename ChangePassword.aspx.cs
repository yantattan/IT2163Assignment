using MySql.Data.MySqlClient;
using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Text.RegularExpressions;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;

namespace Assignment
{
    public partial class ChangePassword : System.Web.UI.Page
    {
        string MYDBConnectionString = System.Configuration.ConfigurationManager.ConnectionStrings["HRDBConnection"].ConnectionString;
        string oldPassword; // Raw input
        string newPassword; // Raw input
        string oldDBHash;
        string oldDBSalt;
        static string oldInputHash;
        static string newInputHash;
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

            oldDBHash = getDBHash((string)Session["LoggedIn"]);
            oldDBSalt = getDBSalt((string)Session["LoggedIn"]);

            HashPassword();

            string pwdWithSalt = oldPassword + oldDBSalt;
            byte[] hashWithSalt = hashing.ComputeHash(Encoding.UTF8.GetBytes(pwdWithSalt));
            oldInputHash = Convert.ToBase64String(hashWithSalt);

            bool toPart2 = false;

            // Check correct old password entered
            if (oldPassword.Length >= 12 && newPassword.Length >= 12 &&
                Regex.IsMatch(oldPassword, "[a-z]") && Regex.IsMatch(oldPassword, "[A-Z]") && 
                Regex.IsMatch(oldPassword, "[0-9]") && Regex.IsMatch(oldPassword, "[^a-zA-Z0-9]") &&
                Regex.IsMatch(newPassword, "[a-z]") && Regex.IsMatch(newPassword, "[A-Z]") && 
                Regex.IsMatch(newPassword, "[0-9]") && Regex.IsMatch(newPassword, "[^a-zA-Z0-9]"))
            {
                if (oldInputHash.Equals(oldDBHash))
                {
                    try
                    {
                        // ----- PASSWORD MIN AGE -----
                        // Get latest changed datetime
                        con.Open();
                        SqlCommand latestDateCmd = new SqlCommand("SELECT max(dateChanged) dateChanged FROM PasswordHistory WHERE email = @email;", con);
                        latestDateCmd.Parameters.AddWithValue("email", Session["LoggedIn"]);

                        using (SqlDataReader latestDateReader = latestDateCmd.ExecuteReader())
                        {
                            while (latestDateReader.Read())
                            {
                                // Check for min password age
                                if (latestDateReader["dateChanged"] == null || latestDateReader["dateChanged"] == DBNull.Value ||
                                    (DateTime.Now - (DateTime)latestDateReader["dateChanged"]).TotalMinutes > 0.5d)
                                {
                                    // Check if user reused the same password as before
                                    if (!newInputHash.Equals(oldDBHash))
                                    {
                                        toPart2 = true;
                                    }

                                    else
                                    {
                                        lbl_errorMsg.Text = "Please change your password to a different one";
                                    }

                                }
                                else
                                {
                                    lbl_errorMsg.Text = "Please wait for 2 minutes before changing your password again";
                                }
                            }
                        }

                    }
                    catch (Exception ex)
                    {
                        System.Diagnostics.Debug.WriteLine(ex);
                    }

                }
                else
                {
                    lbl_errorMsg.Text = "Wrong password entered";
                }
            }
            else
            {
                lbl_errorMsg.Text = "Password must contain minimum 12 characters, combination of lower-case, upper-case, numbers and special characters";
            }

            if (toPart2)
            {
                CheckPwdPart2();
            }
        }

        private void CheckPwdPart2()
        {
            SqlConnection con = new SqlConnection(MYDBConnectionString);
            con.Open();

            SqlCommand findReusedPasswordsCmd = new SqlCommand("SELECT id FROM PasswordHistory WHERE email = @email AND password = @newPassword;", con);
            findReusedPasswordsCmd.Parameters.AddWithValue("email", Session["LoggedIn"]);
            findReusedPasswordsCmd.Parameters.AddWithValue("newPassword", newPasswordFinalHash);
            using (SqlDataReader findReusedPasswordsReader = findReusedPasswordsCmd.ExecuteReader())
            {
                if (findReusedPasswordsReader.Read())
                {
                    while (findReusedPasswordsReader.Read())
                    {
                        System.Diagnostics.Debug.WriteLine("Here");
                        if (findReusedPasswordsReader["id"] == null || findReusedPasswordsReader["id"] == DBNull.Value)
                        {
                            UpdatePassword();
                            System.Diagnostics.Debug.WriteLine("Update password");
                        }
                        else
                        {
                            lbl_errorMsg.Text = "You are not allowed to reuse 2 of the latest passwords that you used previously";
                        }
                    }
                }
                else
                {
                    UpdatePassword();
                }

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
            SqlCommand command = new SqlCommand("SELECT passwordSalt FROM AsgnUser WHERE email = @email", con);
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
                System.Diagnostics.Debug.WriteLine(ex);
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

            // Hash new password
            string pwdWithSalt = newPassword + newPasswordSalt;
            byte[] hashWithSalt = hashing.ComputeHash(Encoding.UTF8.GetBytes(pwdWithSalt));
            newPasswordFinalHash = Convert.ToBase64String(hashWithSalt);

            // Hash new password with old salt
            string pwdWithSaltNew = newPassword + oldDBSalt;
            byte[] hashWithSaltNew = hashing.ComputeHash(Encoding.UTF8.GetBytes(pwdWithSaltNew));
            newInputHash = Convert.ToBase64String(hashWithSaltNew);

            // Hash entered old password
            string pwdWithSaltOld = oldPassword + oldDBSalt;
            byte[] hashWithSaltOld = hashing.ComputeHash(Encoding.UTF8.GetBytes(pwdWithSaltOld));
            oldInputHash = Convert.ToBase64String(hashWithSaltOld);

            RijndaelManaged cipher = new RijndaelManaged();
            cipher.GenerateKey();
            Key = cipher.Key;
            IV = cipher.IV;
        }

        private void UpdatePassword()
        {
            System.Diagnostics.Debug.WriteLine("Here yey3");
            SqlConnection con = new SqlConnection(MYDBConnectionString);
            con.Open();

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

            SqlCommand delLowestPassword = new SqlCommand("DELETE FROM [PasswordHistory] WHERE email = @email AND dateChanged = " +
                "(SELECT min(dateChanged) dateChanged FROM [PasswordHistory]);", con);
            delLowestPassword.Parameters.AddWithValue("email", Session["LoggedIn"]);
           
            SqlCommand addLatestPassword = new SqlCommand("INSERT INTO PasswordHistory VALUES (@dateChanged, @email, @password, @passwordSalt);", con);
            addLatestPassword.Parameters.AddWithValue("dateChanged", DateTime.Now);
            addLatestPassword.Parameters.AddWithValue("email", Session["LoggedIn"]);
            addLatestPassword.Parameters.AddWithValue("password", oldDBHash);
            addLatestPassword.Parameters.AddWithValue("passwordSalt", oldDBSalt);

            SqlDataReader total = totalHistory.ExecuteReader();
            while (total.Read())
            {
                if ((int)total["total"] == 2)
                {
                    using (SqlDataAdapter sda = new SqlDataAdapter())
                    {
                        delLowestPassword.ExecuteNonQuery();
                    }
                }

                using (SqlDataAdapter sda = new SqlDataAdapter())
                {
                    addLatestPassword.ExecuteNonQuery();
                }
            }

            con.Close();
            Response.Redirect("Homepage.aspx", false);
        }

        private void InputXSSValidation()
        {
            oldPassword = HttpUtility.HtmlEncode(tb_oldPassword.Text.ToString().Trim());
            newPassword = HttpUtility.HtmlEncode(tb_newPassword.Text.ToString().Trim());
        }
    }
}