using Microsoft.EntityFrameworkCore;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace MyRssClient.Models {
    public  class PostImage : Image {
        public required Post Post { get; set; }
        public bool IsSelected { get; set; }
    }
}