namespace MyRssClient.Models {
    public  class ChannelImage : Image {
        public required Channel Channel { get; set; }
        public bool IsSelected { get; set; }
    }
}