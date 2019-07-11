using Amazon.Runtime;
using AWS.Logger.Log4net;
using Codesanook.Configuration.Models;
using log4net;
using log4net.Layout;
using log4net.Repository.Hierarchy;
using NHibernate.Transform;
using Orchard.Data;
using Orchard.Environment;
using System.Collections;
using System.Linq;

namespace Codesanook.CloudWatchLogs {
    //https://stackoverflow.com/a/9029457/1872200
    public class ShellEvent : IOrchardShellEvents {
        private readonly ITransactionManager transactionManager;

        public ShellEvent(ITransactionManager transactionManager) =>
            this.transactionManager = transactionManager;

        public void Activated() => AddCloudWatchLogAppender();

        public void Terminating() {
        }

        private void AddCloudWatchLogAppender() {
            var hierarchy = ((Hierarchy)LogManager.GetRepository());
            var rootLogger = hierarchy.Root;
            var appender = CreateCloudWatchLogAppender();
            rootLogger.AddAppender(appender);
        }

        private AWSAppender CreateCloudWatchLogAppender() {
            var patternLayout = new PatternLayout {
                ConversionPattern = "%utcdate{yyyy-MM-ddTHH:mm:ss.fffZ} [%-5level] %logger - %message%newline"
            };
            patternLayout.ActivateOptions();

            var session = transactionManager.GetSession();

            ModuleSettingPartRecord moduleSettingAlias = null; ;
            var setting = session.QueryOver(() => moduleSettingAlias)
                .SelectList(list => list
                    .Select(() => moduleSettingAlias.AwsAccessKey).WithAlias(() => moduleSettingAlias.AwsAccessKey)
                    .Select(() => moduleSettingAlias.AwsSecretKey).WithAlias(() => moduleSettingAlias.AwsSecretKey)
                )
                .TransformUsing(Transformers.AliasToEntityMap)
                .List<IDictionary>()
                .FirstOrDefault();

            var appender = new AWSAppender {
                Layout = patternLayout,
                Credentials = new BasicAWSCredentials(
                    setting[nameof(ModuleSettingPartRecord.AwsAccessKey)].ToString(),
                    setting[nameof(ModuleSettingPartRecord.AwsSecretKey)].ToString()
                ),
                LogGroup = "CodeSanook.CloudWatchLog",
                Region = "ap-southeast-1"
            };

            appender.ActivateOptions();
            return appender;
        }
    }
}