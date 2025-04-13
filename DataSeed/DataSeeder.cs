using Capyndex.DataSeeding;
using System.Text;
using System.Text.Json;

namespace Capyndex.DataSeeding
{
    public class UploadDataSeeder
    {
        private readonly HttpClient _httpClient;
        private readonly Random _random = new();

        // Common words that will be repeated in the generated content
        private readonly string[] _commonWords = new[]
        {
            "the", "and", "is", "in", "to", "with", "of", "for", "a", "on",
            "data", "index", "search", "document", "content", "text", "information",
            "system", "application", "user", "query", "result", "analysis", "database"
        };

        // Less frequent words to add variation
        private readonly string[] _lessCommonWords = new[]
        {
            "algorithm", "repository", "efficient", "structure", "optimize", "retrieve",
            "compute", "storage", "interface", "pipeline", "process", "semantic",
            "vector", "relevance", "ranking", "performance", "scalable", "distributed"
        };

        public UploadDataSeeder(string baseUrl)
        {
            _httpClient = new HttpClient
            {
                BaseAddress = new Uri(baseUrl)
            };
        }

        public async Task SeedDataAsync(int count = 100)
        {
            Console.WriteLine($"Starting to seed {count} documents...");

            var tasks = new List<Task>();
            for (int i = 0; i < count; i++)
            {
                var content = GenerateRandomContent();
                tasks.Add(UploadContentAsync(content));
            }

            await Task.WhenAll(tasks);

            Console.WriteLine("Data seeding completed!");
        }

        private string GenerateRandomContent()
        {
            var sentenceCount = _random.Next(3, 10);
            var sentences = new List<string>();

            for (int i = 0; i < sentenceCount; i++)
            {
                sentences.Add(GenerateRandomSentence());
            }

            return string.Join(" ", sentences);
        }

        private string GenerateRandomSentence()
        {
            var wordCount = _random.Next(5, 15);
            var words = new List<string>();

            for (int i = 0; i < wordCount; i++)
            {
                // 70% chance to use a common word to ensure repetition
                if (_random.NextDouble() < 0.7)
                {
                    words.Add(_commonWords[_random.Next(_commonWords.Length)]);
                }
                else
                {
                    words.Add(_lessCommonWords[_random.Next(_lessCommonWords.Length)]);
                }
            }

            var sentence = string.Join(" ", words);
            // Capitalize first letter and add period
            return char.ToUpper(sentence[0]) + sentence.Substring(1) + ".";
        }

        private async Task UploadContentAsync(string content)
        {
            var request = new
            {
                Content = content
            };

            var json = JsonSerializer.Serialize(request);
            var stringContent = new StringContent(json, Encoding.UTF8, "application/json");

            try
            {
                var response = await _httpClient.PostAsync("/upload", stringContent);
                response.EnsureSuccessStatusCode();
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error uploading content: {ex.Message}");
            }
        }
    }
}

public static class DataSeedingProgram
{
    public static void RunSeederAsync()
    {
        var baseUrl = "http://localhost:5000"; // Change this to your actual base URL
        var seeder = new UploadDataSeeder(baseUrl);

        Task.Run(() => seeder.SeedDataAsync(1500)).GetAwaiter().GetResult();
    }
}