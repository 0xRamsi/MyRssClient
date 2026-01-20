using MyRssClient.Data.Ranking;

public class SifSentenceEmbedding : Similarity {
    private readonly double SifSmoothingParameter;
    private readonly Dictionary<string, double[]> wordVectors;
    private readonly Dictionary<string, double> wordFrequencies;

    public SifSentenceEmbedding(
        Dictionary<string, double[]> wordVectors,
        Dictionary<string, double> wordFrequencies,
        double smoothingParameter = 0.001) {
        this.wordVectors = wordVectors;
        this.wordFrequencies = wordFrequencies;
        this.SifSmoothingParameter = smoothingParameter;
    }

    // Compute SIF-weighted sentence vector
    public double[] SentenceVector(string sentence) {
        var tokens = Tokenizer.Tokenize(sentence);
        var vectors = new List<double[]>();

        foreach (var token in tokens) {
            if (!wordVectors.TryGetValue(token, out var vec))
                continue;

            double p = wordFrequencies.ContainsKey(token) ? wordFrequencies[token] : 1e-6;
            double weight = SifSmoothingParameter / (SifSmoothingParameter + p);

            vectors.Add(Scale(vec, weight));
        }

        if (vectors.Count == 0)
            return null;

        return Mean(vectors);
    }

    // Apply PCA and remove first principal component
    public double[][] RemoveFirstPrincipalComponent(double[][] sentenceVectors) {
        var pc = FirstPrincipalComponent(sentenceVectors);

        var result = new double[sentenceVectors.Length][];
        for (int i = 0; i < sentenceVectors.Length; i++) {
            result[i] = Subtract(sentenceVectors[i], Project(sentenceVectors[i], pc));
        }
        return result;
    }

    // --- Math helpers ---

    private double[] Mean(List<double[]> vectors) {
        int size = vectors[0].Length;
        var avg = new double[size];

        foreach (var v in vectors)
            for (int i = 0; i < size; i++)
                avg[i] += v[i];

        for (int i = 0; i < size; i++)
            avg[i] /= vectors.Count;

        return avg;
    }

    private double[] Scale(double[] v, double s) {
        var r = new double[v.Length];
        for (int i = 0; i < v.Length; i++)
            r[i] = v[i] * s;
        return r;
    }

    private double Dot(double[] a, double[] b) {
        double sum = 0;
        for (int i = 0; i < a.Length; i++)
            sum += a[i] * b[i];
        return sum;
    }

    private double[] Subtract(double[] a, double[] b) {
        var r = new double[a.Length];
        for (int i = 0; i < a.Length; i++)
            r[i] = a[i] - b[i];
        return r;
    }

    private double[] Project(double[] v, double[] pc) {
        double scale = Dot(v, pc);
        var r = new double[v.Length];
        for (int i = 0; i < v.Length; i++)
            r[i] = pc[i] * scale;
        return r;
    }

    // Compute first principal component using power iteration
    private double[] FirstPrincipalComponent(double[][] matrix, int iterations = 20) {
        int size = matrix[0].Length;
        var pc = new double[size];
        var rnd = new Random();

        for (int i = 0; i < size; i++)
            pc[i] = rnd.NextDouble();

        Normalize(pc);

        for (int iter = 0; iter < iterations; iter++) {
            var newPc = new double[size];

            foreach (var row in matrix) {
                double dot = Dot(row, pc);
                for (int i = 0; i < size; i++)
                    newPc[i] += row[i] * dot;
            }

            Normalize(newPc);
            pc = newPc;
        }

        return pc;
    }

    private void Normalize(double[] v) {
        double norm = Math.Sqrt(Dot(v, v));
        for (int i = 0; i < v.Length; i++)
            v[i] /= norm;
    }
}
