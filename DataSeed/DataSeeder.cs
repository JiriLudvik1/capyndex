using Capyndex.DataSeeding;
using System.Text;
using System.Text.Json;

namespace DataSeed;

public class DataSeeder
{
    private readonly HttpClient _httpClient;
    private readonly Random _random = new();
    private readonly int _maxConcurrentRequests;

    public DataSeeder(string baseUrl, int maxConcurrentRequests = 200)
    {
        _httpClient = new HttpClient
        {
            BaseAddress = new Uri(baseUrl)
        };
        _maxConcurrentRequests = maxConcurrentRequests;
    }

    public async Task SeedDataAsync(int count = 100)
    {
        Console.WriteLine($"Starting to seed {count} documents with {_maxConcurrentRequests} parallel requests...");

        // Create all document contents first
        var contents = Enumerable.Range(0, count)
            .Select(_ => GenerateRandomContent())
            .ToList();

        // Progress counter
        int processedCount = 0;

        // Create a list to hold all tasks
        List<Task> allTasks = new List<Task>();

        // Use SemaphoreSlim to control concurrency
        using var throttler = new SemaphoreSlim(_maxConcurrentRequests);

        // Start all tasks with throttling
        for (int i = 0; i < count; i++)
        {
            // Wait for a slot before starting a new task
            await throttler.WaitAsync();

            int index = i; // Capture for closure

            // Start the task and add it to our collection
            Task task = Task.Run(async () =>
            {
                try
                {
                    await UploadContentAsync(contents[index]);

                    // Update progress
                    int current = Interlocked.Increment(ref processedCount);
                    if (current % 10 == 0 || current == count)
                    {
                        Console.WriteLine($"Uploaded {current}/{count} documents");
                    }
                }
                finally
                {
                    // Always release the semaphore when done
                    throttler.Release();
                }
            });

            allTasks.Add(task);
        }

        // Wait for all tasks to complete
        await Task.WhenAll(allTasks);

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