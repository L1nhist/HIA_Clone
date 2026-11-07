using HIA.Core.Base;
using HIA.Services.HosoManagement130;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace HIA.Services.BackgroundServices.VerifyXML;

public class ValidateXML : ValidateXMLService
{
    private readonly IHoSoManagementService130 _hoSoManagementService;
    private readonly ILogger<ValidateXML> _logger;

    public ValidateXML(
        IServiceProvider serviceProvider,
        IScheduleConfig<ValidateXML> config,
        ILogger<ValidateXML> logger)
        : base(config.CronExpression, config.TimeZoneInfo)
    {
        _hoSoManagementService = serviceProvider.CreateScope().ServiceProvider.GetRequiredService<IHoSoManagementService130>();
        _logger = logger;
    }

    public override Task StartAsync(CancellationToken cancellationToken)
    {
        Uti.HandleCronJobLog(_logger, $"starts.", nameof(ValidateXML));
        return base.StartAsync(cancellationToken);
    }

    public override Task DoWork(CancellationToken cancellationToken)
    {
        Uti.HandleCronJobLog(_logger, $"working ({DateTime.Now:hh:mm:ss}).", nameof(ValidateXML));

        _ = _hoSoManagementService.ValidateHosoVIP1();
        return Task.CompletedTask;
    }

    public override Task StopAsync(CancellationToken cancellationToken)
    {
        Uti.HandleCronJobLog(_logger, $"stopped.", nameof(ValidateXML));
        return base.StopAsync(cancellationToken);
    }
}
