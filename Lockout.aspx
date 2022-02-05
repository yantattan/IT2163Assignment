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
                <h3>Your account has been temporarily locked for 2 mins due to multiple invalid login attempts. We have no 
                adminstrators to manage this website. Please wait till the lockout expires :)</h3>
            </fieldset>

            <asp:Button ID="btn_Unlock" runat="server" Text="Request for unlock" OnClick="UnlockAcc" />
        </div>
    </form>
</body>
</html>
