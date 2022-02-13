<%@ Page Language="C#" AutoEventWireup="true" CodeBehind="Register.aspx.cs" Inherits="Assignment.Login" ValidateRequest="false" %>

<!DOCTYPE html>

<html xmlns="http://www.w3.org/1999/xhtml">
<head runat="server">
    <title>Sign up an account</title>
</head>
<body>
    <h1>Register an acocunt</h1>
    <br />
    <form id="login_form" runat="server">
        <div>
            <asp:Label runat="server" ID="err_msg" ForeColor="Red"></asp:Label>

            <asp:Table ID="Table1" runat="server">

                <asp:TableRow>
                    <asp:TableCell>
                        <asp:Label ID="lbl_photo" runat="server" Text="Photo"></asp:Label>
                    </asp:TableCell>
                    <asp:TableCell>
                        <asp:FileUpload ID="fu_photo" runat="server" />
                        <br style="padding-bottom:20px;" />
                    </asp:TableCell>
                </asp:TableRow>

                <asp:TableRow>
                    <asp:TableCell>
                        <asp:Label ID="lbl_fname" runat="server" Text="First Name"></asp:Label>
                    </asp:TableCell>
                    <asp:TableCell>
                        <asp:TextBox ID="tb_fname" runat="server" onkeyup="javascript:CheckFName()"></asp:TextBox>
                        <br />
                        <asp:Label ID="err_fname" runat="server" Text="" ForeColor="Red"></asp:Label>
                    </asp:TableCell>
                </asp:TableRow>

                <asp:TableRow>
                    <asp:TableCell>
                        <asp:Label ID="lbl_lname" runat="server" Text="Last Name"></asp:Label>
                    </asp:TableCell>
                    <asp:TableCell>
                        <asp:TextBox ID="tb_lname" runat="server" onkeyup="javascript:CheckLName()"></asp:TextBox>
                        <br />
                        <asp:Label ID="err_lname" runat="server" Text="" ForeColor="Red"></asp:Label>
                    </asp:TableCell>
                </asp:TableRow>

                <asp:TableRow>
                    <asp:TableCell>
                        <asp:Label ID="lbl_email" runat="server" Text="Email"></asp:Label>
                    </asp:TableCell>
                    <asp:TableCell>
                        <asp:TextBox ID="tb_email" runat="server" onkeyup="javascript:CheckEmail()"></asp:TextBox>
                        <br />
                        <asp:Label ID="err_email" runat="server" Text="" ForeColor="Red"></asp:Label>
                    </asp:TableCell>
                </asp:TableRow>

                <asp:TableRow>
                    <asp:TableCell>
                        <asp:Label ID="lbl_dob" runat="server" Text="Date Of Birth"></asp:Label>
                    </asp:TableCell>
                    <asp:TableCell>
                        <asp:TextBox type="date" ID="tb_dob" runat="server"></asp:TextBox>
                        <br style="padding-bottom:20px;" />
                    </asp:TableCell>
                </asp:TableRow>

                <asp:TableRow>
                    <asp:TableCell>
                        <asp:Label ID="lbl_password" runat="server" Text="Password"></asp:Label>
                    </asp:TableCell>
                    <asp:TableCell>
                        <asp:TextBox type="password" ID="tb_password" runat="server" onkeyup="javascript:CheckPassword()"></asp:TextBox>
                        <br />
                        <asp:Label ID="lbl_passwordRequirement" runat="server" ForeColor="Red" Font-Size="10px"></asp:Label>
                        <br />
                        <asp:Label ID="err_password" runat="server" Text="" ForeColor="Red"></asp:Label>
                    </asp:TableCell>
                </asp:TableRow>

                <asp:TableRow>
                    <asp:TableCell Font-Bold="True">Credit Card Details</asp:TableCell>
                </asp:TableRow>

                <asp:TableRow>
                    <asp:TableCell>
                        <asp:Label ID="lbl_cardNo" runat="server" Text="Card Number"></asp:Label>
                    </asp:TableCell>
                    <asp:TableCell>
                        <asp:TextBox ID="tb_cardNo" runat="server"></asp:TextBox>
                        <br />
                        <asp:Label ID="err_cardNo" runat="server" Text="" ForeColor="Red"></asp:Label>
                    </asp:TableCell>
                </asp:TableRow>
                <asp:TableRow>
                    <asp:TableCell>
                        <asp:Label ID="lbl_expireDate" runat="server" Text="Expiration Date"></asp:Label>
                    </asp:TableCell>
                    <asp:TableCell>
                        <asp:TextBox type="date" ID="tb_expireDate" runat="server"></asp:TextBox>
                    </asp:TableCell>
                </asp:TableRow>
                <asp:TableRow>
                    <asp:TableCell>
                        <asp:Label ID="lbl_cvv" runat="server" Text="CVV"></asp:Label>
                    </asp:TableCell>
                    <asp:TableCell>
                        <asp:TextBox ID="tb_cvv" runat="server" onkeyup="javascript:CheckCVV()"></asp:TextBox>
                        <br />
                        <asp:Label ID="err_cvv" runat="server" Text="" ForeColor="Red"></asp:Label>
                    </asp:TableCell>
                </asp:TableRow>

                <asp:TableRow>

                </asp:TableRow>
            </asp:Table>

            <asp:Button ID="btn_submit" runat="server" Text="Register" OnClick="btn_submit_Click" />
        </div>
    </form>
    <script>
        function CheckPassword() {
            var str = document.getElementById("<%=tb_password.ClientID %>").value;
            var lbl_passwordRequirement = document.getElementById("lbl_passwordRequirement")
            lbl_passwordRequirement.innerHTML = "";
            var msg = "";

            if (str.length < 12) {
                msg += "Password length must be at least 12 characters";
            }
            if (str.search(/[0-9]/) == -1) {
                msg += ", require at least 1 number";
            }
            if (str.search(/[A-Z]/) == -1) {
                msg += ", require an uppercase letter";
            }
            if (str.search(/[a-z]/) == -1) {
                msg += ", require a lowercase letter";
            }
            if (str.search(/[^a-zA-Z0-9]/) == -1) {
                msg += ", require a special character";
            }

            lbl_passwordRequirement.innerHTML = msg;
        }

        function CheckFName() {
            var str = document.getElementById("<%=tb_fname.ClientID %>").value;
            var err_fname = document.getElementById("<%=err_fname.ClientID %>");
            err_fname.innerHTML = "";
            var msg = "";

            if (str.search(/[a-zA-Z0-9]{1,}$/) == -1) {
                msg = "First name must not contain special characters";
            }

            err_fname.innerHTML = msg;
        }

        function CheckLName() {
            var str = document.getElementById("<%=tb_lname.ClientID %>").value;
            var err_lname = document.getElementById("<%=err_lname.ClientID %>");
            err_lname.innerHTML = "";
            var msg = "";

            if (str.search(/[a-zA-Z0-9]{1,}$/) == -1) {
                msg = "Last name must not contain special characters";
            }

            err_lname.innerHTML = msg;
        }

        function CheckEmail() {
            var str = document.getElementById("<%=tb_email.ClientID %>").value;
            var err_email = document.getElementById("<%=err_email.ClientID %>");
            err_email.innerHTML = "";
            var msg = "";

            if (str.search(/^([\w\.\-]+)@([\w\-]+)((\.(\w){2,3})+)$/) == -1) {
                msg = "Email must be in the correct format";
            }

            err_email.innerHTML = msg;
        }

        function CheckCVV() {
            var str = document.getElementById("<%=tb_cvv.ClientID %>").value;
            var err_cvv = document.getElementById("<%=err_cvv.ClientID %>");
            err_cvv.innerHTML = "";
            var msg = "";

            if (str.search(/[0-9]{3,4}$/) == -1) {
                msg = "Credit card CVV must only contain numbers and have 3-4 digits only";
            }

            err_cvv.innerHTML = msg;
        }
    </script>
</body> 
</html>
