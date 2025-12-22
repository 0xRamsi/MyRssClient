using Microsoft.EntityFrameworkCore;
using MyRssClient.Data;
using MyRssClient.Strategies;

namespace MyRssClient.Services {
    public class GetDataService {
        private static readonly IDictionary<string, ISortFilterStrategy> _rankingAlgorithms = new Dictionary<string, ISortFilterStrategy> {
            ["Chronological"] = new ChronologicalStrategy(),
            ["Random"] = new RandomStrategy(),
            ["Unread"] = new UnreadStrategy(),
            ["Liked"] = new LikedStrategy(),
            ["Other ranking algorithm"] = new EmptyListStrategy(),
        };

        public IDbContextFactory<MyContext> _contextFactory { get; }

        public GetDataService(IDbContextFactory<MyRssClient.Data.MyContext> DbFactory) {
            this._contextFactory = DbFactory;
        }

        public IEnumerable<string> GetRankingNames() {
            return _rankingAlgorithms.Select(pair => pair.Key);
        }

        public async Task<ICollection<MyRssClient.Models.Post>> GetData(string algorithmName, int NumberOfPostsToDisplayPerPage) {
            if (!_rankingAlgorithms.TryGetValue(algorithmName, out var rankingObject)) {
                throw new ArgumentException("Invalid ranking");
            }
            return await rankingObject.ProcessAsync(_contextFactory, NumberOfPostsToDisplayPerPage);
        }
    }
}
