using FluentAssertions;
using NSubstitute;
using Raiqub.LlmTools.FlowPair.Support.Console;
using Raiqub.LlmTools.FlowPair.Tests.Testing;
using Spectre.Console;
using Spectre.Console.Rendering;
using Spectre.Console.Testing;

namespace Raiqub.LlmTools.FlowPair.Tests.Support.Console;

public sealed class LongRemainingTimeColumnTests : IDisposable
{
    private readonly TimeProvider _timeProvider = Substitute.For<TimeProvider>();
    private readonly TestConsole _testConsole = new();
    private readonly RenderOptions _renderOptions;

    public LongRemainingTimeColumnTests()
    {
        _renderOptions = RenderOptions.Create(_testConsole);
        _timeProvider.LocalTimeZone.Returns(TimeZoneInfo.Utc);
    }

    public void Dispose() => _testConsole.Dispose();

    [Fact]
    public void ConstructorWithNullTimeProviderShouldUseSystemTimeProvider()
    {
        var column = new LongRemainingTimeColumn();
        column.Should().NotBeNull();
        var result = column.Render(
            options: _renderOptions,
            task: new ProgressTask(1, "Test", 100),
            deltaTime: TimeSpan.Zero);

        result.GetText().Should().Be("--:--:--");
    }

    [Fact]
    public void PropertiesShouldHaveCorrectDefaultValues()
    {
        var column = new LongRemainingTimeColumn();

        column.Style.Should().Be((Style)Color.Blue);
        column.GetColumnWidth(_renderOptions).Should().Be(8);
        var noWrap = column.GetType()
            .GetProperty(
                "NoWrap",
                System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance)
            ?.GetValue(column);
        noWrap.Should().Be(true);
    }

    [Theory]
    [InlineData(null)]
    [InlineData(0)]
    [InlineData(100)] // Task finished
    public void RenderSpecialCasesShouldReturnExpectedOutput(int? value)
    {
        var startTime = new DateTime(2023, 1, 1, 0, 0, 0, DateTimeKind.Utc);
        _timeProvider.GetUtcNow().Returns(new DateTimeOffset(startTime.AddMinutes(10)));

        var column = new LongRemainingTimeColumn(_timeProvider);
        var task = new ProgressTask(1, "Test", 100, autoStart: false) { Value = value ?? 0 };
        task.Unsafe().StartTime = startTime;

        var result = column.Render(_renderOptions, task, TimeSpan.Zero);

        if (value == 100)
            result.GetText().Should().Be("00:00:00");
        else
            result.GetText().Should().Be("--:--:--");
    }

    [Theory]
    [InlineData(25, 5, "00:15:00")]
    [InlineData(50, 10, "00:10:00")]
    [InlineData(75, 15, "00:05:00")]
    public void RenderTaskInProgressShouldReturnAccurateFormattedTime(
        int value,
        int elapsedMinutes,
        string expected)
    {
        var startTime = new DateTime(2023, 1, 1, 0, 0, 0, DateTimeKind.Utc);
        var currentTime = startTime.AddMinutes(elapsedMinutes);
        _timeProvider.GetUtcNow().Returns(new DateTimeOffset(currentTime));

        var column = new LongRemainingTimeColumn(_timeProvider);
        var task = new ProgressTask(1, "Test", 100) { Value = value };
        task.Unsafe().StartTime = startTime;

        var result = column.Render(_renderOptions, task, TimeSpan.Zero);

        result.GetText().Should().Be(expected);
    }

    [Fact]
    public void RenderWhenRemainingTimeExceeds99HoursShouldReturnAsterisks()
    {
        var startTime = new DateTime(2023, 1, 1, 0, 0, 0, DateTimeKind.Utc);
        var currentTime = startTime.AddHours(1);
        _timeProvider.GetUtcNow().Returns(new DateTimeOffset(currentTime));

        var column = new LongRemainingTimeColumn(_timeProvider);
        var task = new ProgressTask(1, "Test", 10000) { Value = 1 };
        task.Unsafe().StartTime = startTime;

        var result = column.Render(_renderOptions, task, TimeSpan.Zero);

        result.GetText().Should().Be("**:**:**");
    }

    [Fact]
    public void RenderEstimationBehaviorShouldBeCorrect()
    {
        var startTime = new DateTime(2023, 1, 1, 0, 0, 0, DateTimeKind.Utc);
        var currentTime = startTime.AddMinutes(5);
        _timeProvider.GetUtcNow().Returns(
            new DateTimeOffset(currentTime),
            new DateTimeOffset(currentTime),
            new DateTimeOffset(currentTime.AddMinutes(20)),
            new DateTimeOffset(currentTime.AddMinutes(200)));

        var column = new LongRemainingTimeColumn(_timeProvider);
        var task = new ProgressTask(1, "Test", 100) { Value = 25 };
        task.Unsafe().StartTime = startTime;

        // First render to set the estimated finish time
        var firstResult = column.Render(_renderOptions, task, TimeSpan.Zero);

        // Second render with unchanged task value
        var secondResult = column.Render(_renderOptions, task, TimeSpan.Zero);
        secondResult.GetText().Should().Be(firstResult.GetText());

        // Third render after the estimated finish time
        var thirdResult = column.Render(_renderOptions, task, TimeSpan.Zero);
        thirdResult.GetText().Should().Be("--:--:--");

        // Update task value and render again
        task.Value = 50;
        var fourthResult = column.Render(_renderOptions, task, TimeSpan.Zero);
        fourthResult.GetText().Should().NotBe(secondResult.GetText());
    }

    [Fact]
    public void RenderWithCustomAndNullStylesShouldUseCorrectStyle()
    {
        var startTime = new DateTime(2023, 1, 1, 0, 0, 0, DateTimeKind.Utc);
        var currentTime = startTime.AddMinutes(5);
        _timeProvider.GetUtcNow().Returns(new DateTimeOffset(currentTime));

        var task = new ProgressTask(1, "Test", 100) { Value = 25 };
        task.Unsafe().StartTime = startTime;

        // Custom style
        var customStyle = new Style(foreground: Color.Red);
        var columnWithCustomStyle = new LongRemainingTimeColumn(_timeProvider) { Style = customStyle };
        var customStyleResult = columnWithCustomStyle.Render(_renderOptions, task, TimeSpan.Zero);
        customStyleResult.Should().NotBeNull();
        customStyleResult.Render(_renderOptions, int.MaxValue).Should()
            .AllSatisfy(s => s.Style.Should().Be(customStyle));

        // Null style
        var columnWithNullStyle = new LongRemainingTimeColumn(_timeProvider) { Style = null };
        var nullStyleResult = columnWithNullStyle.Render(_renderOptions, task, TimeSpan.Zero);
        nullStyleResult.Should().NotBeNull();
        nullStyleResult.Render(_renderOptions, int.MaxValue).Should()
            .AllSatisfy(s => s.Style.Should().Be(Style.Plain));
    }
}
