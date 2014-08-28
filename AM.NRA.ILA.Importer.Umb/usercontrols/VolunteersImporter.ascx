<%@ Control Language="C#" AutoEventWireup="true" CodeBehind="VolunteersImporter.ascx.cs" Inherits="AM.NRA.ILA.Importer.Umb.usercontrols.VolunteersImporter" %>
<h1>Import the Volunteers</h1>
<asp:Button runat="server" ID="SubmitButton" CausesValidation="false" Text="Engage" OnClick="SubmitButton_Clicked" ViewStateMode="Disabled" />
<asp:Literal runat="server" ID="ResultsMessage"></asp:Literal>