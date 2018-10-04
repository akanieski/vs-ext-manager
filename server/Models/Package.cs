using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.IO;
using Newtonsoft.Json;

namespace VSExtensionManager.Models
{
    public class Package : IPreSaveHooked
    {
        public Package()
        {
            Included = true;
        }
        [Key]
        public long Id { get; set; }
        public string PackageIdentifier { get; set; }
        public long? ContentId { get; set; }
        public Blob Content { get; set; }
        public string Name { get; set; }
        public string Description { get; set; }
        public string Author { get; set; }
        public string Version { get; set; }
        public long? IconId { get; set; }
        public Blob Icon { get; set; }
        public string Preview { get; set; }
        public string Tags { get; set; }
        public DateTime DatePublished { get; set; }
        [Column("SupportedVersions")]
        public string _supportedVersionsString { get; set; }
        [NotMapped]
        [JsonIgnore]
        private List<string> _supportedVersions { get; set; }

        [NotMapped]
        public List<string> SupportedVersions
        {
            get
            {
                if (_supportedVersions == null)
                {
                    _supportedVersions = new List<string>((string.IsNullOrEmpty(_supportedVersionsString) ? "" : _supportedVersionsString).Split(','));
                }
                return _supportedVersions;
            }
        }
        public string License { get; set; }
        public string GettingStartedUrl { get; set; }
        public string ReleaseNotesUrl { get; set; }
        public string MoreInfoUrl { get; set; }
        public string Repo { get; set; }
        public string IssueTracker { get; set; }
        public ExtensionList ExtensionList { get; set; }

        [Newtonsoft.Json.JsonProperty(NullValueHandling = NullValueHandling.Ignore)]
        public Gallery Gallery { get; set; }
        public long GalleryId { get; set; }


        public bool Included { get; set; }

        public override string ToString()
        {
            return Name;
        }

        public void OnSave()
        {
            _supportedVersionsString = string.Join(", ", SupportedVersions);
        }
    }
}