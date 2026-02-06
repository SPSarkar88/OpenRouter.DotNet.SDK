using ApiTestProject;

Console.WriteLine("╔══════════════════════════════════════════════════════════╗");
Console.WriteLine("║        OpenRouter API Test Suite                        ║");
Console.WriteLine("╚══════════════════════════════════════════════════════════╝\n");

Console.WriteLine("Select test to run:");
Console.WriteLine("1. Test Raw Request (with reasoning)");
Console.WriteLine("2. Test API Call (simple)");
Console.WriteLine("3. Test Minimal (exact TypeScript match)");
Console.WriteLine("4. Test RestSharp");
Console.WriteLine("5. Test SDK with Reasoning");
Console.WriteLine("6. Test SDK Simple");
Console.WriteLine("7. Run all tests");
Console.Write("\nEnter choice (1-7): ");

var choice = Console.ReadLine();

switch (choice)
{
    case "1":
        Console.WriteLine("\n--- Running Test Raw Request ---\n");
        await TestRawRequest.RunAsync();
        break;
    case "2":
        Console.WriteLine("\n--- Running Test API Call ---\n");
        await TestApiCall.RunAsync();
        break;
    case "3":
        Console.WriteLine("\n--- Running Test Minimal ---\n");
        await TestMinimal.RunAsync();
        break;
    case "4":
        Console.WriteLine("\n--- Running Test RestSharp ---\n");
        await TestRestSharp.RunAsync();
        break;
    case "5":
        Console.WriteLine("\n--- Running Test SDK with Reasoning ---\n");
        await TestSDKWithReasoning.RunAsync();
        break;
    case "6":
        Console.WriteLine("\n--- Running Test SDK Simple ---\n");
        await TestSDKSimple.RunAsync();
        break;
    case "7":
        Console.WriteLine("\n--- Running All Tests ---\n");
        await TestSDKSimple.RunAsync();
        Console.WriteLine("\n\n--- Running Test SDK with Reasoning ---\n");
        await TestSDKWithReasoning.RunAsync();
        Console.WriteLine("\n\n--- Running Test RestSharp ---\n");
        await TestRestSharp.RunAsync();
        break;
    default:
        Console.WriteLine("Invalid choice. Running SDK tests...\n");
        Console.WriteLine("\n--- Running Test SDK Simple ---\n");
        await TestSDKSimple.RunAsync();
        Console.WriteLine("\n\n--- Running Test SDK with Reasoning ---\n");
        await TestSDKWithReasoning.RunAsync();
        break;
}

Console.WriteLine("\n\n✅ Tests completed!");
