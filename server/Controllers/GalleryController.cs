using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Net.Http;
using System.Runtime.Serialization.Json;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Xml;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using VSExtensionManager.Models;
using Microsoft.EntityFrameworkCore;
using VSExtensionManager;

namespace server.Controllers
{
    [Route("api/gallery")]
    [ApiController]
    public class GalleryController : ControllerBase
    {
        [HttpPost()]
        public IActionResult CreateGallery([FromBody] Gallery gallery)
        {
            using (var db = new VSExtensionManagerContext())
            {
                if (gallery.Id == 0)
                {
                    db.Galleries.Add(gallery);
                    db.SaveChanges();
                    return Ok(gallery);
                }
                else
                {
                    return UpdateGallery(gallery.Id, gallery);
                }
            }
        }

        [HttpPut("{galleryId}")]
        public IActionResult UpdateGallery([FromRoute] long galleryId, [FromBody] Gallery gallery)
        {
            using (var db = new VSExtensionManagerContext())
            {
                var existing = db.Galleries.Find(galleryId);

                if (existing == null) return NotFound(new
                {
                    Error = $"Gallery {galleryId} not found."
                });

                existing.DisplayName = gallery.DisplayName;

                db.SaveChanges();

                return Ok(gallery);
            }
        }

        [HttpGet("{galleryId}")]
        public IActionResult GetGallery([FromRoute] long galleryId)
        {
            using (var db = new VSExtensionManagerContext())
            {
                var gallery = db.Galleries.SingleOrDefault(g => g.Id == galleryId);
                if (gallery == null)
                {
                    return NotFound(new { Error = $"Gallery '{galleryId}' not found." });
                }
                else
                {
                    return Ok(gallery);
                }
            }
        }

        [HttpGet("{galleryId}/feed")]
        public IActionResult GetFeed([FromRoute] long galleryId)
        {
            using (var db = new VSExtensionManagerContext())
            {
                var gallery = db.Galleries.Include("Packages").SingleOrDefault(g => g.Id == galleryId);
                if (gallery == null)
                {
                    return NotFound(new { Error = $"Gallery '{galleryId}' not found." });
                }
                else
                {
                    return Content(gallery.GetFeed(), "text/xml");
                }
            }
        }

        [HttpGet("{galleryId}/packages")]
        public IActionResult GetGalleryPackages([FromRoute] long galleryId, [FromQuery]int skip = 0, [FromQuery]int limit = 10)
        {
            using (var db = new VSExtensionManagerContext())
            {
                var packages = db.Packages.Where(g => g.GalleryId == galleryId).Skip(skip).Take(limit);

                return Ok(new
                {
                    Results = packages.ToList(),
                    Count = packages.Count(),
                    Skip = skip,
                    Limit = limit
                });
            }
        }

        [HttpGet]
        public IActionResult GetGalleries([FromQuery]int skip = 0, [FromQuery]int limit = 10)
        {
            using (var db = new VSExtensionManagerContext())
            {
                var query = db.Galleries.Where(g => true);
                return Ok(new
                {
                    Results = query.Skip(skip).Take(limit).ToList(),
                    Count = query.Count(),
                    Limit = limit,
                    Skip = skip
                });
            }
        }

        // GET api/values
        [HttpPost("{galleryId}/package_upload")]
        public IActionResult UploadVSIX([FromRoute] long galleryId, IFormFile file, [FromQuery] string iconPath = "")
        {
            using (var db = new VSExtensionManagerContext())
            {
                var gallery = db.Galleries.Find(galleryId);
                if (gallery == null) return NotFound(new
                {
                    Error = $"Gallery '{galleryId}' not found."
                });

                // full path to file in temp location
                var filePath = Path.GetTempFileName();

                using (var stream = new FileStream(filePath, FileMode.Create))
                {
                    file.CopyTo(stream);
                }

                var package = ProcessVsix(filePath);
                
                package.Content.Filename = file.FileName;
                package.Gallery = gallery;
                package.GalleryId = gallery.Id;

                db.Blobs.Add(package.Content);
                db.SaveChanges();
                if (package.Icon != null) db.Blobs.Add(package.Icon);
                db.SaveChanges();

                package.ContentId = package.Content.Id;
                if (package.Icon != null) package.IconId = package.Icon.Id;

                var existing = db.Packages.FirstOrDefault(p => p.PackageIdentifier == package.PackageIdentifier && p.GalleryId == galleryId);
                if (existing != null)
                {
                    existing.Name = package.Name;
                    existing.Author = package.Author;
                    existing.DatePublished = package.DatePublished;
                    existing.Description = package.Description;
                    existing.ExtensionList = package.ExtensionList;
                    //existing.FileName = package.FileName;
                    existing.GettingStartedUrl = package.GettingStartedUrl;
                    existing.Icon = package.Icon;
                    existing.Content = package.Content;
                    existing.Included = package.Included;
                    existing.IssueTracker = package.IssueTracker;
                    existing.License = package.License;
                    existing.MoreInfoUrl = package.MoreInfoUrl;
                    existing.Preview = package.Preview;
                    existing.ReleaseNotesUrl = package.ReleaseNotesUrl;
                    existing.Repo = package.Repo;
                    existing.SupportedVersions.Clear();
                    existing.SupportedVersions.AddRange(package.SupportedVersions);
                    existing.Tags = package.Tags;
                    existing.Version = package.Version;
                    package = existing;
                }
                else
                {
                    db.Packages.Add(package);
                }
                db.SaveChanges();
                package.Gallery.Packages = null;
                if (package.Icon != null) package.Icon.Data = null;
                package.Content.Data = null;    
                return Ok(package);
            }
        }

        private static Package ProcessVsix(string sourceVsixPath, string iconPath = "")
        {
            var contentBlob = new Blob();
            contentBlob.Data = System.IO.File.ReadAllBytes(sourceVsixPath);
            contentBlob.Filename = Path.GetFileName(sourceVsixPath);
            contentBlob.MimeType = "application/octet-stream";
            contentBlob.ContentLength = contentBlob.Data.Count();

            string temp = Path.GetTempPath();
            string tempFolder = Path.Combine(temp, Guid.NewGuid().ToString());

            try
            {
                Directory.CreateDirectory(tempFolder);
                ZipFile.ExtractToDirectory(sourceVsixPath, tempFolder);

                var parser = new VsixManifestParser();
                Package package = parser.CreateFromManifest(tempFolder, sourceVsixPath);

                package.Content = contentBlob;

                if (!string.IsNullOrEmpty(iconPath))
                {

                    var iconBlob = new Blob();
                    string currentDir = Path.GetDirectoryName(sourceVsixPath);
                    string sourceIconPath = Path.Combine(tempFolder, iconPath);

                    if (System.IO.File.Exists(sourceIconPath))
                    {
                        iconBlob.Data = System.IO.File.ReadAllBytes(sourceIconPath);
                        iconBlob.ContentLength = iconBlob.Data.Count();
                        iconBlob.Filename = Path.GetFileName(sourceIconPath);
                        switch(Path.GetExtension(sourceIconPath).ToLower()) {
                            case ".jpg":
                            case ".jpeg":
                                iconBlob.MimeType = "image/jpeg";
                            break;
                            case ".png":
                                iconBlob.MimeType = "image/png";
                            break;
                            case ".gif":
                                iconBlob.MimeType = "image/gif";
                            break;
                            case ".bmp":
                                iconBlob.MimeType = "image/bmp";
                            break;
                        }
                        package.Icon = iconBlob;
                    }
                }

                Console.WriteLine($"Parsed {sourceVsixPath}");

                return package;
            }
            finally
            {
                Directory.Delete(tempFolder, true);
            }
        }
    }

    public class VsixManifestParser
    {
        public Package CreateFromManifest(string tempFolder, string vsixFileName)
        {
            string xml = File.ReadAllText(Path.Combine(tempFolder, "extension.vsixmanifest"));
            xml = Regex.Replace(xml, "( xmlns(:\\w+)?)=\"([^\"]+)\"", string.Empty);

            var doc = new XmlDocument();
            doc.LoadXml(xml);

            var package = new Package();

            if (doc.GetElementsByTagName("DisplayName").Count > 0)
            {
                Vs2012Format(doc, package);
            }
            else
            {
                Vs2010Format(doc, package);
            }

            string license = ParseNode(doc, "License", false);
            if (!string.IsNullOrEmpty(license))
            {
                string path = Path.Combine(tempFolder, license);
                if (File.Exists(path))
                {
                    package.License = File.ReadAllText(path);
                }
            }

            AddExtensionList(package, tempFolder);

            return package;
        }

        private void AddExtensionList(Package package, string tempFolder)
        {
            string vsext = Directory.EnumerateFiles(tempFolder, "*.vsext", SearchOption.AllDirectories).FirstOrDefault();

            if (!string.IsNullOrEmpty(vsext))
            {
                string json = File.ReadAllText(vsext);

                using (var ms = new MemoryStream(Encoding.UTF8.GetBytes(json)))
                {
                    var serializer = new DataContractJsonSerializer(typeof(ExtensionList));
                    var list = (ExtensionList)serializer.ReadObject(ms);
                    package.ExtensionList = list;
                }
            }
        }

        private void Vs2012Format(XmlDocument doc, Package package)
        {
            package.PackageIdentifier = ParseNode(doc, "Identity", true, "Id");
            package.Name = ParseNode(doc, "DisplayName", true);
            package.Description = ParseNode(doc, "Description", true);
            package.Version = new Version(ParseNode(doc, "Identity", true, "Version")).ToString();
            package.Author = ParseNode(doc, "Identity", true, "Publisher");
            //package.Icon = ParseNode(doc, "Icon", false);
            package.Preview = ParseNode(doc, "PreviewImage", false);
            package.Tags = ParseNode(doc, "Tags", false);
            package.DatePublished = DateTime.UtcNow;
            package.SupportedVersions.Clear();
            package.SupportedVersions.AddRange(GetSupportedVersions(doc).ToList());
            package.ReleaseNotesUrl = ParseNode(doc, "ReleaseNotes", false);
            package.GettingStartedUrl = ParseNode(doc, "GettingStartedGuide", false);
            package.MoreInfoUrl = ParseNode(doc, "MoreInfo", false);
        }

        private void Vs2010Format(XmlDocument doc, Package package)
        {
            package.PackageIdentifier = ParseNode(doc, "Identifier", true, "Id");
            package.Name = ParseNode(doc, "Name", true);
            package.Description = ParseNode(doc, "Description", true);
            package.Version = new Version(ParseNode(doc, "Version", true)).ToString();
            package.Author = ParseNode(doc, "Author", true);
            //package.Icon = ParseNode(doc, "Icon", false);
            package.Preview = ParseNode(doc, "PreviewImage", false);
            package.DatePublished = DateTime.UtcNow;
            package.SupportedVersions.Clear();
            package.SupportedVersions.AddRange(GetSupportedVersions(doc).ToList());
            package.ReleaseNotesUrl = ParseNode(doc, "ReleaseNotes", false);
            package.GettingStartedUrl = ParseNode(doc, "GettingStartedGuide", false);
            package.MoreInfoUrl = ParseNode(doc, "MoreInfo", false);
        }

        private static IEnumerable<string> GetSupportedVersions(XmlDocument doc)
        {
            XmlNodeList list = doc.GetElementsByTagName("InstallationTarget");

            if (list.Count == 0)
                list = doc.GetElementsByTagName("<VisualStudio");

            var versions = new List<string>();

            foreach (XmlNode node in list)
            {
                string raw = node.Attributes["Version"].Value.Trim('[', '(', ']', ')');
                string[] entries = raw.Split(',');

                foreach (string entry in entries)
                {
                    if (Version.TryParse(entry, out Version v) && !versions.Contains(v.ToString()))
                    {
                        versions.Add(v.ToString());
                    }
                }
            }

            return versions;
        }

        private string ParseNode(XmlDocument doc, string name, bool required, string attribute = "")
        {
            XmlNodeList list = doc.GetElementsByTagName(name);

            if (list.Count > 0)
            {
                XmlNode node = list[0];

                if (string.IsNullOrEmpty(attribute))
                    return node.InnerText;

                XmlAttribute attr = node.Attributes[attribute];

                if (attr != null)
                    return attr.Value;
            }

            if (required)
            {
                string message = string.Format("Attribute '{0}' could not be found on the '{1}' element in the .vsixmanifest file.", attribute, name);
                throw new Exception(message);
            }

            return null;
        }

    }
}
