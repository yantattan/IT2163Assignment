using System;
using System.Collections.Generic;
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

        static string passwordFinalHash;
        static string passwordSalt;
        byte[] Key;
        byte[] IV;


        protected void Page_Load(object sender, EventArgs e)
        {

        }

        protected void btn_submit_Click(object sender, EventArgs e)
        {
            //Action[] functions = { checkStrongPassword, hashPassword, null };
            //Func<string, string>[] stringFunctions = { encryptData };
            var result = true;

            //for (var i= 0; i<functions.Length; i++)
            //{
            //    if (functions[i] != null)
            //        functions[i](); 
            //    else
            //        stringFunctions
            //}

            CheckStrongPassword();
            HashPassword();
            string cardNo = EncryptData(tb_cardNo.Text.ToString().Trim());
            string expireDate = EncryptData(tb_expireDate.ToString().Trim());
            string cvv = EncryptData(tb_cvv.ToString().Trim());

            if (result)
            {
                Response.Redirect("Login.aspx", false);
            }
        }

        private void CheckStrongPassword()
        {
            string password = tb_password.Text;

            bool valid = false;
            if (password.Length >= 12)
            {
                if (Regex.IsMatch(password, "[a-z]") && Regex.IsMatch(password, "[A-Z]") && Regex.IsMatch(password, "[0-9]") &&
                Regex.IsMatch(password, "[^a-zA-Z0-9]"))
                {
                    valid = true;
                }
            }

            if (!valid)
                err_password.Text = "Password must contain minimum 12 characters, combination of lower-case, upper-case, numbers and special characters";
        }

        private void HashPassword()
        {
            string password = tb_password.Text.ToString().Trim();

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
    }
}