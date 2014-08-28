<%@ Control Language="C#" AutoEventWireup="true" CodeBehind="Importer.ascx.cs" Inherits="AM.NRA.ILA.Importer.Umb.usercontrols.Importer" %>
<style type="text/css">
    @import url(http://code.jquery.com/ui/1.10.3/themes/smoothness/jquery-ui.css);
    input[type=text].wide {display:block; width:250px;}
    label {display:block; margin-bottom:0.5em;}
    input[type=button] {margin-top: 2em;}
    .hide {display:none;}
    .show {display:block;}
    .error {color:Red;}
</style>
<script type="text/javascript" src="http://code.jquery.com/ui/1.10.3/jquery-ui.js"></script>
<script type="text/javascript">
    $(function () {
        $("#txtStartDate").datepicker({
            changeMonth:true,
            changeYear:true,
            dateFormat:'yy-mm-dd'
        });
        $("#txtEndDate").datepicker({
            changeMonth:true,
            changeYear:true,
            dateFormat:'yy-mm-dd'
        });
    });

    $(function () {
        $("#buttonStartImport").click(function (event) {
            event.preventDefault();
            var problems = 0;

            var fileName = $("#txtSourceFileName").val();
            if (fileName == "" || typeof fileName == "undefined") {
                problems++;
            }

            var contentType = $("input[name=radioButtonList]:checked").val();
            if (contentType == "" || typeof contentType == "undefined") {
                problems++;
            }

            var startDate = $("#txtStartDate").val();
            if (startDate == "" || typeof startDate == "undefined") {
                problems++;
            }

            var endDate = $("#txtEndDate").val();
            if (endDate == "" || typeof endDate == "undefined") {
                problems++;
            }

            if (problems > 0) {
                if ($("#validationSummary").not(":visible")) {
                    $("#validationSummary").removeClass("hide");
                    $("#validationSummary").addClass("show");
                }
            } else {
                $("#validationSummary").removeClass("show");
                $("#validationSummary").addClass("hide");
                //testAjaxCall();
                startImport($("#txtSourceFileName").val(),
                    $("input[name=radioButtonList]:checked").val(),
                    $("#txtStartDate").val(),
                    $("#txtEndDate").val());
            }
        });

        function testAjaxCall() {
            var aRequest = $.ajax({
                url: "/base/Test/Hello.aspx",
                cache: false,
                dataType: "xml"
            });

            aRequest.done(function (msg) {
                var message = $(msg).find("value").text();
                alert("Success: " + message);
            });

            aRequest.fail(function (jqXHR, textStatus) {
                alert("Request failed: " + textStatus);
            });
        }

        function startImport(fileName, importType, startDate, endDate) {
            var requestUrl = "/base/Test/StartImport/";
            requestUrl += fileName;
            requestUrl += "/";
            requestUrl += importType;
            requestUrl += "/";
            requestUrl += startDate;
            requestUrl += "/";
            requestUrl += endDate;
            requestUrl += ".aspx";

            var aRequest = $.ajax({
                url: requestUrl,
                cache: false,
                dataType: "xml"
            });

            aRequest.done(function (msg) {
                var message = $(msg).find("value").text();
                alert(message);
            });

            aRequest.fail(function (jqXHR, textStatus) {
                alert("Request failed: " + textStatus);
            });
        }
    });
</script>
<div class="propertypane">
    <div>
        <div class="propertyitem">
            <div class="dashboardWrapper">
                <h2>ILA Content Importer</h2>
                <h3>Step One</h3>
                <p>Enter the name of your source XML file. Note: this file must be saved in the website's app_data directory.</p>
                <input type="text" id="txtSourceFileName" class="wide" />
                <h3>Step Two</h3>
                <p>Select the type of content you want to import.</p>
                <label for="radioArticles"><input type="radio" id="radioArticles" name="radioButtonList" value="article" /> Articles</label>
                <label for="radioVideo"><input type="radio" id="radioVideo" name="radioButtonList" value="video" /> Video Items</label>
                <label for="radioAudio"><input type="radio" id="radioAudio" name="radioButtonList" value="audio" /> Audio Items</label>
                <label for="radioPDF"><input type="radio" id="radioPDF" name="radioButtonList" value="pdf" /> PDF Items</label>
                <h3>Step Three</h3>
                <p>Select the publication date range.</p>
                <label for="txtStartDate">Start Date: <input type="text" id="txtStartDate" /></label>
                <label for="txtEndDate">End Date: <input type="text" id="txtEndDate" /></label>
                <div id="validationSummary" class="error hide"><p>All fields are required.</p></div>
                <input type="button" id="buttonStartImport" value="Start Import" />
            </div>
        </div>
    </div>
</div>