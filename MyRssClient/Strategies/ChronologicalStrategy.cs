using Microsoft.EntityFrameworkCore;
using MyRssClient.Data;
using MyRssClient.Models;

namespace MyRssClient.Strategies {
    public class ChronologicalStrategy : ISortFilterStrategy {
        public async Task<ICollection<Post>> ProcessAsync(IDbContextFactory<MyContext> contextFactory, int NumberOfPostsToDisplayPerPage) {
            await using var context = await contextFactory.CreateDbContextAsync();
            var result = context.Posts
                .OrderByDescending(i => i.PublishDate.ToString())
                .Include(p => p.ParentChannel)
                .ThenInclude(c => c.Images)
                .Include(p => p.Images)
                .Take(NumberOfPostsToDisplayPerPage)
            ;
            return await Task.FromResult(result.ToList());
        }
    }
}
