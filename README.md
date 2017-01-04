# EmbeddedPageHandlerFactory - Binary-Only ASP.NET 1.1

ASP.NET 1.1 `HttpModule` that allows you to serve your ASPX markup from embedded resources. [Originally released on my blog.](http://www.paraesthesia.com/archive/2007/05/31/embeddedpagehandlerfactory-binary-only-asp.net-1.1.aspx/)

You set your application `web.config` file to use the EmbeddedPageHandlerFactory and in your ASP.NET project set your ASPX files from "Content" to "Embedded Resource." At application startup, the module will go through the assemblies you registered as containing pages and extracts the ASPX to a temporary location. A replacement for the standard `PageHandlerFactory` redirects requests to the temporary location so pages get served up just like usual. When the application shuts down, the temporary files get cleaned up.

Add the module and handler configuration to your `web.config` file like this:

```xml
<?xml version="1.0" encoding="utf-8" ?>
<configuration>
  <configSections>
    <section
      name="embeddedPageAssemblies"
      type="System.Configuration.DictionarySectionHandler, System, Version=1.0.5000.0, Culture=neutral, PublicKeyToken=b77a5c561934e089, Custom=null"
      />
  </configSections>
  <embeddedPageAssemblies>
    <add
      key="Paraesthesia.EmbeddedPageHandlerFactory.Demo"
      value="Paraesthesia.EmbeddedPageHandlerFactory.Demo" />
  </embeddedPageAssemblies>
  <system.web>
    <httpModules>
      <add
        name="EmbeddedPageHandlerFactory"
        type="Paraesthesia.Web.UI.EmbeddedPageHandlerFactory, Paraesthesia.EmbeddedPageHandlerFactory" />
    </httpModules>
    <httpHandlers>
      <add
        verb="*"
        path="*.aspx"
        type="Paraesthesia.Web.UI.EmbeddedPageHandlerFactory, Paraesthesia.EmbeddedPageHandlerFactory" />
    </httpHandlers>
  </system.web>
  </appSettings>
    <add
      key="Paraesthesia.Web.UI.EmbeddedPageHandlerFactory.AllowFileSystemPages"
      value="false" />
  <appSettings>
</configuration>
```