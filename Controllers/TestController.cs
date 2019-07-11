using Codesanook.Common.Modules;
using Orchard.Logging;
using System.Web.Mvc;

namespace Codesanook.CloudWatchLogs.Controllers {
    public class TestController : Controller
    {
        public ILogger Logger { get; set; }

        public TestController() => Logger = NullLogger.Instance;

        public ActionResult Index()
        {
            var moduleName = ModuleHelper.GetModuleName<TestController>();
            Logger.Information($"Hello world from {moduleName}");
            Logger.Warning($"Hello world from {moduleName}");
            Logger.Error($"Hello world from {moduleName}");
            return Content("log message sent to CloudWatch Logs");
        }
    }
}