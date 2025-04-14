using System.Text;
using System.Text.Json;

namespace Capyndex.DataSeeding;

public class DataSeeder
{
    private readonly HttpClient _httpClient;
    private readonly Random _random = new();

    public DataSeeder(string baseUrl)
    {
        _httpClient = new HttpClient
        {
            BaseAddress = new Uri(baseUrl)
        };
    }

    public async Task SeedDataAsync(int count = 100)
    {
        Console.WriteLine($"Starting to seed {count} documents...");

        for (int i = 0; i < count; i++)
        {
            var content = GenerateRandomContent();
            await UploadContentAsync(content);

            if (i % 10 == 0)
            {
                Console.WriteLine($"Uploaded {i} documents");
            }
        }

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
            words.Add(_random.NextDouble() < 0.7
                ? Words.CommonWords[_random.Next(Words.CommonWords.Length)]
                : Words.LessCommonWords[_random.Next(Words.LessCommonWords.Length)]);
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