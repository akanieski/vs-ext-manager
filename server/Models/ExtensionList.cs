using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Runtime.Serialization;

namespace VSExtensionManager.Models
{
    [DataContract]
    public class ExtensionList
    {
        [Key]
        [DataMember(Name = "DbId")]
        public long Id { get; set; }

        [DataMember(Name = "id")]
        public string PackageIdentifier { get; set; }


        [DataMember(Name = "name")]
        public string Name { get; set; }

        [DataMember(Name = "version")]
        public string Version { get; set; }

        [DataMember(Name = "extensions")]
        public List<Extension> Extensions { get; set; }

        public override string ToString()
        {
            return Name;
        }
    }
}