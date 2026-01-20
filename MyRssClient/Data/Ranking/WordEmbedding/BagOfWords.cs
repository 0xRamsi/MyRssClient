using MyRssClient.Data.Ranking;

public class BagOfWords {
    private readonly Dictionary<string, int> _vocabulary = new Dictionary<string, int>();

    // Build vocabulary from a collection of documents
    public void Fit(IEnumerable<string> documents) {
        foreach (var doc in documents) {
            foreach (var token in Tokenizer.Tokenize(doc)) {
                if (!_vocabulary.ContainsKey(token)) {
                    _vocabulary[token] = _vocabulary.Count;
                }
            }
        }
    }

    // Transform a single document into a BoW vector
    public double[] Transform(string document) {
        var vector = new double[_vocabulary.Count];
        var tokens = Tokenizer.Tokenize(document);

        foreach (var token in tokens) {
            if (_vocabulary.TryGetValue(token, out int index)) {
                vector[index]++;
            }
        }

        return vector;
    }

    // Transform multiple documents
    public IEnumerable<double[]> Transform(IEnumerable<string> documents) {
        return documents.Select(Transform);
    }

    // Optional: get vocabulary list
    public IReadOnlyDictionary<string, int> Vocabulary => _vocabulary;
}
