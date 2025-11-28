namespace MyRssClient.Models {
    public class Movie {
        public Guid Id { get; set; }
        public string Title { get; set; }
        public ICollection<MovieImage> Images { get; set; } = [];
        public MovieImage? SelectedImage { get; set; }
    }
}
