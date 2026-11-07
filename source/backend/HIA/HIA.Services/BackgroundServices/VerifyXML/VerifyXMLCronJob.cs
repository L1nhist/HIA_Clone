using HIA.Core.Base;
using HIA.Services.HosoManagement130;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace HIA.Services.BackgroundServices.VerifyXML;

public class VerifyXMLCronJob : CronJobService
{
    private readonly IHoSoManagementService130 _hoSoManagementService;
    private readonly ILogger<VerifyXMLCronJob> _logger;

    public VerifyXMLCronJob(
        IServiceProvider serviceProvider,
        IScheduleConfig<VerifyXMLCronJob> config,
        ILogger<VerifyXMLCronJob> logger)
        : base(config.CronExpression, config.TimeZoneInfo)
    {
        _hoSoManagementService = serviceProvider.CreateScope().ServiceProvider.GetRequiredService<IHoSoManagementService130>();
        _logger = logger;
    }

    public override Task StartAsync(CancellationToken cancellationToken)
    {
        Uti.HandleCronJobLog(_logger, $"starts.", nameof(VerifyXMLCronJob));
        return base.StartAsync(cancellationToken);
    }

    public override Task DoWork(CancellationToken cancellationToken)
    {
        Uti.HandleCronJobLog(_logger, $"working ({DateTime.Now:hh:mm:ss}).", nameof(VerifyXMLCronJob));

        _ = _hoSoManagementService.ValidateHosoVIP3();

        return Task.CompletedTask;
    }

    public override Task StopAsync(CancellationToken cancellationToken)
    {
        Uti.HandleCronJobLog(_logger, $"stopped.", nameof(VerifyXMLCronJob));
        return base.StopAsync(cancellationToken);
    }
}
