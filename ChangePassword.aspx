<%@ Page Language="C#" AutoEventWireup="true" CodeBehind="ChangePassword.aspx.cs" Inherits="Assignment.ChangePassword" ValidateRequest="true" Async="true"%>

<!DOCTYPE html>

<html xmlns="http://www.w3.org/1999/xhtml">
<head runat="server">
    <title></title>
</head>
<body>
    <h1>Change your password</h1>
    <form id="form1" runat="server">
        <div>
            <asp:Table runat="server">
                <asp:TableRow>
                    <asp:TableCell>
                        <asp:Label runat="server" Text="Old password"></asp:Label>
                    </asp:TableCell>
                    <asp:TableCell>
                        <asp:TextBox ID="tb_oldPassword" runat="server"></asp:TextBox>
                    </asp:TableCell>
                </asp:TableRow>
                <asp:TableRow>
                    <asp:TableCell>
                        <asp:Label runat="server" Text="New password"></asp:Label>
                    </asp:TableCell>
                    <asp:TableCell>
                        <asp:TextBox ID="tb_newPassword" runat="server"></asp:TextBox>
                    </asp:TableCell>
                </asp:TableRow>
            </asp:Table>
            <br />
            <asp:Label runat="server" ID="lbl_errorMsg" Text="" ForeColor="Red"></asp:Label>
            <br />

            <asp:Button runat="server" OnClick="ChangePwd" Text="Change my password" />
        </div>
    </form>
</body>
</html>
