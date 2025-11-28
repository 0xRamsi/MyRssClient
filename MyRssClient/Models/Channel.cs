using System.ComponentModel.DataAnnotations;

namespace MyRssClient.Models {
    public class Channel {
        public required Guid Id { get; set; }
        public required string Title { get; set; }
        public required Uri RssLink { get; set; }
        [Display(Name = "URLs")]
        public required IList<Uri> URLs { get; set; }
        public required string Description { get; set; }
        public required string Language { get; set; }
        [Display(Name = "Last publish")]
        public required DateTimeOffset LastPublishDate { get; set; }
        [Display(Name = "Last fetch")]
        public required DateTimeOffset LastFetchAt { get; set; }
        public ICollection<ChannelImage> Images { get; set; } = [];
        public ChannelImage? SelectedImage => Images.FirstOrDefault(i => i.IsSelected);
        public ICollection<Post> Items { get; set; } = [];

        public static Channel GetDummyChannel() {
            return new() {
                Id = Guid.NewGuid(),
                Title = string.Empty,
                RssLink = new Uri("example.com"),
                URLs = [],
                Description = string.Empty,
                Language = string.Empty,
                LastPublishDate = DateTimeOffset.MinValue,
                LastFetchAt = DateTimeOffset.MinValue,
            };
        }
    }
}
