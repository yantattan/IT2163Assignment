<%@ Page Language="C#" AutoEventWireup="true" CodeBehind="2FAVerify.aspx.cs" Inherits="Assignment._2FAVerify" ValidateRequest="true" Async="true" %>

<!DOCTYPE html>

<html xmlns="http://www.w3.org/1999/xhtml">
<head runat="server">
    <title></title>
</head>
<body>
    <form id="form1" runat="server">
        <div>
            <h3>2FA Verification</h3>
            <br />
            <p>We have sent you a verification code through your email. Please key in the <b>6-digit</b> verification code</p>
            <br />
            <asp:Label ID="lbl_errMsg" runat="server" Text="" ForeColor="Red"></asp:Label>
            <br />
            <asp:TextBox ID="tb_verificationCode" runat="server"></asp:TextBox>
            <br />
            <asp:Button ID="btn_resendOTP" runat="server" Text="Resend OTP" OnClick="ResendOTP" />
            <asp:Button ID="btn_2faSubmit" runat="server" Text="Verify" OnClick="AddSession" />
        </div>
    </form>
</body>
</html>
