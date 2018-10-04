using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Xml;
using Newtonsoft.Json;

namespace VSExtensionManager.Models
{

    public class Gallery
    {
        private const string BASE_URL = "https://localhost:5001";

        [Key]
        public long Id { get; set; }
        public string DisplayName { get; set; }

        [Newtonsoft.Json.JsonProperty(NullValueHandling = NullValueHandling.Ignore)]
        public List<Package> Packages {get;set;}


        public string GetFeed()
        {
            var sb = new StringBuilder();
            var settings = new XmlWriterSettings
            {
                Indent = true
            };

            using (var writer = XmlWriter.Create(sb, settings))
            {
                writer.WriteStartElement("feed", "http://www.w3.org/2005/Atom");

                writer.WriteElementString("title", DisplayName);
                writer.WriteElementString("id", Id.ToString());
                writer.WriteElementString("updated", DateTime.Now.ToString("yyyy-MM-ddTHH:mm:ssZ"));
                writer.WriteElementString("subtitle", "");

                writer.WriteStartElement("link");
                writer.WriteAttributeString("rel", "alternate");
                writer.WriteAttributeString("href", $"https://localhost:5001/api/gallery/{Id}/feed");
                writer.WriteEndElement(); // link

                foreach (Package package in Packages)
                {
                    AddEntry(writer, package);
                }

                writer.WriteEndElement(); // feed
            }

            return sb.ToString().Replace("utf-16", "utf-8");
        }

        private void AddEntry(XmlWriter writer, Package package)
        {
            writer.WriteStartElement("entry");

            writer.WriteElementString("id", package.PackageIdentifier);

            writer.WriteStartElement("title");
            writer.WriteAttributeString("type", "text");
            writer.WriteValue(package.Name);
            writer.WriteEndElement(); // title

            writer.WriteStartElement("link");
            writer.WriteAttributeString("rel", "alternate");
            writer.WriteAttributeString("href", $"{BASE_URL}/api/gallery/{Id}/packages/{package.Id}/content");
            writer.WriteEndElement(); // link

            writer.WriteStartElement("summary");
            writer.WriteAttributeString("type", "text");
            writer.WriteValue(package.Description);
            writer.WriteEndElement(); // summary

            writer.WriteElementString("published", package.DatePublished.ToString("yyyy-MM-ddTHH:mm:ssZ"));
            writer.WriteElementString("updated", package.DatePublished.ToString("yyyy-MM-ddTHH:mm:ssZ"));

            writer.WriteStartElement("author");
            writer.WriteElementString("name", package.Author);
            writer.WriteEndElement(); // author

            writer.WriteStartElement("content");
            writer.WriteAttributeString("type", "application/octet-stream");
            writer.WriteAttributeString("src", $"{BASE_URL}/api/gallery/{Id}/packages/{package.Id}/content");
            writer.WriteEndElement(); // content

            if (package.Icon != null)
            {
                writer.WriteStartElement("link");
                writer.WriteAttributeString("rel", "icon");
                writer.WriteAttributeString("href", $"{BASE_URL}/api/gallery/{Id}/packages/{package.Id}/icon");
                writer.WriteEndElement(); // icon
            }

            writer.WriteRaw("\r\n<Vsix xmlns:xsi=\"http://www.w3.org/2001/XMLSchema-instance\" xmlns:xsd=\"http://www.w3.org/2001/XMLSchema\" xmlns=\"http://schemas.microsoft.com/developer/vsx-syndication-schema/2010\">\r\n");

            writer.WriteElementString("Id", package.PackageIdentifier);
            writer.WriteElementString("Version", package.Version);

            writer.WriteStartElement("References");
            writer.WriteEndElement();

            writer.WriteRaw("\r\n<Rating xsi:nil=\"true\" />");
            writer.WriteRaw("\r\n<RatingCount xsi:nil=\"true\" />");
            writer.WriteRaw("\r\n<DownloadCount xsi:nil=\"true\" />\r\n");

            if (package.ExtensionList?.Extensions != null)
            {
                string ids = string.Join(";", package.ExtensionList.Extensions.Select(e => e.VsixId));
                writer.WriteElementString("PackedExtensionIDs", ids);
            }

            writer.WriteRaw("</Vsix>");// Vsix
            writer.WriteEndElement(); // entry
        }
    }
}