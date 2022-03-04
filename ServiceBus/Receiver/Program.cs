namespace MessageReader;



public class Program
{
    private const string serviceConnectionString = "Endpoint=sb://speedrunanypercent.servicebus.windows.net/;SharedAccessKeyName=RootManageSharedAccessKey;SharedAccessKey=xA3J6qp526AlxokoN1u4YaGuSSXwmzxM0rCTc6PnkWo=";
    static string queueName = "messagequeue";
    static ServiceBusClient? client;
    static ServiceBusProcessor? processor;

    static async Task MessageHandler(ProcessMessageEventArgs args)
    {
        string body = args.Message.Body.ToString();
        Console.WriteLine($"Received: {body}");
        //await args.CompleteMessageAsync (args.Message); // Not possible in ReceiveAndDelete mode
    }

    static Task ErrorHandler(ProcessErrorEventArgs args)
    {
        Console.WriteLine(args.Exception.ToString());
        return Task.CompletedTask;
    }

    static async Task Main(string[] args)
    {
        client = new ServiceBusClient(serviceConnectionString);
        
        processor = client.CreateProcessor(queueName, new ServiceBusProcessorOptions(){
                ReceiveMode = ServiceBusReceiveMode.ReceiveAndDelete,
                PrefetchCount = 25,
                AutoCompleteMessages = true,
                MaxConcurrentCalls = 10
            });
        
        try
        {
            processor.ProcessMessageAsync += MessageHandler;
            processor.ProcessErrorAsync += ErrorHandler;

            await processor.StartProcessingAsync();
            Console.WriteLine("Wait for a minute and then press any key to end the processing");
            Console.ReadKey();

            Console.WriteLine("\nStopping the receiver...");
            await processor.StopProcessingAsync();
            Console.WriteLine("Stopped receiving messages");
        }
        finally
        {
            await processor.DisposeAsync();
            await client.DisposeAsync();
        }
    }

}