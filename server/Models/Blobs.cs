using System;
using System.ComponentModel.DataAnnotations.Schema;

namespace VSExtensionManager.Models
{
    public class Blob
    {
        public long Id { get; set; }
        public int ContentLength { get; set; }
        public byte[] Data { get; set; }
        public string Filename { get; set; }
        public string MimeType { get; set; }
    }
}