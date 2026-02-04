using FluentAssertions;
using OpenRouter.SDK.Models;
using OpenRouter.SDK.Services;
using System.Text.Json;

namespace OpenRouter.SDK.Tests;

public class ConversationStateTests
{
    #region ConversationStatus Tests

    [Fact]
    public void ConversationStatus_ShouldHaveAllStates()
    {
        // Assert
        Enum.GetValues<ConversationStatus>()
            .Should()
            .BeEquivalentTo(new[]
            {
                ConversationStatus.Complete,
                ConversationStatus.Interrupted,
                ConversationStatus.AwaitingApproval,
                ConversationStatus.InProgress
            });
    }

    #endregion

    #region ConversationState Tests

    [Fact]
    public void ConversationState_ShouldSerializeToJson()
    {
        // Arrange
        var state = new ConversationState
        {
            Id = "conv_123",
            Messages = new List<ResponsesInputItem>
            {
                new() { Type = "text", Text = "Hello" }
            },
            Status = ConversationStatus.InProgress,
            CreatedAt = 1234567890000,
            UpdatedAt = 1234567890000
        };

        // Act
        var json = JsonSerializer.Serialize(state);

        // Assert
        json.Should().Contain("\"id\":\"conv_123\"");
        json.Should().Contain("\"status\":\"in_progress\"");
    }

    [Fact]
    public void ConversationState_ShouldDeserializeFromJson()
    {
        // Arrange
        var json = """
            {
                "id": "conv_456",
                "messages": [{"type": "text", "text": "Hi"}],
                "status": "complete",
                "created_at": 1234567890000,
                "updated_at": 1234567890000
            }
            """;

        // Act
        var state = JsonSerializer.Deserialize<ConversationState>(json);

        // Assert
        state.Should().NotBeNull();
        state!.Id.Should().Be("conv_456");
        state.Status.Should().Be(ConversationStatus.Complete);
        state.Messages.Should().HaveCount(1);
    }

    [Fact]
    public void ConversationState_ShouldSupportMetadata()
    {
        // Arrange
        var state = new ConversationState
        {
            Id = "conv_789",
            Messages = new List<ResponsesInputItem>(),
            Status = ConversationStatus.InProgress,
            CreatedAt = 1234567890000,
            UpdatedAt = 1234567890000,
            Metadata = new Dictionary<string, object>
            {
                ["userId"] = "user_123",
                ["sessionId"] = "session_456"
            }
        };

        // Act
        var json = JsonSerializer.Serialize(state);
        var deserialized = JsonSerializer.Deserialize<ConversationState>(json);

        // Assert
        deserialized.Should().NotBeNull();
        deserialized!.Metadata.Should().ContainKey("userId");
        deserialized.Metadata.Should().ContainKey("sessionId");
    }

    #endregion

    #region InMemoryStateAccessor Tests

    [Fact]
    public async Task InMemoryStateAccessor_ShouldSaveAndLoad()
    {
        // Arrange
        var accessor = new InMemoryStateAccessor();
        var state = new ConversationState
        {
            Id = "conv_001",
            Messages = new List<ResponsesInputItem>
            {
                new() { Type = "text", Text = "Test message" }
            },
            Status = ConversationStatus.InProgress,
            CreatedAt = 1234567890000,
            UpdatedAt = 1234567890000
        };

        // Act
        await accessor.SaveAsync(state);
        var loaded = await accessor.LoadAsync();

        // Assert
        loaded.Should().NotBeNull();
        loaded!.Id.Should().Be("conv_001");
        loaded.Messages.Should().HaveCount(1);
        loaded.Status.Should().Be(ConversationStatus.InProgress);
    }

    [Fact]
    public async Task InMemoryStateAccessor_ShouldReturnNullWhenEmpty()
    {
        // Arrange
        var accessor = new InMemoryStateAccessor();

        // Act
        var loaded = await accessor.LoadAsync();

        // Assert
        loaded.Should().BeNull();
    }

    [Fact]
    public async Task InMemoryStateAccessor_ShouldOverwriteOnSave()
    {
        // Arrange
        var accessor = new InMemoryStateAccessor();
        var state1 = new ConversationState
        {
            Id = "conv_001",
            Messages = new List<ResponsesInputItem>(),
            Status = ConversationStatus.InProgress,
            CreatedAt = 1000,
            UpdatedAt = 1000
        };
        var state2 = new ConversationState
        {
            Id = "conv_002",
            Messages = new List<ResponsesInputItem>(),
            Status = ConversationStatus.Complete,
            CreatedAt = 2000,
            UpdatedAt = 2000
        };

        // Act
        await accessor.SaveAsync(state1);
        await accessor.SaveAsync(state2);
        var loaded = await accessor.LoadAsync();

        // Assert
        loaded.Should().NotBeNull();
        loaded!.Id.Should().Be("conv_002");
        loaded.Status.Should().Be(ConversationStatus.Complete);
    }

    #endregion

    #region DictionaryStateAccessor Tests

    [Fact]
    public async Task DictionaryStateAccessor_ShouldSaveAndLoadMultipleConversations()
    {
        // Arrange
        var storage = new Dictionary<string, ConversationState>();
        var accessor1 = DictionaryStateAccessor.Create(storage, "conv_001");
        var accessor2 = DictionaryStateAccessor.Create(storage, "conv_002");

        var state1 = new ConversationState
        {
            Id = "conv_001",
            Messages = new List<ResponsesInputItem>(),
            Status = ConversationStatus.InProgress,
            CreatedAt = 1000,
            UpdatedAt = 1000
        };
        var state2 = new ConversationState
        {
            Id = "conv_002",
            Messages = new List<ResponsesInputItem>(),
            Status = ConversationStatus.Complete,
            CreatedAt = 2000,
            UpdatedAt = 2000
        };

        // Act
        await accessor1.SaveAsync(state1);
        await accessor2.SaveAsync(state2);
        var loaded1 = await accessor1.LoadAsync();
        var loaded2 = await accessor2.LoadAsync();

        // Assert
        loaded1.Should().NotBeNull();
        loaded1!.Id.Should().Be("conv_001");
        loaded1.Status.Should().Be(ConversationStatus.InProgress);

        loaded2.Should().NotBeNull();
        loaded2!.Id.Should().Be("conv_002");
        loaded2.Status.Should().Be(ConversationStatus.Complete);

        storage.Should().HaveCount(2);
    }

    [Fact]
    public async Task DictionaryStateAccessor_ShouldReturnNullForNonExistentConversation()
    {
        // Arrange
        var storage = new Dictionary<string, ConversationState>();
        var accessor = DictionaryStateAccessor.Create(storage, "conv_999");

        // Act
        var loaded = await accessor.LoadAsync();

        // Assert
        loaded.Should().BeNull();
    }

    #endregion

    #region ConversationStateUtilities Tests

    [Fact]
    public void GenerateConversationId_ShouldGenerateUniqueIds()
    {
        // Act
        var id1 = ConversationStateUtilities.GenerateConversationId();
        var id2 = ConversationStateUtilities.GenerateConversationId();

        // Assert
        id1.Should().StartWith("conv_");
        id2.Should().StartWith("conv_");
        id1.Should().NotBe(id2);
    }

    [Fact]
    public void CreateInitialState_ShouldCreateStateWithGeneratedId()
    {
        // Act
        var state = ConversationStateUtilities.CreateInitialState();

        // Assert
        state.Id.Should().StartWith("conv_");
        state.Messages.Should().BeEmpty();
        state.Status.Should().Be(ConversationStatus.InProgress);
        state.CreatedAt.Should().BeGreaterThan(0);
        state.UpdatedAt.Should().BeGreaterThan(0);
    }

    [Fact]
    public void CreateInitialState_ShouldAcceptCustomId()
    {
        // Act
        var state = ConversationStateUtilities.CreateInitialState("custom_123");

        // Assert
        state.Id.Should().Be("custom_123");
        state.Status.Should().Be(ConversationStatus.InProgress);
    }

    [Fact]
    public void UpdateState_ShouldApplyChangesAndUpdateTimestamp()
    {
        // Arrange
        var state = ConversationStateUtilities.CreateInitialState();
        var originalUpdatedAt = state.UpdatedAt;
        Thread.Sleep(10); // Ensure timestamp changes

        // Act
        var updatedState = ConversationStateUtilities.UpdateState(state, s =>
        {
            s.Status = ConversationStatus.Complete;
        });

        // Assert
        updatedState.Status.Should().Be(ConversationStatus.Complete);
        updatedState.UpdatedAt.Should().BeGreaterThan(originalUpdatedAt);
    }

    [Fact]
    public void AppendToMessages_ShouldAddMessageAndUpdateTimestamp()
    {
        // Arrange
        var state = ConversationStateUtilities.CreateInitialState();
        var originalUpdatedAt = state.UpdatedAt;
        Thread.Sleep(10);

        var message = new ResponsesInputItem { Type = "text", Text = "Hello" };

        // Act
        ConversationStateUtilities.AppendToMessages(state, message);

        // Assert
        state.Messages.Should().HaveCount(1);
        state.Messages[0].Text.Should().Be("Hello");
        state.UpdatedAt.Should().BeGreaterThan(originalUpdatedAt);
    }

    [Fact]
    public void IsComplete_ShouldReturnTrueForCompleteStatus()
    {
        // Arrange
        var state = new ConversationState
        {
            Id = "test",
            Messages = new List<ResponsesInputItem>(),
            Status = ConversationStatus.Complete,
            CreatedAt = 1000,
            UpdatedAt = 1000
        };

        // Act & Assert
        ConversationStateUtilities.IsComplete(state).Should().BeTrue();
    }

    [Fact]
    public void RequiresApproval_ShouldReturnTrueWhenPendingToolCallsExist()
    {
        // Arrange
        var state = new ConversationState
        {
            Id = "test",
            Messages = new List<ResponsesInputItem>(),
            Status = ConversationStatus.AwaitingApproval,
            PendingToolCalls = new List<FunctionToolCall>
            {
                new() { Id = "call_1", Name = "test_tool", Arguments = "{}" }
            },
            CreatedAt = 1000,
            UpdatedAt = 1000
        };

        // Act & Assert
        ConversationStateUtilities.RequiresApproval(state).Should().BeTrue();
    }

    [Fact]
    public void RequiresApproval_ShouldReturnFalseWhenNoPendingToolCalls()
    {
        // Arrange
        var state = new ConversationState
        {
            Id = "test",
            Messages = new List<ResponsesInputItem>(),
            Status = ConversationStatus.InProgress,
            CreatedAt = 1000,
            UpdatedAt = 1000
        };

        // Act & Assert
        ConversationStateUtilities.RequiresApproval(state).Should().BeFalse();
    }

    [Fact]
    public void IsInterrupted_ShouldReturnTrueForInterruptedStatus()
    {
        // Arrange
        var state = new ConversationState
        {
            Id = "test",
            Messages = new List<ResponsesInputItem>(),
            Status = ConversationStatus.Interrupted,
            InterruptedBy = "user_action",
            CreatedAt = 1000,
            UpdatedAt = 1000
        };

        // Act & Assert
        ConversationStateUtilities.IsInterrupted(state).Should().BeTrue();
    }

    [Fact]
    public void MarkComplete_ShouldSetStatusAndUpdateTimestamp()
    {
        // Arrange
        var state = ConversationStateUtilities.CreateInitialState();
        var originalUpdatedAt = state.UpdatedAt;
        Thread.Sleep(10);

        // Act
        ConversationStateUtilities.MarkComplete(state);

        // Assert
        state.Status.Should().Be(ConversationStatus.Complete);
        state.UpdatedAt.Should().BeGreaterThan(originalUpdatedAt);
    }

    [Fact]
    public void MarkInterrupted_ShouldSetStatusAndReasonAndUpdateTimestamp()
    {
        // Arrange
        var state = ConversationStateUtilities.CreateInitialState();
        var originalUpdatedAt = state.UpdatedAt;
        Thread.Sleep(10);

        // Act
        ConversationStateUtilities.MarkInterrupted(state, "timeout");

        // Assert
        state.Status.Should().Be(ConversationStatus.Interrupted);
        state.InterruptedBy.Should().Be("timeout");
        state.UpdatedAt.Should().BeGreaterThan(originalUpdatedAt);
    }

    [Fact]
    public void MarkAwaitingApproval_ShouldSetStatusAndPendingCallsAndUpdateTimestamp()
    {
        // Arrange
        var state = ConversationStateUtilities.CreateInitialState();
        var originalUpdatedAt = state.UpdatedAt;
        Thread.Sleep(10);

        var toolCalls = new List<FunctionToolCall>
        {
            new() { Id = "call_1", Name = "get_weather", Arguments = "{\"city\":\"London\"}" }
        };

        // Act
        ConversationStateUtilities.MarkAwaitingApproval(state, toolCalls);

        // Assert
        state.Status.Should().Be(ConversationStatus.AwaitingApproval);
        state.PendingToolCalls.Should().HaveCount(1);
        state.PendingToolCalls![0].Name.Should().Be("get_weather");
        state.UpdatedAt.Should().BeGreaterThan(originalUpdatedAt);
    }

    #endregion
}
