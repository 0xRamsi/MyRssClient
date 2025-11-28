using Microsoft.EntityFrameworkCore;
using System.ComponentModel.DataAnnotations;

namespace MyRssClient.Models {
    public class Link {
        public required Uri Uri { get; set; }
        public required string Title { get; set; }
    }
}