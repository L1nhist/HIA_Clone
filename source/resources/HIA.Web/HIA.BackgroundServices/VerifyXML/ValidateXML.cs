using HIA.Core.Base;
//Linhist removed 29/03/25
//using HIA.Services.HosoManagement;
using HIA.Services.HosoManagement130;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace HIA.BackgroundServices.VerifyXML
{
    public class ValidateXML : ValidateXMLService
    {
        //private readonly IHoSoManagementService _hoSoManagementService;
        private readonly IHoSoManagementService130 _hoSoManagementService;
        private readonly ILogger<ValidateXML> _logger;

        public ValidateXML(
            IServiceProvider serviceProvider,
            IScheduleConfig<ValidateXML> config,
            ILogger<ValidateXML> logger)
            : base(config.CronExpression, config.TimeZoneInfo)
        {
            _hoSoManagementService = serviceProvider.CreateScope().ServiceProvider.GetRequiredService<IHoSoManagementService130>(); ;
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

            //_ = _hoSoManagementService.ValidateNewXML();
            //_ = _hoSoManagementService.ValidateHosoFrom4210();
            _ = _hoSoManagementService.ValidateHosoVIP1();
            return Task.CompletedTask;
            //await Task.FromCanceled(cancellationToken);
        }

        public override Task StopAsync(CancellationToken cancellationToken)
        {
            Uti.HandleCronJobLog(_logger, $"stopped.", nameof(ValidateXML));
            return base.StopAsync(cancellationToken);
        }
    }
}