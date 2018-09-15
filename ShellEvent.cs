using Amazon.Runtime;
using AWS.Logger.Log4net;
using CodeSanook.Configuration.Models;
using log4net;
using log4net.Layout;
using log4net.Repository.Hierarchy;
using Orchard.ContentManagement;
using Orchard.Environment;
using Orchard.Settings;

namespace CodeSanook.CloudWatchLogs
{
    //https://stackoverflow.com/a/9029457/1872200
    public class ShellEvent : IOrchardShellEvents
    {
        private readonly ISiteService siteService;
        public ShellEvent(ISiteService siteService)
        {
            this.siteService = siteService;
        }

        public void Activated()
        {
            var hierarchy = ((Hierarchy)LogManager.GetRepository());
            var rootLogger = hierarchy.Root;
            var appender = CreateCloudWatchLogAppender();
            rootLogger.AddAppender(appender);
        }

        public void Terminating()
        {
        }

        private AWSAppender CreateCloudWatchLogAppender()
        {
            var patternLayout = new PatternLayout
            {
                ConversionPattern = "%utcdate{yyyy-MM-ddTHH:mm:ss.fffZ} [%-5level] %logger - %message%newline"
            };
            patternLayout.ActivateOptions();

            var setting = this.siteService.GetSiteSettings().As<ModuleSettingPart>();
            var appender = new AWSAppender
            {
                Layout = patternLayout,
                Credentials = new BasicAWSCredentials(setting.AwsAccessKey, setting.AwsSecretKey),
                LogGroup = "CodeSanook.CloudWatchLog",
                Region = "ap-southeast-1"
            };

            appender.ActivateOptions();
            return appender;
        }

    }
}