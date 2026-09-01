using XRatio.Desktop;

namespace XRatio.Desktop.Tests;

public sealed class ProgramPerformanceTests
{
    [Fact]
    public async Task ActivationWait_BlocksUntilTheNamedEventIsSignaled()
    {
        using var activationEvent = new EventWaitHandle(
            initialState: false,
            EventResetMode.AutoReset);
        using var cancellation = new CancellationTokenSource();
        var wait = Task.Run(() => Program.WaitForActivation(activationEvent, cancellation.Token));

        try
        {
            await Task.Delay(TimeSpan.FromMilliseconds(50));
            Assert.False(wait.IsCompleted);

            activationEvent.Set();
            Assert.True(await wait.WaitAsync(TimeSpan.FromSeconds(1)));
        }
        finally
        {
            cancellation.Cancel();
            activationEvent.Set();
            await wait.WaitAsync(TimeSpan.FromSeconds(1));
        }
    }

    [Fact]
    public void ActivationWait_StopsImmediatelyWhenAlreadyCanceled()
    {
        using var activationEvent = new EventWaitHandle(
            initialState: false,
            EventResetMode.AutoReset);
        using var cancellation = new CancellationTokenSource();
        cancellation.Cancel();

        Assert.False(Program.WaitForActivation(activationEvent, cancellation.Token));
    }
}
