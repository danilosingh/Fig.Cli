<xsl:stylesheet version="1.0"
  xmlns:xsl="http://www.w3.org/1999/XSL/Transform"
  xmlns:msxsl="urn:schemas-microsoft-com:xslt"
  exclude-result-prefixes="msxsl"
  xmlns:wix="http://schemas.microsoft.com/wix/2006/wi"
  xmlns:my="my:my"
  xmlns:util="http://schemas.microsoft.com/wix/UtilExtension">

  <xsl:output method="xml" indent="yes" />

  <xsl:strip-space elements="*"/>

  <xsl:template match="@*|node()">
    <xsl:copy>
      <xsl:apply-templates select="@*|node()"/>
    </xsl:copy>
  </xsl:template>

  <!--<xsl:key name="cs-search" match="wix:Directory[@Name = 'cs']" use="@Id" />
  <xsl:template match="wix:Directory[@Name='cs']" />
  <xsl:template match="wix:Component[key('cs-search', @Directory)]" />
  <xsl:template match="wix:Fragment[wix:DirectoryRef[key('cs-search', @Id)]]" />
  <xsl:template match="wix:DirectoryRef[wix:DirectoryRef[key('cs-search', @Id)]]" />
  <xsl:template match="wix:ComponentRef[wix:DirectoryRef[key('cs-search', @Id)]]" />-->
  
  <!--<xsl:key name="de-search" match="wix:Directory[@Name = 'de']" use="@Id" />
  <xsl:template match="wix:Directory[@Name='de']" />
  <xsl:template match="wix:Component[key('de-search', @Directory)]" />

  <xsl:key name="es-search" match="wix:Directory[@Name = 'es']" use="@Id" />
  <xsl:template match="wix:Directory[@Name='es']" />
  <xsl:template match="wix:Component[key('es-search', @Directory)]" />

  <xsl:key name="fr-search" match="wix:Directory[@Name = 'fr']" use="@Id" />
  <xsl:template match="wix:Directory[@Name='fr']" />
  <xsl:template match="wix:Component[key('fr-search', @Directory)]" />

  <xsl:key name="it-search" match="wix:Directory[@Name = 'it']" use="@Id" />
  <xsl:template match="wix:Directory[@Name='it']" />
  <xsl:template match="wix:Component[key('it-search', @Directory)]" />

  <xsl:key name="ja-search" match="wix:Directory[@Name = 'ja']" use="@Id" />
  <xsl:template match="wix:Directory[@Name='ja']" />
  <xsl:template match="wix:Component[key('ja-search', @Directory)]" />

  <xsl:key name="ko-search" match="wix:Directory[@Name = 'ko']" use="@Id" />
  <xsl:template match="wix:Directory[@Name='ko']" />
  <xsl:template match="wix:Component[key('ko-search', @Directory)]" />

  <xsl:key name="pl-search" match="wix:Directory[@Name = 'pl']" use="@Id" />
  <xsl:template match="wix:Directory[@Name='pl']" />
  <xsl:template match="wix:Component[key('pl-search', @Directory)]" />

  <xsl:key name="ru-search" match="wix:Directory[@Name = 'ru']" use="@Id" />
  <xsl:template match="wix:Directory[@Name='ru']" />
  <xsl:template match="wix:Component[key('ru-search', @Directory)]" />

  <xsl:key name="tr-search" match="wix:Directory[@Name = 'tr']" use="@Id" />
  <xsl:template match="wix:Directory[@Name='tr']" />
  <xsl:template match="wix:Component[key('tr-search', @Directory)]" />-->

 
  
  <xsl:key name="logs-search" match="wix:Directory[@Name = 'logs']" use="@Id" />
  <xsl:template match="wix:Directory[@Name='logs']" />
  <xsl:template match="wix:Component[key('logs-search', @Directory)]" />
  
  
  <xsl:key name="svn-search" match="wix:Directory[@Name = 'data']" use="@Id" />
  <xsl:template match="wix:Directory[@Name='data']" />
  <xsl:template match="wix:Component[key('svn-search', @Directory)]" />

  <xsl:key name="svn-search" match="wix:Directory[@Name = 'data']" use="@Id" />
  <xsl:template match="wix:Directory[@Name='data']" />
  <xsl:template match="wix:Component[key('svn-search', @Directory)]" />
  
  <xsl:key name="service-search" match="wix:Component[contains(wix:File/@Source, '.vshost.exe')]" use="@Id" />
  <xsl:template match="wix:Component[key('service-search', @Id)]" />
  <xsl:template match="wix:ComponentRef[key('service-search', @Id)]" />

  <xsl:key name="pdb-search" match="wix:Component[contains(wix:File/@Source, '.pdb')]" use="@Id" />
  <xsl:template match="wix:Component[key('pdb-search', @Id)]" />
  <xsl:template match="wix:ComponentRef[key('pdb-search', @Id)]" />

  <xsl:key name="conecdb-search" match="wix:Component[contains(wix:File/@Source, '.conecdb')]" use="@Id" />
  <xsl:template match="wix:Component[key('conecdb-search', @Id)]" />
  <xsl:template match="wix:ComponentRef[key('conecdb-search', @Id)]" />

  <xsl:key name="xmldoc-search" match="wix:Component[contains(wix:File/@Source, '.xml')]" use="@Id" />
  <xsl:template match="wix:Component[key('xmldoc-search', @Id)]" />
  <xsl:template match="wix:ComponentRef[key('xmldoc-search', @Id)]" />

  <xsl:key name="sql-search" match="wix:Component[contains(wix:File/@Source, '.sql')]" use="@Id" />
  <xsl:template match="wix:Component[key('sql-search', @Id)]" />
  <xsl:template match="wix:ComponentRef[key('sql-search', @Id)]" />

  <xsl:key name="log-search" match="wix:Component[contains(wix:File/@Source, '.log')]" use="@Id" />
  <xsl:template match="wix:Component[key('log-search', @Id)]" />
  <xsl:template match="wix:ComponentRef[key('log-search', @Id)]" />
  

</xsl:stylesheet>