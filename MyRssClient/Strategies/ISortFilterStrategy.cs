using Microsoft.EntityFrameworkCore;
using MyRssClient.Data;
using MyRssClient.Models;

namespace MyRssClient.Strategies {
    public interface ISortFilterStrategy {
        Task<ICollection<Post>> ProcessAsync(IDbContextFactory<MyContext> context, int NumberOfPostsToDisplayPerPage);
    }
}
