class Program
{
    static async Task Main(string[] args)
    {
        if (args.Length == 0)
        {
            Console.WriteLine("Usage: dotnet run <configFilePath>");
            return;
        }

        string configFilePath = args[0]; // Get config file path from command-line argument
        SalesforceConfig config = await SalesforceDataProcessor.LoadConfigAsync(configFilePath);

        SalesforceDataProcessor processor = new(config);
        await processor.ProcessChildSObjectsAsync();
    }
}
