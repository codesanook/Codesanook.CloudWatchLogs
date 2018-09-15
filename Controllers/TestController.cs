using CodeSanook.Common.Modules;
using CodeSanook.Common.Web;
using Orchard.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace CodeSanook.CloudWatchLogs.Controllers
{
    public class TestController : Controller
    {
        public ILogger Logger { get; set; }

        public TestController()
        {
            this.Logger = NullLogger.Instance;
        }

        public ActionResult Index()
        {
            var moduleName = ModuleHelper.GetModuleName<TestController>();
            Logger.Warning($"Hello world from {moduleName}");
            Logger.Error($"Hello world from {moduleName}");
            return Content("log message sent to CloudWatch Logs");
        }
    }
}