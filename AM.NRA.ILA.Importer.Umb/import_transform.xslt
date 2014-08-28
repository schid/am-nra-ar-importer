<?xml version="1.0" encoding="UTF-8"?>
<!DOCTYPE xsl:stylesheet [ <!ENTITY nbsp "&#x00A0;"> ]>
<xsl:stylesheet 
  version="1.0" 
  xmlns:xsl="http://www.w3.org/1999/XSL/Transform" 
  xmlns:msxml="urn:schemas-microsoft-com:xslt" 
  xmlns:Exslt.ExsltCommon="urn:Exslt.ExsltCommon" xmlns:Exslt.ExsltDatesAndTimes="urn:Exslt.ExsltDatesAndTimes" xmlns:Exslt.ExsltMath="urn:Exslt.ExsltMath" xmlns:Exslt.ExsltRegularExpressions="urn:Exslt.ExsltRegularExpressions" xmlns:Exslt.ExsltStrings="urn:Exslt.ExsltStrings" xmlns:Exslt.ExsltSets="urn:Exslt.ExsltSets" 
  exclude-result-prefixes="msxml Exslt.ExsltCommon Exslt.ExsltDatesAndTimes Exslt.ExsltMath Exslt.ExsltRegularExpressions Exslt.ExsltStrings Exslt.ExsltSets ">

  <xsl:output method="html" omit-xml-declaration="yes"/>

  <xsl:template match="/">
    <!--Total Count: <xsl:value-of select="count(//mediaCenterAudio[@isDoc])"/><br/>
      <xsl:apply-templates select="//mediaCenterAudio[@isDoc]"></xsl:apply-templates>-->
    <!--Total Count: <xsl:value-of select="count(//mediaCenterPDF[@isDoc])"/><br/>
    <xsl:apply-templates select="//mediaCenterPDF[@isDoc]"></xsl:apply-templates>-->
    <!--Total Count: <xsl:value-of select="count(//mediaCenterVideo[@isDoc])"/><br/>-->
    <!--<xsl:apply-templates select="//mediaCenterVideo[@isDoc]">
      <xsl:sort select="./publicationDate" order="descending"/>
    </xsl:apply-templates>-->
    
    Total Count: <xsl:value-of select="count(//*[@isDoc][bodyText != ''])"/>
  
    <!--<h2>Election Local Office</h2> //NO LOCAL ELECTIONS WITH VOTE X TEXT
    <xsl:apply-templates select="//electionLocalOffice2[@isDoc][bodyText != '']"></xsl:apply-templates>
    <br></br>-->

    <h2>Election State House Office</h2>
    <xsl:apply-templates select="//electionStateHouseOffice2[@isDoc][bodyText != '']"></xsl:apply-templates>
    <br></br>
    
    <h2>Election State Senate Office</h2>
    <xsl:apply-templates select="//electionStateSenateOffice2[@isDoc][bodyText != '']"></xsl:apply-templates>
    <br></br>
    
    <h2>Election State Office</h2>
    <xsl:apply-templates select="//electionStateOffice2[@isDoc][bodyText != '']"></xsl:apply-templates>
    <br></br>
    
    <!--<h2>Election Federal House Office</h2> //NOTHING WORTH UPDATING
    <xsl:apply-templates select="//electionUSHouseOffice2[@isDoc][bodyText != '']"></xsl:apply-templates>
    <br></br>-->
    
    <!--<h2>Election Federal Senate Office</h2> // NOTHING WORTH UPDATING
    <xsl:apply-templates select="//electionUSSenateOffice2[@isDoc][bodyText != '']"></xsl:apply-templates>
    <br></br>-->
  
   </xsl:template>

  
  <xsl:template match="electionLocalOffice2">
    <p>state = <xsl:value-of select="../../../@nodeName" /> | office = <xsl:value-of select="./pageTitle"/> | <b>description = <xsl:value-of select="./bodyText"/></b> | election date = <xsl:value-of select="./electionDate"/></p>
  </xsl:template>
  
  <xsl:template match="electionStateHouseOffice2">
    <p>state = <xsl:value-of select="../../@nodeName" /> | office = <xsl:value-of select="./pageTitle"/> | <b>description = <xsl:value-of select="./bodyText"/></b> | election date = <xsl:value-of select="./electionDate"/></p>
  </xsl:template>
  
  <xsl:template match="electionStateSenateOffice2">
    <p>state = <xsl:value-of select="../../@nodeName" /> | office = <xsl:value-of select="./pageTitle"/> | <b>description = <xsl:value-of select="./bodyText"/></b> | election date = <xsl:value-of select="./electionDate"/></p>
  </xsl:template>
  
  <xsl:template match="electionStateOffice2">
    <p>state = <xsl:value-of select="../../@nodeName" /> | office = <xsl:value-of select="./pageTitle"/> | <b>description = <xsl:value-of select="./bodyText"/></b> | election date = <xsl:value-of select="./electionDate"/></p>
  </xsl:template>
  
  <xsl:template match="electionUSHouseOffice2">
    <p>state = <xsl:value-of select="../../@nodeName" /> | office = <xsl:value-of select="./pageTitle"/> | <b>description = <xsl:value-of select="./bodyText"/></b> | election date = <xsl:value-of select="./electionDate"/></p>
  </xsl:template>
  
  <xsl:template match="electionUSSenateOffice2">
    <p>state = <xsl:value-of select="../../@nodeName" /> | office = <xsl:value-of select="./pageTitle"/> | <b>description = <xsl:value-of select="./bodyText"/></b> | election date = <xsl:value-of select="./electionDate"/></p>
  </xsl:template>
    
  
  
  
    
  
  
  <xsl:template match="mediaCenterVideo">
    ID = <xsl:value-of select="./@id"/> Publication Date = <xsl:value-of select="./publicationDate"/> Thumbnail = <xsl:value-of select="./mcThumbnail"/> Poster Image = <xsl:value-of select="./mcSplash"/> NRANewsThumbnail = <xsl:value-of select="./mcNRANewsThumb"/><br/>
  </xsl:template>
  
  <xsl:template match="mediaCenterAudio">
    <p>ID = <xsl:value-of select="./@id"/> Publication Date = <xsl:value-of select="./publicationDate"/></p>
  </xsl:template>

  <xsl:template match="audio">
    <p>thumb = <xsl:value-of select="./@thumbnail"/></p>
  </xsl:template>

  <xsl:template match="mediaCenterPDF">
    <p>ID = <xsl:value-of select="./@id"/> Publication Date = <xsl:value-of select="./publicationDate"/> File = <xsl:value-of select="./mcItemFile"/>
  </p>
  </xsl:template>

</xsl:stylesheet>