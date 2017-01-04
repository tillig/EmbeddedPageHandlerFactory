<%@ Page language="c#" Codebehind="NotEmbedded.aspx.cs" AutoEventWireup="false" Inherits="Paraesthesia.EmbeddedPageHandlerFactory.Demo.NotEmbedded" %>
<!DOCTYPE HTML PUBLIC "-//W3C//DTD HTML 4.0 Transitional//EN" > 

<html>
	<head>
		<title>Not Embedded</title>
	</head>
	<body>

		<form id="Form1" method="post" runat="server">
			<h1>Not Embedded</h1>
			<p>This web form is not embedded in the assembly.</p>
			<p>Current date/time is: <asp:Literal id="litDateTime" runat="server" /></p>
		</form>

	</body>
</html>
