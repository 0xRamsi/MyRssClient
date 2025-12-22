using Microsoft.EntityFrameworkCore;
using MyRssClient.Data;
using MyRssClient.Models;

namespace MyRssClient.Strategies {
    public class EmptyListStrategy : ISortFilterStrategy {
        public Task<ICollection<Post>> ProcessAsync(IDbContextFactory<MyContext> contextFactory, int NumberOfPostsToDisplayPerPage) {
            return Task.FromResult<ICollection<Post>>([]);
        }
    }
}
