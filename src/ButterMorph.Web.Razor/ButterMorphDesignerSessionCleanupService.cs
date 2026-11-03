namespace ButterMorph.Web.Razor;

using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Options;

// Periodically releases abandoned in-memory workspaces without requiring another browser request.
internal sealed class ButterMorphDesignerSessionCleanupService : BackgroundService
{
    private readonly IButterMorphDesignerSessionManager sessions;
    private readonly ButterMorphDesignerSessionOptions options;

    public ButterMorphDesignerSessionCleanupService(
        IButterMorphDesignerSessionManager sessions,
        IOptions<ButterMorphRazorDesignerOptions> options)
    {
        this.sessions = sessions;
        this.options = options.Value.Sessions;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        TimeSpan interval = options.SweepInterval > TimeSpan.Zero ? options.SweepInterval : TimeSpan.FromMinutes(2);
        using PeriodicTimer timer = new(interval);

        while (await timer.WaitForNextTickAsync(stoppingToken))
        {
            sessions.SweepExpired();
        }
    }
}
