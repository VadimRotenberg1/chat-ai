using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.SignalR;
using Microsoft.Extensions.AI;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Moq;
using WebApplication2.Contracts;
using WebApplication2.Options;
using WebApplication2.Services;

namespace WebApplication2.Tests;

public class ChatResponseServiceTests
{
    [Fact]
    public async Task SendAnswerAsync_StreamsResponsesToClient()
    {
        // Arrange
        var conversationId = "test-conversation";
        var message = "Hello";
        var request = new ChatRequest(message, conversationId);

        var mockClientProxy = new Mock<IClientProxy>();

        var mockChatClient = new Mock<IChatClient>();
        var chatUpdates = new[]
        {
            new ChatResponseUpdate { Contents = [new TextContent("Hi ")] },
            new ChatResponseUpdate { Contents = [new TextContent("there!")] }
        };
        mockChatClient.Setup(c => c.GetStreamingResponseAsync(It.IsAny<IEnumerable<ChatMessage>>(), It.IsAny<ChatOptions>(), It.IsAny<CancellationToken>()))
            .Returns(chatUpdates.ToAsyncEnumerable());

        var serviceProvider = new ServiceCollection()
            .AddSingleton<IChatClient>(mockChatClient.Object)
            .BuildServiceProvider();

        var logger = new Mock<ILogger<ChatResponseService>>().Object;
        var mockEnvironment = new Mock<IWebHostEnvironment>();
        mockEnvironment.Setup(m => m.ContentRootPath).Returns(Directory.GetCurrentDirectory());

        var mockAiOptions = new Mock<IOptions<AiOptions>>();
        mockAiOptions.Setup(o => o.Value).Returns(new AiOptions { AgentName = "assistant" });

        var service = new ChatResponseService(serviceProvider, mockEnvironment.Object, mockAiOptions.Object, logger);

        // Act
        await service.SendAnswerAsync(mockClientProxy.Object, request, CancellationToken.None);

        // Assert
        mockClientProxy.Verify(
            c => c.SendCoreAsync(ChatClientEvents.AssistantStarted, It.IsAny<object[]>(), It.IsAny<CancellationToken>()),
            Times.Once);

        mockClientProxy.Verify(
            c => c.SendCoreAsync(ChatClientEvents.AssistantChunk, It.Is<object[]>(args => args != null && args[0].ToString()!.Contains("Hi ")), It.IsAny<CancellationToken>()),
            Times.Once);

        mockClientProxy.Verify(
            c => c.SendCoreAsync(ChatClientEvents.AssistantChunk, It.Is<object[]>(args => args != null && args[0].ToString()!.Contains("there!")), It.IsAny<CancellationToken>()),
            Times.Once);

        mockClientProxy.Verify(
            c => c.SendCoreAsync(ChatClientEvents.AssistantCompleted, It.IsAny<object[]>(), It.IsAny<CancellationToken>()),
            Times.Once);
    }

    [Fact]
    public async Task SendAnswerAsync_UsesConfiguredAgentSkillsAsSystemPrompt()
    {
        // Arrange
        var tempRoot = Path.Combine(Path.GetTempPath(), $"webapp-skills-{Guid.NewGuid():N}");
        var skillsDirectory = Path.Combine(tempRoot, "Agents", "assistant");
        Directory.CreateDirectory(skillsDirectory);

        var expectedPrompt = "Use the project-specific assistant skills.";
        await File.WriteAllTextAsync(Path.Combine(skillsDirectory, "skills.md"), expectedPrompt);

        try
        {
            var request = new ChatRequest("Hello", "test-conversation");

            var mockClientProxy = new Mock<IClientProxy>();

            IReadOnlyList<ChatMessage>? capturedMessages = null;
            var mockChatClient = new Mock<IChatClient>();
            mockChatClient
                .Setup(c => c.GetStreamingResponseAsync(It.IsAny<IEnumerable<ChatMessage>>(), It.IsAny<ChatOptions>(), It.IsAny<CancellationToken>()))
                .Callback<IEnumerable<ChatMessage>, ChatOptions, CancellationToken>((messages, _, _) => capturedMessages = messages.ToList())
                .Returns(Array.Empty<ChatResponseUpdate>().ToAsyncEnumerable());

            var serviceProvider = new ServiceCollection()
                .AddSingleton<IChatClient>(mockChatClient.Object)
                .BuildServiceProvider();

            var mockEnvironment = new Mock<IWebHostEnvironment>();
            mockEnvironment.Setup(m => m.ContentRootPath).Returns(tempRoot);

            var mockAiOptions = new Mock<IOptions<AiOptions>>();
            mockAiOptions.Setup(o => o.Value).Returns(new AiOptions { AgentName = "assistant" });

            var service = new ChatResponseService(
                serviceProvider,
                mockEnvironment.Object,
                mockAiOptions.Object,
                new Mock<ILogger<ChatResponseService>>().Object);

            // Act
            await service.SendAnswerAsync(mockClientProxy.Object, request, CancellationToken.None);

            // Assert
            Assert.NotNull(capturedMessages);
            var systemMessage = Assert.Single(capturedMessages, message => message.Role == ChatRole.System);
            Assert.Equal(expectedPrompt, systemMessage.Text);
        }
        finally
        {
            Directory.Delete(tempRoot, recursive: true);
        }
    }
}
