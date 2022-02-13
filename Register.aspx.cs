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
    public partial class Login : System.Web.UI.Page
    {
        string fname;
        string lname;
        string email;
        string password;
        string creditNo;
        string creditCVV;
        string MYDBConnectionString = System.Configuration.ConfigurationManager.ConnectionStrings["HRDBConnection"].ConnectionString;
        static string passwordFinalHash;
        static string passwordSalt;
        byte[] Key;
        byte[] IV;
        internal static bool lockout;

        protected void Page_Load(object sender, EventArgs e)
        {

        }

        protected void btn_submit_Click(object sender, EventArgs e)
        {
            RemoveAllErrorMsg();
            InputXSSValidation();
            if (CheckRegex()) 
            {
                HashPassword();
                CreateAccount();
            }
        }

        protected void CreateAccount()
        {
            try
            {
                using (SqlConnection con = new SqlConnection(MYDBConnectionString))
                {
                    using (SqlCommand addAcc = new SqlCommand("INSERT INTO AsgnUser VALUES " +
                    "(@email, @fname, @lname, @dob, @photo, @creditNo, @creditExpireDate, @creditCVV, @password, @passwordSalt, @dateCreated, @IV, @Key);"))
                    {
                        using (SqlDataAdapter sda = new SqlDataAdapter())
                        {
                            string fileName = fu_photo.FileName.ToString();
                            string creditExpireDateEnc = EncryptData(tb_expireDate.Text.ToString().Trim());
                            string creditNoEnc = EncryptData(creditNo);
                            string creditCVVEnc = EncryptData(creditCVV);

                            addAcc.Parameters.AddWithValue("email", email);
                            addAcc.Parameters.AddWithValue("fname", fname);
                            addAcc.Parameters.AddWithValue("lname", lname);
                            addAcc.Parameters.AddWithValue("dob", DateTime.Parse(tb_dob.Text.ToString().Trim()));
                            addAcc.Parameters.AddWithValue("photo", fileName);
                            addAcc.Parameters.AddWithValue("creditNo", creditNoEnc);
                            addAcc.Parameters.AddWithValue("creditExpireDate", creditExpireDateEnc);
                            addAcc.Parameters.AddWithValue("creditCVV", creditCVVEnc);
                            addAcc.Parameters.AddWithValue("password", passwordFinalHash);
                            addAcc.Parameters.AddWithValue("passwordSalt", passwordSalt);
                            addAcc.Parameters.AddWithValue("dateCreated", DateTime.Now);
                            addAcc.Parameters.AddWithValue("IV", Convert.ToBase64String(IV));
                            addAcc.Parameters.AddWithValue("Key", Convert.ToBase64String(Key));

                            addAcc.Connection = con;
                            con.Open();
                            fu_photo.PostedFile.SaveAs(Server.MapPath("~/ImageUploads/") + fileName);
                            addAcc.ExecuteNonQuery();
                            con.Close();
                        }
                    }

                    SqlCommand log = new SqlCommand("INSERT INTO Log (time, email, action) VALUES (@time, @email, @action);", con);
                    log.Parameters.AddWithValue("time", DateTime.Now);
                    log.Parameters.AddWithValue("email", email);
                    log.Parameters.AddWithValue("action", "account registered to the website");
                    using (SqlDataAdapter sda = new SqlDataAdapter())
                    {
                        con.Open();
                        log.ExecuteNonQuery();
                        con.Close();
                    }

                    Response.Redirect("Login.aspx", true);
                }
            }
            catch (SqlException)
            {
                err_msg.Text = "Invalid input field entered, or an existing user already exists";
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex);
            }
        }

        private bool CheckRegex()
        {
            bool valid = true;
            if (password.Length < 12)
            {
                if (!(Regex.IsMatch(password, "[a-z]") && Regex.IsMatch(password, "[A-Z]") && Regex.IsMatch(password, "[0-9]") &&
                Regex.IsMatch(password, "[^a-zA-Z0-9]")) )
                {
                    valid = false;
                }
            }

            if (!valid)
                err_password.Text = "Password must contain minimum 12 characters, combination of lower-case, upper-case, numbers and special characters";

            if (!Regex.IsMatch(fname, "[a-zA-Z0-9]{1,}$"))
            {
                err_fname.Text = "First name must not contain special characters";
                valid = false;
            }

            if (!Regex.IsMatch(lname, "[a-zA-Z0-9]{1,}$"))
            {
                err_lname.Text = "Last name must not contain special characters";
                valid = false; 
            }

            if (!Regex.IsMatch(email, @"^([\w\.\-]+)@([\w\-]+)((\.(\w){2,3})+)$"))
            {
                err_email.Text = "Email must be in the correct format - example@example.somthing";
                valid = false;
            }   

            if (!Regex.IsMatch(creditCVV, "[0-9]{3,4}$"))
            {
                err_cvv.Text = "Credit card CVV must only contain numbers and have 3-4 digits only";
                valid = false;
            }

            return valid;
        }

        private void HashPassword()
        {

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

        protected string EncryptData(string data)
        {
            byte[] cipherText = null;
            try
            {
                RijndaelManaged cipher = new RijndaelManaged();
                cipher.IV = IV;
                cipher.Key = Key;
                ICryptoTransform encryptTransform = cipher.CreateEncryptor();
                
                byte[] plainText = Encoding.UTF8.GetBytes(data);
                cipherText = encryptTransform.TransformFinalBlock(plainText, 0, plainText.Length);

            }
            catch (Exception ex)
            {
                throw new Exception(ex.ToString());
            }

            finally { }
            return Convert.ToBase64String(cipherText);
        }

        private void InputXSSValidation()
        {
            email = tb_email.Text.ToString().Trim();
            password = HttpUtility.HtmlEncode(tb_password.Text.ToString().Trim());
            fname = HttpUtility.HtmlEncode(tb_fname.Text.ToString().Trim());
            lname = HttpUtility.HtmlEncode(tb_lname.Text.ToString().Trim());
            creditNo = HttpUtility.HtmlEncode(tb_cardNo.Text.ToString().Trim());
            creditCVV = HttpUtility.HtmlEncode(tb_cvv.Text.ToString().Trim());
        }

        private void RemoveAllErrorMsg()
        {
            err_fname.Text = "";
            err_lname.Text = "";
            err_email.Text = "";
            err_cvv.Text = "";
            err_password.Text = "";
            err_msg.Text = "";
        }
    }
}