using System.Runtime.CompilerServices;
using Spectre.Console;

namespace Raiqub.LlmTools.FlowPair.Tests.Testing;

public static class ProgressTaskUnsafeExtension
{
    public static Accessor Unsafe(this ProgressTask progressTask) => new(progressTask);

    public sealed class Accessor(ProgressTask progressTask)
    {
        // Private fields
        [UnsafeAccessor(UnsafeAccessorKind.Field, Name = "_lock")]
        private static extern ref object GetLock(ProgressTask @this);

        [UnsafeAccessor(UnsafeAccessorKind.Field, Name = "_maxValue")]
        private static extern ref double GetMaxValue(ProgressTask @this);

        [UnsafeAccessor(UnsafeAccessorKind.Field, Name = "_description")]
        private static extern ref string GetDescription(ProgressTask @this);

        [UnsafeAccessor(UnsafeAccessorKind.Field, Name = "_value")]
        private static extern ref double GetValue(ProgressTask @this);

        // Properties with private setters
        [UnsafeAccessor(UnsafeAccessorKind.Method, Name = "get_StartTime")]
        private static extern DateTime? StartTimeGetter(ProgressTask @this);

        [UnsafeAccessor(UnsafeAccessorKind.Method, Name = "set_StartTime")]
        private static extern void StartTimeSetter(ProgressTask @this, DateTime? value);

        [UnsafeAccessor(UnsafeAccessorKind.Method, Name = "get_StopTime")]
        private static extern DateTime? StopTimeGetter(ProgressTask @this);

        [UnsafeAccessor(UnsafeAccessorKind.Method, Name = "set_StopTime")]
        private static extern void StopTimeSetter(ProgressTask @this, DateTime? value);

        [UnsafeAccessor(UnsafeAccessorKind.Method, Name = "get_State")]
        private static extern ProgressTaskState StateGetter(ProgressTask @this);

        // Private methods
        [UnsafeAccessor(UnsafeAccessorKind.Method, Name = "GetPercentage")]
        private static extern double GetPercentageMethod(ProgressTask @this);

        [UnsafeAccessor(UnsafeAccessorKind.Method, Name = "GetSpeed")]
        private static extern double? GetSpeedMethod(ProgressTask @this);

        [UnsafeAccessor(UnsafeAccessorKind.Method, Name = "GetElapsedTime")]
        private static extern TimeSpan? GetElapsedTimeMethod(ProgressTask @this);

        [UnsafeAccessor(UnsafeAccessorKind.Method, Name = "GetRemainingTime")]
        private static extern TimeSpan? GetRemainingTimeMethod(ProgressTask @this);

        // Public properties to access private fields
        public object Lock
        {
            get => GetLock(progressTask);
            set => GetLock(progressTask) = value;
        }

        public double MaxValue
        {
            get => GetMaxValue(progressTask);
            set => GetMaxValue(progressTask) = value;
        }

        public string Description
        {
            get => GetDescription(progressTask);
            set => GetDescription(progressTask) = value;
        }

        public double Value
        {
            get => GetValue(progressTask);
            set => GetValue(progressTask) = value;
        }

        // Properties with private setters
        public DateTime? StartTime
        {
            get => StartTimeGetter(progressTask);
            set => StartTimeSetter(progressTask, value);
        }

        public DateTime? StopTime
        {
            get => StopTimeGetter(progressTask);
            set => StopTimeSetter(progressTask, value);
        }

        public ProgressTaskState State => StateGetter(progressTask);

        // Public methods to access private methods
        public double GetPercentage() => GetPercentageMethod(progressTask);
        public double? GetSpeed() => GetSpeedMethod(progressTask);
        public TimeSpan? GetElapsedTime() => GetElapsedTimeMethod(progressTask);
        public TimeSpan? GetRemainingTime() => GetRemainingTimeMethod(progressTask);
    }
}
