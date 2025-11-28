namespace MyRssClient.Models {
    public abstract class Image {
        public required string Id { get; set; }
        public required Uri Path { get; set; }
        public required byte[] ImageBytes { get; set; }
        public string ImageAsBase64 => "data:image/png;base64," + Convert.ToBase64String(ImageBytes);
    }
}