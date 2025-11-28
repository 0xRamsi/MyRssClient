using MyRssClient.Models;
using Microsoft.EntityFrameworkCore;

namespace MyRssClient.Data {
    public class MyContext(DbContextOptions<MyContext> options) : DbContext(options) {
        //public DbSet<Models.Movie> Movies { get; set; }
        public DbSet<Models.Channel> Channels { get; set; }
        public DbSet<Models.Post> Posts { get; set; }
        public DbSet<Models.ChannelImage> ChannelImages { get; set; }
    }
}
