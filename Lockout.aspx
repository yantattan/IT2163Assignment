<%@ Page Language="C#" AutoEventWireup="true" CodeBehind="Lockout.aspx.cs" Inherits="Assignment.Lockout" %>

<!DOCTYPE html>

<html xmlns="http://www.w3.org/1999/xhtml">
<head runat="server">
    <title>Account Locked</title>
</head>
<body>
    <form id="form1" runat="server">
        <div>
            <fieldset>
                <legend>:(</legend>
                <h3>Your account has been temporarily locked for 1 mins due to multiple invalid login attempts. We have no 
                adminstrators to manage this website. Please wait till the lockout expires :)</h3>
                <asp:Label ID="lbl_errorMsg" runat="server" ForeColor="Red"></asp:Label>
            </fieldset>
            <asp:TextBox ID="lockout_time" runat="server" Enabled="false" style="display:none;"></asp:TextBox>
        </div>
    </form>
</body>
<script>
    var lockoutTime = parseInt(document.getElementById("lockout_time").value);
    if (lockoutTime) {
        setTimeout(function () {
            window.location.replace("https://localhost:44308/Login.aspx");
        }, lockoutTime)
    }
</script>
</html>
