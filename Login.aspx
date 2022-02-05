<%@ Page Language="C#" AutoEventWireup="true" CodeBehind="Login.aspx.cs" Inherits="Assignment.Login1" ValidateRequest="false" %>

<!DOCTYPE html>

<html xmlns="http://www.w3.org/1999/xhtml">
<head runat="server">
    <title>Login</title>
    <script src="https://www.google.com/recaptcha/api.js?render=6Le-BjYeAAAAAOL7aKVMu48LKsoHdb_hcKH8fMTb"></script>
</head>
<body>
    <form id="form1" runat="server">
        <div>
            <asp:Label runat="server" ID="err_msg"></asp:Label>

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
                        <asp:TextBox ID="tb_password" runat="server"></asp:TextBox>
                    </asp:TableCell>
                </asp:TableRow>
            </asp:Table>

            <input type="hidden" id="g-recaptcha-response" name="g-recaptcha-response"/>
            <asp:Label runat="server" ID="lbl_gScore"></asp:Label>

            <asp:Button ID="submit_btn" runat="server" Text="Button" OnClick="Login" />
        </div>
    </form>
</body>
<script>
    grecaptcha.ready(function () {
        grecaptcha.execute('6Le-BjYeAAAAAOL7aKVMu48LKsoHdb_hcKH8fMTb', { action: 'Login' }).then(function (token) {
            document.getElementById("g-recaptcha-response").value = token;
        });
    });
</script>

</html>
