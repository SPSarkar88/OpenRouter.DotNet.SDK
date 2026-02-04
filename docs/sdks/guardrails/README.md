# Guardrails

## Overview

Content moderation and safety guardrails management

### Available Operations

* [ListGuardrailsAsync](#listguardrailsasync) - List all guardrails
* [CreateGuardrailAsync](#createguardrailasync) - Create a new guardrail
* [GetGuardrailAsync](#getguardrailasync) - Get a single guardrail
* [UpdateGuardrailAsync](#updateguardrailasync) - Update a guardrail
* [DeleteGuardrailAsync](#deleteguardrailasync) - Delete a guardrail
* [ListKeyAssignmentsAsync](#listkeyassignmentsasync) - List API key guardrail assignments
* [ListMemberAssignmentsAsync](#listmemberassignmentsasync) - List member guardrail assignments
* [ListGuardrailKeyAssignmentsAsync](#listguardrailkeyassignmentsasync) - List key assignments for a guardrail
* [BulkAssignKeysAsync](#bulkassignkeysasync) - Bulk assign API keys to a guardrail
* [ListGuardrailMemberAssignmentsAsync](#listguardrailmemberassignmentsasync) - List member assignments for a guardrail
* [BulkAssignMembersAsync](#bulkassignmembersasync) - Bulk assign members to a guardrail
* [BulkUnassignKeysAsync](#bulkunassignkeysasync) - Bulk unassign API keys from a guardrail
* [BulkUnassignMembersAsync](#bulkunassignmembersasync) - Bulk unassign members from a guardrail

## ListGuardrailsAsync

List all guardrails for the authenticated user.

### Example Usage

```csharp
using OpenRouter.SDK;

var client = new OpenRouterClient("YOUR_API_KEY");

var result = await client.Guardrails.ListGuardrailsAsync();

foreach (var guardrail in result.Data)
{
    Console.WriteLine($"Guardrail: {guardrail.Name}");
    Console.WriteLine($"  ID: {guardrail.Id}");
    Console.WriteLine($"  Type: {guardrail.Type}");
}

// With pagination
var pagedResult = await client.Guardrails.ListGuardrailsAsync(
    offset: "0",
    limit: "10"
);
```

### Parameters

| Parameter           | Type              | Required           | Description                                   |
| ------------------- | ----------------- | ------------------ | --------------------------------------------- |
| `offset`            | string            | :heavy_minus_sign: | Number of records to skip for pagination      |
| `limit`             | string            | :heavy_minus_sign: | Maximum number of records to return (max 100) |
| `cancellationToken` | CancellationToken | :heavy_minus_sign: | Cancellation token for the operation          |

### Response

**Task\<ListGuardrailsResponse\>**

## CreateGuardrailAsync

Create a new guardrail for content moderation.

### Example Usage

```csharp
using OpenRouter.SDK;
using OpenRouter.SDK.Models;

var client = new OpenRouterClient("YOUR_API_KEY");

var request = new CreateGuardrailRequest
{
    Name = "Content Safety Filter",
    Type = "content_policy",
    Config = new GuardrailConfig
    {
        BlockedTopics = new List<string> { "violence", "hate_speech" },
        Severity = "high"
    }
};

var result = await client.Guardrails.CreateGuardrailAsync(request);

Console.WriteLine($"Created guardrail: {result.Id}");
```

### Parameters

| Parameter           | Type                     | Required           | Description                          |
| ------------------- | ------------------------ | ------------------ | ------------------------------------ |
| `request`           | CreateGuardrailRequest   | :heavy_check_mark: | Guardrail creation request           |
| `cancellationToken` | CancellationToken        | :heavy_minus_sign: | Cancellation token for the operation |

### Response

**Task\<GuardrailResponse\>**

## GetGuardrailAsync

Get a single guardrail by ID.

### Example Usage

```csharp
using OpenRouter.SDK;

var client = new OpenRouterClient("YOUR_API_KEY");

var result = await client.Guardrails.GetGuardrailAsync("guardrail_123");

Console.WriteLine($"Guardrail: {result.Name}");
Console.WriteLine($"Type: {result.Type}");
Console.WriteLine($"Created: {result.CreatedAt}");
```

### Parameters

| Parameter           | Type              | Required           | Description                          |
| ------------------- | ----------------- | ------------------ | ------------------------------------ |
| `id`                | string            | :heavy_check_mark: | Guardrail ID                         |
| `cancellationToken` | CancellationToken | :heavy_minus_sign: | Cancellation token for the operation |

### Response

**Task\<GuardrailResponse\>**

## UpdateGuardrailAsync

Update an existing guardrail.

### Example Usage

```csharp
using OpenRouter.SDK;
using OpenRouter.SDK.Models;

var client = new OpenRouterClient("YOUR_API_KEY");

var request = new UpdateGuardrailRequest
{
    Name = "Updated Guardrail Name",
    Config = new GuardrailConfig
    {
        BlockedTopics = new List<string> { "violence", "hate_speech", "spam" }
    }
};

var result = await client.Guardrails.UpdateGuardrailAsync("guardrail_123", request);

Console.WriteLine($"Updated: {result.Name}");
```

### Parameters

| Parameter           | Type                     | Required           | Description                          |
| ------------------- | ------------------------ | ------------------ | ------------------------------------ |
| `id`                | string                   | :heavy_check_mark: | Guardrail ID                         |
| `request`           | UpdateGuardrailRequest   | :heavy_check_mark: | Update request                       |
| `cancellationToken` | CancellationToken        | :heavy_minus_sign: | Cancellation token for the operation |

### Response

**Task\<GuardrailResponse\>**

## DeleteGuardrailAsync

Delete an existing guardrail.

### Example Usage

```csharp
using OpenRouter.SDK;

var client = new OpenRouterClient("YOUR_API_KEY");

await client.Guardrails.DeleteGuardrailAsync("guardrail_123");

Console.WriteLine("Guardrail deleted successfully");
```

### Parameters

| Parameter           | Type              | Required           | Description                          |
| ------------------- | ----------------- | ------------------ | ------------------------------------ |
| `id`                | string            | :heavy_check_mark: | Guardrail ID                         |
| `cancellationToken` | CancellationToken | :heavy_minus_sign: | Cancellation token for the operation |

### Response

**Task**

## BulkAssignKeysAsync

Assign multiple API keys to a specific guardrail.

### Example Usage

```csharp
using OpenRouter.SDK;
using OpenRouter.SDK.Models;

var client = new OpenRouterClient("YOUR_API_KEY");

var request = new BulkAssignKeysRequest
{
    KeyHashes = new List<string>
    {
        "key_hash_1",
        "key_hash_2",
        "key_hash_3"
    }
};

var result = await client.Guardrails.BulkAssignKeysAsync("guardrail_123", request);

Console.WriteLine($"Success: {result.Success}");
Console.WriteLine($"Assigned: {result.AssignedCount} keys");
```

### Parameters

| Parameter           | Type                   | Required           | Description                          |
| ------------------- | ---------------------- | ------------------ | ------------------------------------ |
| `id`                | string                 | :heavy_check_mark: | Guardrail ID                         |
| `request`           | BulkAssignKeysRequest  | :heavy_check_mark: | Bulk assignment request              |
| `cancellationToken` | CancellationToken      | :heavy_minus_sign: | Cancellation token for the operation |

### Response

**Task\<BulkOperationResponse\>**

### Errors

| Error Type                  | Status Code | Content Type     |
| --------------------------- | ----------- | ---------------- |
| BadRequestException         | 400         | application/json |
| UnauthorizedException       | 401         | application/json |
| ForbiddenException          | 403         | application/json |
| NotFoundException           | 404         | application/json |
| InternalServerException     | 500         | application/json |
| OpenRouterException         | 4XX, 5XX    | \*/\*            |
