//using HIA.Repositories.Files;
//using HIA.Services.HosoManagement130;
//using Microsoft.Extensions.Logging;
//using Org.BouncyCastle.Utilities;
//using System;
//using System.Collections.Generic;
//using System.Linq;
//using System.Text;
//using System.Threading.Tasks;
//using System.Collections.Generic;
//using System.Threading.Tasks;
//using HIA.Core.Base;



//namespace UnitTestHIA
//{
//    public class ValidateTest
//    {
//        private readonly IHoSoManagementService130 _hoSoManagementService;
//        private readonly ILogger<ValidateTest> _logger;

//        public ValidateTest(IHoSoManagementService130 hoSoManagementService, ILogger<ValidateTest> logger, IFileRepository fileRepository)
//        {
//            _hoSoManagementService = hoSoManagementService;
//            _logger = logger;
//            _fileRepository = fileRepository;
//        }
//        public async Task RunValidationAsync()
//        {
//            var maCSKCB = "75009"; //Thong tin CSKCB can test Validate
//                                   // var lstIds = await _validateRepository.GetListIDToValidate(maCSKCB, 50);
//            Uti.HandleCronJobLog(_logger, $"Validate130: Lay {lstIds.Count()} ho so moi nhat tu viện ít hồ sơ nhất {maCSKCB} [{DateTime.Now:dd/MM hh:mm:ss}]", nameof(ValidateHoso));
//            DateTime dteStart = DateTime.Now;
//            // await ValidateXML(maCSKCB: maCSKCB, xmlIds: lstIds.ToArray());
//            TimeSpan execCounter = DateTime.Now - dteStart;
//            Uti.HandleCronJobLog(_logger, $"Validate130: Giam dinh xong Viện ít hồ sơ nhất {lstIds.Count()} file cua {maCSKCB}. Thoi gian thuc hien {execCounter.TotalMilliseconds}", nameof(ValidateHoso));
//        }

//    }
//}



