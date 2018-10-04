using System.ComponentModel.DataAnnotations;
using System.Runtime.Serialization;

namespace VSExtensionManager.Models
{
    [DataContract]
    public class Extension
    {
        [Key]
        public long Id { get; set; }
        [DataMember(Name = "vsixId")]
        public string VsixId { get; set; }

        [DataMember(Name = "name")]
        public string Name { get; set; }

        public override string ToString()
        {
            return Name;
        }
    }
}