namespace MyRssClient.Data {
    public interface IConverterService<FROM_TYPE, TO_CHANNEL_TYPE, TO_POST_TYPE> {
        Task<TO_CHANNEL_TYPE> ConvertAsync(FROM_TYPE input, Uri rssLink);
        Task<ICollection<TO_POST_TYPE>> ConvertItems(FROM_TYPE input, TO_CHANNEL_TYPE channel);
        Task<byte[]?> GetMyImage(Uri path);
    }
}
