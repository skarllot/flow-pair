using Spectre.Console;
using Spectre.Console.Rendering;

namespace Raiqub.LlmTools.FlowPair.Support.Console;

public sealed class LongRemainingTimeColumn : ProgressColumn
{
    private readonly TimeProvider _timeProvider;
    private double _lastStep;
    private DateTime? _estimatedFinishTime;

    public LongRemainingTimeColumn(TimeProvider? timeProvider = null)
    {
        _timeProvider = timeProvider ?? TimeProvider.System;
    }

    /// <inheritdoc/>
    protected override bool NoWrap => true;

    /// <summary>
    /// Gets or sets the style of the remaining time text.
    /// </summary>
    public Style? Style { get; set; } = Color.Blue;

    /// <inheritdoc/>
    public override IRenderable Render(RenderOptions options, ProgressTask task, TimeSpan deltaTime)
    {
        var now = _timeProvider.GetLocalNow();
        CollectSample(task, now);

        var remaining = GetRemainingTime(task, now);
        if (remaining == null)
        {
            return new Markup("--:--:--");
        }

        if (remaining.Value.TotalHours > 99)
        {
            return new Markup("**:**:**");
        }

        return new Text($"{remaining.Value:hh\\:mm\\:ss}", Style ?? Style.Plain);
    }

    /// <inheritdoc/>
    public override int? GetColumnWidth(RenderOptions options) => 8;

    private void CollectSample(ProgressTask task, DateTimeOffset now)
    {
        if (task.StartTime == null || task.Value.Equals(_lastStep))
        {
            return;
        }

        var currTime = now.DateTime;
        var timeElapsed = currTime - task.StartTime.Value;
        var rateSecs = timeElapsed.TotalSeconds / task.Value;
        var remainingSteps = task.MaxValue - task.Value;
        var remainingTime = remainingSteps * rateSecs;

        _lastStep = task.Value;
        _estimatedFinishTime = currTime + TimeSpan.FromSeconds(remainingTime);
    }

    private TimeSpan? GetRemainingTime(ProgressTask task, DateTimeOffset now)
    {
        if (task.IsFinished)
        {
            return TimeSpan.Zero;
        }

        if (_estimatedFinishTime == null)
        {
            return null;
        }

        var currTime = now.DateTime;
        return currTime < _estimatedFinishTime.Value
            ? _estimatedFinishTime.Value - currTime
            : null;
    }
}
