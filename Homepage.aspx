<%@ Page Language="C#" AutoEventWireup="true" CodeBehind="Homepage.aspx.cs" Inherits="Assignment.Homepage" %>

<!DOCTYPE html>

<html xmlns="http://www.w3.org/1999/xhtml">
<head runat="server">
    <title></title>
</head>
<body>
    <form id="form1" runat="server">
        <div>
            <fieldset> 
                <h1>Welcome user!</h1>
                <br /> 
                <h3>What would you like to do today?</h3>
                <p>Oh wait you can only logout</p>
                <br />
                <br /> 

                <asp:Button ID="btnLogout" runat="server" Text="Logout" OnClick="Logout" Visible="false" /> 

                <p/> 

            </fieldset>
        </div>
    </form>
</body>
</html>
