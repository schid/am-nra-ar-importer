<%@ Control Language="C#" AutoEventWireup="true" CodeBehind="FixTags.ascx.cs" Inherits="AM.NRA.ILA.Importer.Umb.usercontrols.FixTags" %>
<h1>Fix Tags</h1>
<asp:TextBox runat="server" ID="startNode"></asp:TextBox>
<asp:Button runat="server" ID="SubmitButton" CausesValidation="false" Text="Start" OnClick="SubmitButton_Click" />
<asp:Literal runat="server" ID="literalMessage"></asp:Literal>