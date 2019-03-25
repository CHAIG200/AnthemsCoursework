<%@ Page Language="C#" AutoEventWireup="true" CodeBehind="index.aspx.cs" Inherits="AnthemsCoursework._Default" %>

<!DOCTYPE html PUBLIC "-//W3C//DTD XHTML 1.0 Transitional//EN" "http://www.w3.org/TR/xhtml1/DTD/xhtml1-transitional.dtd">

<html xmlns="http://www.w3.org/1999/xhtml">
<head id="Head1" runat="server">
    <title>Anthem previews</title>
</head>
<body>
    <form id="form1" runat="server">
        <asp:ScriptManager ID="sm1" runat="server" />
        <div>
            Upload MP3:
            <asp:FileUpload ID="upload" runat="server" accept=".mp3"/>
            <asp:Button ID="submitButton" runat="server" Text="Submit" OnClick="submitButton_Click" />
        </div>
        <div>
            <asp:UpdatePanel ID="up1" runat="server">
                <ContentTemplate>
                    <asp:ListView ID="AnthemDisplayControl" runat="server">
                        <ItemTemplate>
                           <audio src='<%# Eval("Url") %>' controls="" preload="none"></audio>
                            <asp:Literal ID="label" Text='<%# Eval("Title") %>' runat="server"/>
                            <asp:Label ID="Label1" runat="server" Text="<br />"> </asp:Label>
                        </ItemTemplate>
                    </asp:ListView>
                    <asp:button ID="refresh" runat="server" Text="Refresh Page" OnClick="refresh_Click" />
                </ContentTemplate>
            </asp:UpdatePanel>
        </div>
    </form>
</body>
</html>
