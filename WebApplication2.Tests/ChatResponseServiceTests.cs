using Microsoft.AspNetCore.SignalR;
using Microsoft.Extensions.AI;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Moq;
using WebApplication2.Contracts;
using WebApplication2.Hubs;
using WebApplication2.Services;
using Xunit;

namespace WebApplication2.Tests;

public class ChatResponseServiceTests
{
    [Fact]
    public async Task SendAnswerAsync_StreamsResponsesToClient()
    {
        // Arrange
        var connectionId = "test-connection";
        var conversationId = "test-conversation";
        var message = "Hello";
        var request = new ChatRequest(connectionId, message, conversationId);

        var mockClients = new Mock<IHubClients>();
        var mockClientProxy = new Mock<ISingleClientProxy>();
        mockClients.Setup(c => c.Client(connectionId)).Returns(mockClientProxy.Object);

        var mockHubContext = new Mock<IHubContext<ChatHub>>();
        mockHubContext.Setup(c => c.Clients).Returns(mockClients.Object);

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
        var service = new ChatResponseService(mockHubContext.Object, serviceProvider, logger);

        // Act
        await service.SendAnswerAsync(request, CancellationToken.None);

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
}
