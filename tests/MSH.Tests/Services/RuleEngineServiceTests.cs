using System.Text.Json;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Moq;
using MSH.Infrastructure.Entities;
using MSH.Infrastructure.Services;
using Xunit;

namespace MSH.Tests.Services;

public class RuleEngineServiceTests
{
    private readonly Mock<ILogger<RuleEngineService>> _loggerMock;
    private readonly RuleEngineService _ruleEngineService;

    public RuleEngineServiceTests()
    {
        _loggerMock = new Mock<ILogger<RuleEngineService>>();
        _ruleEngineService = new RuleEngineService(_loggerMock.Object);
    }

    [Fact]
    public async Task EvaluateRuleAsync_WhenConditionIsTrue_ExecutesAction()
    {
        // Arrange
        var rule = new Rule
        {
            Name = "Test Rule",
            Condition = JsonDocument.Parse("\"true\""),
            Action = JsonDocument.Parse("\"test action\"")
        };

        // Act
        await _ruleEngineService.EvaluateRuleAsync(rule);

        // Assert
        _loggerMock.Verify(
            x => x.Log(
                LogLevel.Information,
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((v, t) => v.ToString().Contains("Action executed: test action")),
                It.IsAny<Exception>(),
                It.IsAny<Func<It.IsAnyType, Exception, string>>()),
            Times.Once);
    }

    [Fact]
    public async Task TriggerRuleAsync_WhenTriggerTypeIsTime_LogsTimeBasedTrigger()
    {
        // Arrange
        var trigger = new RuleTrigger
        {
            RuleId = 1,
            TriggerType = "time",
            TriggerConfig = JsonDocument.Parse("\"12:00\"")
        };

        // Act
        await _ruleEngineService.TriggerRuleAsync(trigger);

        // Assert
        _loggerMock.Verify(
            x => x.Log(
                LogLevel.Information,
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((v, t) => v.ToString().Contains("Time-based trigger activated for rule: 1")),
                It.IsAny<Exception>(),
                It.IsAny<Func<It.IsAnyType, Exception, string>>()),
            Times.Once);
    }

    [Fact]
    public async Task ExecuteActionAsync_LogsActionExecution()
    {
        // Arrange
        var rule = new Rule
        {
            Name = "Test Rule",
            Action = JsonDocument.Parse("\"test action\"")
        };

        // Act
        await _ruleEngineService.ExecuteActionAsync(rule);

        // Assert
        _loggerMock.Verify(
            x => x.Log(
                LogLevel.Information,
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((v, t) => v.ToString().Contains("Action executed: test action")),
                It.IsAny<Exception>(),
                It.IsAny<Func<It.IsAnyType, Exception, string>>()),
            Times.Once);
    }
} 