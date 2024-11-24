using Microsoft.Extensions.AI;
using Microsoft.Extensions.VectorData;
using Microsoft.SemanticKernel.Connectors.InMemory;
using Xunit.Abstractions;

namespace DimonSmart.Classifier.Tests
{
    public class ClassificationUnitTests
    {
        private readonly ITestOutputHelper _output;

        public ClassificationUnitTests(ITestOutputHelper output)
        {
            _output = output;
        }

        [Fact]
        public async Task Test1Async()
        {
            var movieData = TestData.GetMovieData();

            var vectorStore = new InMemoryVectorStore();

            var movies = vectorStore.GetCollection<int, Movie>("movies");

            await movies.CreateCollectionIfNotExistsAsync();

            IEmbeddingGenerator<string, Embedding<float>> generator =
                new OllamaEmbeddingGenerator(new Uri("http://localhost:11434/"), "llama3.2"); //all-minilm

            foreach (var movie in movieData)
            {
                movie.Vector = await generator.GenerateEmbeddingVectorAsync(movie.Description);
                await movies.UpsertAsync(movie);
            }

            var query = "A family friendly movie";
            var queryEmbedding = await generator.GenerateEmbeddingVectorAsync(query);
            var searchOptions = new VectorSearchOptions()
            {
                Top = 2,
                VectorPropertyName = "Vector"
            };

            var results = await movies.VectorizedSearchAsync(queryEmbedding, searchOptions);

            await foreach (var result in results.Results)
            {
                _output.WriteLine($"Title: {result.Record.Title}");
                _output.WriteLine($"Description: {result.Record.Description}");
                _output.WriteLine($"Score: {result.Score}");
                _output.WriteLine("");
            }
        }
    }
}