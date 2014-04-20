<xsl:stylesheet xmlns:xsl="http://www.w3.org/1999/XSL/Transform" version="1.0">
  <xsl:template match="bookstore">
    <html>
      <body>
        <table BORDER="2">
          <tr>
            <td>ISBN</td>
            <td>Title</td>
            <td>Price</td>
          </tr>
          <xsl:apply-templates select="book"/>
        </table>
      </body>
    </html>
  </xsl:template>
  <xsl:template match="book">
    <tr>
      <td>
        <xsl:value-of select="@ISBN"/>
      </td>
      <td>
        <xsl:value-of select="title"/>
      </td>
      <td>
        <xsl:value-of select="price"/>
      </td>
    </tr>
  </xsl:template>
</xsl:stylesheet>