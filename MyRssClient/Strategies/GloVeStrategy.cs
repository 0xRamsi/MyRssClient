using Microsoft.EntityFrameworkCore;
using MyRssClient.Data;
using MyRssClient.Data.Ranking;
using MyRssClient.Data.Ranking.WordEmbedding;
using MyRssClient.Models;

namespace MyRssClient.Strategies {
    public class GloVeStrategy : ISortFilterStrategy {
        private GloVe glove = new();
        public async Task<ICollection<Post>> ProcessAsync(IDbContextFactory<MyContext> contextFactory, int NumberOfPostsToDisplayPerPage) {
            await using var context = await contextFactory.CreateDbContextAsync();
            glove = TrainModel(context);

            double[] centroid = Similarity.CalculateCentroid(GetQueryData(context).Select(p => glove.Transform(p)));
            var sortedPosts = GetAllData(context).AsEnumerable().OrderByDescending(p => GetSimilarty(p, centroid));
            return sortedPosts.Take(NumberOfPostsToDisplayPerPage).ToList();

            throw new NotImplementedException();
        }

        private double GetSimilarty(Post p, double[] centroid) {
            var embeddings = Array.ConvertAll(glove.Transform(p.Title), x => (double)x);
            return Similarity.Cosine(embeddings, centroid);
        }

        private static GloVe TrainModel(MyContext context) {
            var corpus = GetTrainingData(context);
            var glove = new GloVe();
            glove.BuildVocabulary(corpus);
            glove.BuildCooccurrence(corpus);
            glove.Train(epochs: 30);
            return glove;
        }

        private static IQueryable<string> GetTrainingData(MyContext context) => GetAllData(context).Select(p => p.Title);
        private static IQueryable<Post> GetAllData(MyContext context) {
            return context.Posts
                .OrderByDescending(i => i.PublishDate.ToString())
                .Include(p => p.ParentChannel)
                .ThenInclude(c => c.Images)
                .Include(p => p.Images);
        }

        private static IQueryable<string> GetQueryData(MyContext context) {
            return context.Posts
                .Where(p => p.IsLiked)
                .Select(p => p.Title);
        }
    }
}
