﻿#pragma warning disable 1591
//------------------------------------------------------------------------------
// <auto-generated>
//     This code was generated by a tool.
//     Runtime Version:4.0.30319.296
//
//     Changes to this file may cause incorrect behavior and will be lost if
//     the code is regenerated.
// </auto-generated>
//------------------------------------------------------------------------------

namespace DeviceHive.DocGenerator.Templates
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Text;
    
    #line 2 "..\..\Templates\PartialApi.cshtml"
    using DeviceHive.DocGenerator;
    
    #line default
    #line hidden
    
    #line 3 "..\..\Templates\PartialApi.cshtml"
    using DeviceHive.DocGenerator.Templates;
    
    #line default
    #line hidden
    
    [System.CodeDom.Compiler.GeneratedCodeAttribute("RazorGenerator", "1.5.0.0")]
    public partial class PartialApi : RazorGenerator.Templating.RazorTemplateBase
    {
#line hidden

        #line 4 "..\..\Templates\PartialApi.cshtml"

    public Metadata Metadata { get; set; }
    public Metadata WsMetadata { get; set; }

        #line default
        #line hidden

        public override void Execute()
        {


WriteLiteral("\r\n");




WriteLiteral("\r\n<div id=\"sidebar\">\r\n    <ul>\r\n        <li><a href=\"#GetStarted\">Get Started</a>" +
"</li>\r\n        <li><a href=\"#Reference\">REST API Reference</a>\r\n            <ul>" +
"\r\n");


            
            #line 13 "..\..\Templates\PartialApi.cshtml"
             foreach (var resource in Metadata.Resources)
            {

            
            #line default
            #line hidden
WriteLiteral("                <li><a href=\"#Reference/");


            
            #line 15 "..\..\Templates\PartialApi.cshtml"
                                   Write(Html.Encode(resource.Name));

            
            #line default
            #line hidden
WriteLiteral("\">");


            
            #line 15 "..\..\Templates\PartialApi.cshtml"
                                                                Write(Html.Encode(resource.Name));

            
            #line default
            #line hidden
WriteLiteral("</a>\r\n                    <ul id=\"nav-Reference-");


            
            #line 16 "..\..\Templates\PartialApi.cshtml"
                                     Write(Html.Encode(resource.Name));

            
            #line default
            #line hidden
WriteLiteral("\">\r\n");


            
            #line 17 "..\..\Templates\PartialApi.cshtml"
                     foreach (var method in resource.Methods)
                    {

            
            #line default
            #line hidden
WriteLiteral("                        <li><a href=\"#Reference/");


            
            #line 19 "..\..\Templates\PartialApi.cshtml"
                                           Write(Html.Encode(resource.Name));

            
            #line default
            #line hidden
WriteLiteral("/");


            
            #line 19 "..\..\Templates\PartialApi.cshtml"
                                                                       Write(Html.Encode(method.Name));

            
            #line default
            #line hidden
WriteLiteral("\">");


            
            #line 19 "..\..\Templates\PartialApi.cshtml"
                                                                                                  Write(Html.Encode(method.Name));

            
            #line default
            #line hidden
WriteLiteral("</a></li>\r\n");


            
            #line 20 "..\..\Templates\PartialApi.cshtml"
                    }

            
            #line default
            #line hidden
WriteLiteral("                    </ul>\r\n                </li>\r\n");


            
            #line 23 "..\..\Templates\PartialApi.cshtml"
            }

            
            #line default
            #line hidden
WriteLiteral("            </ul>\r\n        </li>\r\n        <li><a href=\"#WsReference\">WebSocket AP" +
"I Reference</a>\r\n            <ul>\r\n");


            
            #line 28 "..\..\Templates\PartialApi.cshtml"
             foreach (var service in WsMetadata.Services)
            {

            
            #line default
            #line hidden
WriteLiteral("                <li><a href=\"#WsReference/");


            
            #line 30 "..\..\Templates\PartialApi.cshtml"
                                     Write(Html.Encode(service.Name));

            
            #line default
            #line hidden
WriteLiteral("\">");


            
            #line 30 "..\..\Templates\PartialApi.cshtml"
                                                                 Write(Html.Encode(service.Name));

            
            #line default
            #line hidden
WriteLiteral("</a>\r\n                    <ul id=\"nav-WsReference-");


            
            #line 31 "..\..\Templates\PartialApi.cshtml"
                                       Write(Html.Encode(service.Name));

            
            #line default
            #line hidden
WriteLiteral("\">\r\n");


            
            #line 32 "..\..\Templates\PartialApi.cshtml"
                     foreach (var method in service.Methods)
                    {

            
            #line default
            #line hidden
WriteLiteral("                        <li><a href=\"#WsReference/");


            
            #line 34 "..\..\Templates\PartialApi.cshtml"
                                             Write(Html.Encode(service.Name));

            
            #line default
            #line hidden
WriteLiteral("/");


            
            #line 34 "..\..\Templates\PartialApi.cshtml"
                                                                        Write(Html.Encode(method.ID()));

            
            #line default
            #line hidden
WriteLiteral("\">");


            
            #line 34 "..\..\Templates\PartialApi.cshtml"
                                                                                                   Write(Html.Encode(method.Name));

            
            #line default
            #line hidden
WriteLiteral("</a></li>\r\n");


            
            #line 35 "..\..\Templates\PartialApi.cshtml"
                    }

            
            #line default
            #line hidden
WriteLiteral("                    </ul>\r\n                </li>\r\n");


            
            #line 38 "..\..\Templates\PartialApi.cshtml"
            }

            
            #line default
            #line hidden
WriteLiteral("            </ul>\r\n        </li>\r\n    </ul>\r\n</div>\r\n<div id=\"content\" class=\"col" +
"umn\" role=\"main\">\r\n    <div id=\"GetStarted\">\r\n        ");


            
            #line 45 "..\..\Templates\PartialApi.cshtml"
    Write(new PartialGetStarted().TransformText());

            
            #line default
            #line hidden
WriteLiteral("\r\n    </div>\r\n    <div id=\"Reference\">\r\n        <h1>REST API Reference</h1>\r\n    " +
"    <p>The DeviceHive REST API exposes the following resources:</p>\r\n");


            
            #line 50 "..\..\Templates\PartialApi.cshtml"
         foreach (var resource in Metadata.Resources)
        {

            
            #line default
            #line hidden
WriteLiteral("            <h2>");


            
            #line 52 "..\..\Templates\PartialApi.cshtml"
           Write(Html.Encode(resource.Name));

            
            #line default
            #line hidden
WriteLiteral("</h2>\r\n");



WriteLiteral("            <p>");


            
            #line 53 "..\..\Templates\PartialApi.cshtml"
          Write(Html.Documentation(resource.Documentation));

            
            #line default
            #line hidden
WriteLiteral("</p>\r\n");



WriteLiteral("            <p>For ");


            
            #line 54 "..\..\Templates\PartialApi.cshtml"
              Write(Html.Encode(resource.Name));

            
            #line default
            #line hidden
WriteLiteral(" resource details, see the <a href=\"#Reference/");


            
            #line 54 "..\..\Templates\PartialApi.cshtml"
                                                                                        Write(Html.Encode(resource.Name));

            
            #line default
            #line hidden
WriteLiteral("\">resource representation</a> page.</p>\r\n");



WriteLiteral(@"            <table>
                <tr>
                    <th style=""width:120px"">Method</th>
                    <th style=""width:150px"">Authorization</th>
                    <th style=""width:300px"">Uri</th>
                    <th style=""width:400px"">Description</th>
                </tr>
");


            
            #line 62 "..\..\Templates\PartialApi.cshtml"
                 foreach (var method in resource.Methods)
                {

            
            #line default
            #line hidden
WriteLiteral("                <tr>\r\n                    <td><a href=\"#Reference/");


            
            #line 65 "..\..\Templates\PartialApi.cshtml"
                                       Write(Html.Encode(resource.Name));

            
            #line default
            #line hidden
WriteLiteral("/");


            
            #line 65 "..\..\Templates\PartialApi.cshtml"
                                                                   Write(Html.Encode(method.Name));

            
            #line default
            #line hidden
WriteLiteral("\">");


            
            #line 65 "..\..\Templates\PartialApi.cshtml"
                                                                                              Write(Html.Encode(method.Name));

            
            #line default
            #line hidden
WriteLiteral("</a></td>\r\n                    <td>");


            
            #line 66 "..\..\Templates\PartialApi.cshtml"
                   Write(Html.Encode(method.Authorization));

            
            #line default
            #line hidden
WriteLiteral("</td>\r\n                    <td>");


            
            #line 67 "..\..\Templates\PartialApi.cshtml"
                   Write(Html.Encode(method.Verb));

            
            #line default
            #line hidden
WriteLiteral(" ");


            
            #line 67 "..\..\Templates\PartialApi.cshtml"
                                             Write(Html.Encode(method.UriNoQuery()));

            
            #line default
            #line hidden
WriteLiteral("</td>\r\n                    <td>");


            
            #line 68 "..\..\Templates\PartialApi.cshtml"
                   Write(Html.Documentation(method.Documentation));

            
            #line default
            #line hidden
WriteLiteral("</td>\r\n                </tr>\r\n");


            
            #line 70 "..\..\Templates\PartialApi.cshtml"
                }

            
            #line default
            #line hidden
WriteLiteral("            </table>\r\n");


            
            #line 72 "..\..\Templates\PartialApi.cshtml"
        }

            
            #line default
            #line hidden
WriteLiteral("    </div>\r\n    <div id=\"WsReference\">\r\n        <h1>WebSocket API Reference</h1>\r" +
"\n        <p>The DeviceHive WebSocket API exposes the following services:</p>\r\n");


            
            #line 77 "..\..\Templates\PartialApi.cshtml"
         foreach (var service in WsMetadata.Services)
        {

            
            #line default
            #line hidden
WriteLiteral("            <h2>");


            
            #line 79 "..\..\Templates\PartialApi.cshtml"
           Write(Html.Encode(service.Name));

            
            #line default
            #line hidden
WriteLiteral("</h2>\r\n");



WriteLiteral("            <p>");


            
            #line 80 "..\..\Templates\PartialApi.cshtml"
          Write(Html.Documentation(service.Documentation));

            
            #line default
            #line hidden
WriteLiteral("</p>\r\n");



WriteLiteral("            <p>For the ");


            
            #line 81 "..\..\Templates\PartialApi.cshtml"
                  Write(Html.Encode(service.Name));

            
            #line default
            #line hidden
WriteLiteral(" service details, see the <a href=\"#WsReference/");


            
            #line 81 "..\..\Templates\PartialApi.cshtml"
                                                                                            Write(Html.Encode(service.Name));

            
            #line default
            #line hidden
WriteLiteral("\">service information</a> page.</p>\r\n");



WriteLiteral(@"            <table>
                <tr>
                    <th style=""width:150px"">Message</th>
                    <th style=""width:80px"">Originator</th>
                    <th style=""width:100px"">Authorization</th>
                    <th style=""width:400px"">Description</th>
                </tr>
");


            
            #line 89 "..\..\Templates\PartialApi.cshtml"
                 foreach (var method in service.Methods)
                {

            
            #line default
            #line hidden
WriteLiteral("                <tr>\r\n                    <td><a href=\"#WsReference/");


            
            #line 92 "..\..\Templates\PartialApi.cshtml"
                                         Write(Html.Encode(service.Name));

            
            #line default
            #line hidden
WriteLiteral("/");


            
            #line 92 "..\..\Templates\PartialApi.cshtml"
                                                                    Write(Html.Encode(method.ID()));

            
            #line default
            #line hidden
WriteLiteral("\">");


            
            #line 92 "..\..\Templates\PartialApi.cshtml"
                                                                                               Write(Html.Encode(method.Name));

            
            #line default
            #line hidden
WriteLiteral("</a></td>\r\n                    <td>");


            
            #line 93 "..\..\Templates\PartialApi.cshtml"
                   Write(Html.Encode(method.Originator));

            
            #line default
            #line hidden
WriteLiteral("</td>\r\n                    <td>");


            
            #line 94 "..\..\Templates\PartialApi.cshtml"
                   Write(Html.Encode(method.Authorization));

            
            #line default
            #line hidden
WriteLiteral("</td>\r\n                    <td>");


            
            #line 95 "..\..\Templates\PartialApi.cshtml"
                   Write(Html.Documentation(method.Documentation));

            
            #line default
            #line hidden
WriteLiteral("</td>\r\n                </tr>\r\n");


            
            #line 97 "..\..\Templates\PartialApi.cshtml"
                }

            
            #line default
            #line hidden
WriteLiteral("            </table>\r\n");


            
            #line 99 "..\..\Templates\PartialApi.cshtml"
        }

            
            #line default
            #line hidden
WriteLiteral("    </div>\r\n");


            
            #line 101 "..\..\Templates\PartialApi.cshtml"
     foreach (var resource in Metadata.Resources)
    {

            
            #line default
            #line hidden
WriteLiteral("    <div id=\"Reference-");


            
            #line 103 "..\..\Templates\PartialApi.cshtml"
                  Write(Html.Encode(resource.Name));

            
            #line default
            #line hidden
WriteLiteral("\">\r\n        ");


            
            #line 104 "..\..\Templates\PartialApi.cshtml"
    Write(new PartialResource { Resource = resource }.TransformText());

            
            #line default
            #line hidden
WriteLiteral("\r\n    </div>\r\n");


            
            #line 106 "..\..\Templates\PartialApi.cshtml"
        foreach (var method in resource.Methods)
        {

            
            #line default
            #line hidden
WriteLiteral("    <div id=\"Reference-");


            
            #line 108 "..\..\Templates\PartialApi.cshtml"
                  Write(Html.Encode(resource.Name));

            
            #line default
            #line hidden
WriteLiteral("-");


            
            #line 108 "..\..\Templates\PartialApi.cshtml"
                                              Write(Html.Encode(method.Name));

            
            #line default
            #line hidden
WriteLiteral("\">\r\n        ");


            
            #line 109 "..\..\Templates\PartialApi.cshtml"
    Write(new PartialMethod { Resource = resource, Method = method }.TransformText());

            
            #line default
            #line hidden
WriteLiteral("\r\n    </div>\r\n");


            
            #line 111 "..\..\Templates\PartialApi.cshtml"
        }
    }

            
            #line default
            #line hidden

            
            #line 113 "..\..\Templates\PartialApi.cshtml"
     foreach (var service in WsMetadata.Services)
    {

            
            #line default
            #line hidden
WriteLiteral("    <div id=\"WsReference-");


            
            #line 115 "..\..\Templates\PartialApi.cshtml"
                    Write(Html.Encode(service.Name));

            
            #line default
            #line hidden
WriteLiteral("\">\r\n        ");


            
            #line 116 "..\..\Templates\PartialApi.cshtml"
    Write(new PartialWsService { Service = service }.TransformText());

            
            #line default
            #line hidden
WriteLiteral("\r\n    </div>\r\n");


            
            #line 118 "..\..\Templates\PartialApi.cshtml"
        foreach (var method in service.Methods)
        {

            
            #line default
            #line hidden
WriteLiteral("    <div id=\"WsReference-");


            
            #line 120 "..\..\Templates\PartialApi.cshtml"
                    Write(Html.Encode(service.Name));

            
            #line default
            #line hidden
WriteLiteral("-");


            
            #line 120 "..\..\Templates\PartialApi.cshtml"
                                               Write(Html.Encode(method.ID()));

            
            #line default
            #line hidden
WriteLiteral("\">\r\n        ");


            
            #line 121 "..\..\Templates\PartialApi.cshtml"
    Write(new PartialWsMethod { Service = service, Method = method }.TransformText());

            
            #line default
            #line hidden
WriteLiteral("\r\n    </div>\r\n");


            
            #line 123 "..\..\Templates\PartialApi.cshtml"
    }}

            
            #line default
            #line hidden
WriteLiteral(@"</div>
<script type=""text/javascript"">
    jQuery(function () {
        if (""onhashchange"" in window) {
            window.onhashchange = function () {
                navigator.open(window.location.hash);
            }
        }
        else {
            var storedHash = window.location.hash;
            window.setInterval(function () {
                if (window.location.hash != storedHash) {
                    storedHash = window.location.hash;
                    navigator.open(storedHash);
                }
            }, 100);
        }

        var navigator = {
            open: function (hash) {
                hash = hash.replace(/^\#/, """"); if (hash == """") hash = ""GetStarted"";
                jQuery(""#content > div"").hide().filter(""#"" + hash.replace(/\//g, ""-"")).show();
                jQuery(""#sidebar ul[id^='nav-']"").hide();
                var nav = """"; jQuery.each(hash.split(""/""), function (index, value) { nav += ""-"" + value; jQuery(""#nav"" + nav).show(); });
                window.scrollTo(0, 0);
            }
        }

        navigator.open(window.location.hash);
    });
</script>

");


        }
    }
}
#pragma warning restore 1591
