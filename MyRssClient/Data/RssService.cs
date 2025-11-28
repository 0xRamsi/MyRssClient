using System.ServiceModel.Syndication;
using System.Xml;

namespace MyRssClient.Data {
    public class RssService : IRssService {
        public async Task<SyndicationFeed> Read_Channel(string? url) {
            return await Task.Run(() => { 
                using XmlReader reader = XmlReader.Create(url ?? string.Empty);
                return SyndicationFeed.Load(reader);
            });
            // Optimisation idea: Use HTTP If-Modified-Since: https://developer.mozilla.org/en-US/docs/Web/HTTP/Reference/Headers/If-Modified-Since
        }
    }
}
