using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Linq;

namespace EAAddInBase.MDGBuilder
{
   public sealed class ShapeScript
    {
       public ShapeScript(String content)
       {
           Content = content;
       }

       public string Content { get; private set; }

       internal XElement ToXml()
       {
           var dt = XNamespace.Get("urn:schemas-microsoft-com:datatypes");
           return new XElement("Image",
               new XAttribute("type", "EAShapeScript 1.0"),
               new XAttribute(XNamespace.Xmlns + "dt", "urn:schemas-microsoft-com:datatypes"),
               new XAttribute(dt + "dt", "bin.base64"),
               ToBase64EncodedZip(Content));
       }

       private static string ToBase64EncodedZip(String s){
           /* Shape scripts are represented in a base64 encoded zip file containing one
            * file called "str.dat". This file contains the shape script in UTF-16.
            */
           using (var zipStream = new MemoryStream())
           {
               using (var zipArchive = new ZipArchive(zipStream, ZipArchiveMode.Update))
               {
                   var scriptEntry = zipArchive.CreateEntry("str.dat", CompressionLevel.NoCompression);
                   // encoding 1200: UTF-16
                   using (var writer = new StreamWriter(scriptEntry.Open(), Encoding.GetEncoding(1200)))
                   {
                       writer.Write(s);
                   }
               }

               byte[] result = zipStream.ToArray();
               return Convert.ToBase64String(result);
           }
       }
    }
}
