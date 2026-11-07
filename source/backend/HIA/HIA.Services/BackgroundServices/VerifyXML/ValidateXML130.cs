using HIA.Core.Base;
using HIA.Services.HosoManagement130;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace HIA.Services.BackgroundServices.VerifyXML;

public class ValidateXML130 : ValidateXMLService130
{
    private readonly IHoSoManagementService130 _hoSoManagementService;
    private readonly ILogger<ValidateXML130> _logger;

    public ValidateXML130(
        IServiceProvider serviceProvider,
        IScheduleConfig<ValidateXML130> config,
        ILogger<ValidateXML130> logger)
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
        Uti.HandleCronJobLog(_logger, $"working ({DateTime.Now:hh:mm:ss}).", nameof(ValidateXML130));

        _ = _hoSoManagementService.ValidateHosoVIP2();
        return Task.CompletedTask;
    }

    public override Task StopAsync(CancellationToken cancellationToken)
    {
        Uti.HandleCronJobLog(_logger, $"stopped.", nameof(ValidateXML130));
        return base.StopAsync(cancellationToken);
    }
}
