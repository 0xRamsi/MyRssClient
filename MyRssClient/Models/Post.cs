using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Diagnostics.CodeAnalysis;
using System.Runtime.Serialization;
using System.Text.Json;

namespace MyRssClient.Models {
    public class Post : Comparer<Post>, IComparer<Post> {
        [Key]
        public required Guid Guid { get; set; }
        public required string PostIdFromFeed { get; set; }
        public required string Title { get; set; }
        public required string Description { get; set; }
        [NotMapped]
        public required IEnumerable<Link> Links { get; set; }
        public string LinksAsJson { get => JsonSerializer.Serialize(Links); set => Links = JsonSerializer.Deserialize<IEnumerable<Link>>(value) ?? throw new SerializationException("Cannot parse Links from JSON: " + value); }
        public required string Authors { get; set; }
        public ICollection<PostImage> Images { get; set; }
        public required DateTimeOffset PublishDate { get; set; }
        public required DateTimeOffset FetchedAt { get; set; }
        public required bool IsRead { get; set; } = false;
        public required bool IsLiked { get; set; } = false;
        public required int ClickCounter { get;set; } = 0;
        public required Channel ParentChannel { get; set; }

        public override int Compare(Post? x, Post? y) {
            var isSame = x?.PostIdFromFeed.CompareTo(y?.PostIdFromFeed);
            if (isSame != null && isSame != 0) {
                return isSame.Value;
            }
            return x?.PublishDate.CompareTo(y?.PublishDate ?? DateTimeOffset.MinValue) ?? 0;
        }
    }

    public class PostsComparer : IEqualityComparer<Post> {
        public bool Equals(Post? x, Post? y) {
            if (x?.PostIdFromFeed == string.Empty) {
                return false;
            }
            return x?.PostIdFromFeed == y?.PostIdFromFeed;
        }

        public int GetHashCode([DisallowNull] Post obj) {
            return obj is Post post ? post.PostIdFromFeed.GetHashCode() : obj.GetHashCode();
        }
    }
}
