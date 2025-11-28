using System.ServiceModel.Syndication;

namespace MyRssClient.Data {
    public interface IRssService {
        Task<SyndicationFeed> Read_Channel(string? url);
    }
}
