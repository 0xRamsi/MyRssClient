namespace MyRssClient.Data.Ranking.WordEmbedding {
    public class GloVe {
        private readonly int vectorSize;
        private readonly double learningRate;
        private readonly double xMax;
        private readonly double alpha;

        private Dictionary<string, int> wordToId = [];
        private List<string> idToWord = [];

        private double[][] W;      // word vectors
        private double[][] Wt;     // context word vectors
        private double[] bias;
        private double[] biasT;

        private Dictionary<string, double> frequencies;
        private Dictionary<(int, int), double> cooccurrence = [];

        public GloVe(int vectorSize = 50, double learningRate = 0.05, double xMax = 100, double alpha = 0.75) {
            this.vectorSize = vectorSize;
            this.learningRate = learningRate;
            this.xMax = xMax;
            this.alpha = alpha;
        }

        // Build vocabulary
        public void BuildVocabulary(IEnumerable<string> corpus) {
            foreach (var sentence in corpus) {
                foreach (var word in Tokenizer.Tokenize(sentence)) {
                    if (!wordToId.ContainsKey(word)) {
                        int id = wordToId.Count;
                        wordToId[word] = id;
                        idToWord.Add(word);
                    }
                }
            }

            int vocabSize = wordToId.Count;

            W = CreateMatrix(vocabSize, vectorSize);
            Wt = CreateMatrix(vocabSize, vectorSize);
            bias = new double[vocabSize];
            biasT = new double[vocabSize];

            Random rnd = new();
            for (int i = 0; i < vocabSize; i++) {
                for (int j = 0; j < vectorSize; j++) {
                    W[i][j] = (rnd.NextDouble() - 0.5) / vectorSize;
                    Wt[i][j] = (rnd.NextDouble() - 0.5) / vectorSize;
                }
            }
        }

        // Build co-occurrence matrix
        public void BuildCooccurrence(IEnumerable<string> corpus, int windowSize = 5) {
            foreach (var sentence in corpus) {
                var tokens = Tokenizer.Tokenize(sentence);
                for (int i = 0; i < tokens.Count; i++) {
                    if (!wordToId.TryGetValue(tokens[i], out int wi)) continue;

                    int start = Math.Max(0, i - windowSize);
                    int end = Math.Min(tokens.Count - 1, i + windowSize);

                    for (int j = start; j <= end; j++) {
                        if (i == j) continue;
                        if (!wordToId.TryGetValue(tokens[j], out int wj)) continue;

                        double distance = Math.Abs(i - j);
                        double weight = 1.0 / distance;

                        var key = (wi, wj);
                        if (!cooccurrence.ContainsKey(key))
                            cooccurrence[key] = 0;

                        cooccurrence[key] += weight;
                    }
                }
            }
            frequencies = ComputeFrequenciesFromCorpus(corpus);
        }

        private static Dictionary<string, double> ComputeFrequenciesFromCorpus(IEnumerable<string> corpus) {
            var counts = new Dictionary<string, long>(StringComparer.OrdinalIgnoreCase);
            long total = 0;

            foreach (var sentence in corpus)
            {
                var words = Tokenizer.Tokenize(sentence);

                foreach (var w in words)
                {
                    if (!counts.ContainsKey(w))
                        counts[w] = 0;

                    counts[w]++;
                    total++;
                }
            }

            // Convert counts to probabilities
            return counts.ToDictionary(
                kv => kv.Key,
                kv => (double)kv.Value / total
            );
        }

        // Train GloVe vectors
        public void Train(int epochs = 50) {
            foreach (var epoch in Enumerable.Range(1, epochs)) {
                foreach (var pair in cooccurrence) {
                    int i = pair.Key.Item1;
                    int j = pair.Key.Item2;
                    double x = pair.Value;

                    double weight = x < xMax ? Math.Pow(x / xMax, alpha) : 1.0;

                    double dot = Dot(W[i], Wt[j]);
                    double diff = dot + bias[i] + biasT[j] - Math.Log(x);

                    double grad = weight * diff;

                    for (int k = 0; k < vectorSize; k++) {
                        double tempWi = W[i][k];
                        double tempWj = Wt[j][k];

                        W[i][k] -= learningRate * grad * tempWj;
                        Wt[j][k] -= learningRate * grad * tempWi;
                    }

                    bias[i] -= learningRate * grad;
                    biasT[j] -= learningRate * grad;
                }

                Console.WriteLine($"Epoch {epoch}/{epochs} complete");
            }
        }

        // Get vector for a word
        public double[] GetVector(string word) {
            if (!wordToId.TryGetValue(word, out int id)) {
                return null;
            }

            return W[id];
        }

        public double[] Transform(string sentence) {
            Dictionary<string, double[]> wordVectors = Tokenizer.Tokenize(sentence)
                        .Select(token => new { token, vec = GetVector(token) })
                        .ToDictionary(x => x.token, x => x.vec);
            ;

            var sif = new SifSentenceEmbedding(wordVectors, frequencies);
            return sif.SentenceVector(sentence);
        }

        // Utility
        private double[][] CreateMatrix(int rows, int cols) {
            var m = new double[rows][];
            for (int i = 0; i < rows; i++)
                m[i] = new double[cols];
            return m;
        }

        private double Dot(double[] a, double[] b) {
            double sum = 0;
            for (int i = 0; i < a.Length; i++)
                sum += a[i] * b[i];
            return sum;
        }
    }
}