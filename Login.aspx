<%@ Page Language="C#" AutoEventWireup="true" CodeBehind="Login.aspx.cs" Inherits="Assignment.Login1" ValidateRequest="false" Async="true"%>

<!DOCTYPE html>

<html xmlns="http://www.w3.org/1999/xhtml">
<head runat="server">
    <title>Login</title>
    <script src="https://www.google.com/recaptcha/api.js?render=6Lfa4WEeAAAAAM6kUhr7x4uUleuoGai5Ej2IpTf7"></script>
</head>
<body>
    <h1>Login</h1>
    <br />
    <form id="form1" runat="server">
        <div>
            <asp:Label runat="server" ID="err_msg" ForeColor="Red"></asp:Label>

            <asp:Table runat="server">
                <asp:TableRow>
                    <asp:TableCell>
                        <asp:Label runat="server" Text="Email"></asp:Label>
                    </asp:TableCell>
                    <asp:TableCell>
                        <asp:TextBox ID="tb_email" runat="server"></asp:TextBox>
                    </asp:TableCell>
                </asp:TableRow>

                <asp:TableRow>
                    <asp:TableCell>
                        <asp:Label runat="server" Text="Password"></asp:Label>
                    </asp:TableCell>
                    <asp:TableCell>
                        <asp:TextBox ID="tb_password" runat="server" type="password"></asp:TextBox>
                    </asp:TableCell>
                </asp:TableRow>
            </asp:Table>

            <input type="hidden" id="g-recaptcha-response" name="g-recaptcha-response"/>
            <asp:Label runat="server" ID="lbl_gScore" ForeColor="Red"></asp:Label>

            <asp:Button ID="submit_btn" runat="server" Text="Login" OnClick="Login" />
            <br />
            <asp:HyperLink ID="btnRegister" runat="server" NavigateUrl="Register.aspx">Sign up</asp:HyperLink>
        </div>
    </form>
</body>
<script>
    grecaptcha.ready(function () {
        grecaptcha.execute('6Lfa4WEeAAAAAM6kUhr7x4uUleuoGai5Ej2IpTf7', { action: 'Login' }).then(function (token) {
            document.getElementById("g-recaptcha-response").value = token;
        });
    });

</script>

</html>
