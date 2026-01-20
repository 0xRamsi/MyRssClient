namespace MyRssClient.Data.Ranking {
    public class Similarity {

        // Cosine similarity for comparing sentences
        public static double Cosine(double[] a, double[] b) {
            TestPreconditions(a, b);
            double dot = 0, a_vector_lenght = 0, b_vector_length = 0;
            for (int i = 0; i < a.Length; i++) {
                dot += a[i] * b[i];
                a_vector_lenght += a[i] * a[i];
                b_vector_length += b[i] * b[i];
            }
            return dot / (Math.Sqrt(a_vector_lenght) * Math.Sqrt(b_vector_length));
        }
        public static double EuclideanDistance(double[] a, double[] b) {
            TestPreconditions(a, b);
            double sum = 0;
            for (int i = 0; i < a.Length; i++) {
                double diff = a[i] - b[i];
                sum += diff * diff;
            }

            return Math.Sqrt(sum);
        }

        private static void TestPreconditions(double[] a, double[] b) {
            if (a == null || b == null) {
                throw new ArgumentNullException("Vectors must not be null.");
            }

            if (a.Length != b.Length) {
                throw new ArgumentException("Vectors must have the same length.");
            }
        }

        public static double[] CalculateCentroid(IEnumerable<double[]> vectors) {
            if (vectors == null || !vectors.Any()) {
                throw new ArgumentException("List of vectors is empty or null.");
            }

            int dimensions = vectors.First().Length;
            double[] centroid = new double[dimensions];

            foreach (var vector in vectors) {
                if (vector.Length != dimensions)
                    throw new ArgumentException("All vectors must have the same number of dimensions.");

                for (int i = 0; i < dimensions; i++) {
                    centroid[i] += vector[i];
                }
            }

            for (int i = 0; i < dimensions; i++) {
                centroid[i] /= vectors.Count();
            }

            return centroid;
        }
    }
}