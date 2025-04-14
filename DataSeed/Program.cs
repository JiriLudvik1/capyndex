using Capyndex.DataSeeding;
using DataSeed;

try
{
    Console.WriteLine("Data Seeder Console Application");
    Console.WriteLine("-------------------------------");

    var baseUrl = "http://localhost:5000";
    var documentCount = 3000;

// Allow command line arguments to override defaults
    if (args.Length > 0)
    {
        baseUrl = args[0];
    }

    if (args.Length > 1 && int.TryParse(args[1], out int count))
    {
        documentCount = count;
    }

    Console.WriteLine($"Using base URL: {baseUrl}");
    Console.WriteLine($"Documents to seed: {documentCount}");
    Console.ReadLine();

    var seeder = new DataSeeder(baseUrl);
    await seeder.SeedDataAsync(documentCount);

    Console.WriteLine("Press any key to exit...");
    Console.ReadKey();
}
catch (Exception ex)
{
    Console.WriteLine(ex);
    throw;
}