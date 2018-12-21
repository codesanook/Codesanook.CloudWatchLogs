using Amazon.Runtime;
using AWS.Logger.Log4net;
using CodeSanook.Configuration.Models;
using log4net;
using log4net.Layout;
using log4net.Repository.Hierarchy;
using NHibernate.Transform;
using Orchard.Data;
using Orchard.Environment;
using Orchard.Logging;
using System;
using System.Collections;
using System.Linq;

namespace CodeSanook.CloudWatchLogs
{
    //https://stackoverflow.com/a/9029457/1872200
    public class ShellEvent : IOrchardShellEvents
    {
        private readonly ITransactionManager transactionManager;
        public ILogger Logger { get; set; }

        public ShellEvent(ITransactionManager transactionManager)
        {
            this.transactionManager = transactionManager;
            this.Logger = NullLogger.Instance;
        }

        public void Activated()
        {
            try
            {
                var hierarchy = ((Hierarchy)LogManager.GetRepository());
                var rootLogger = hierarchy.Root;
                var appender = CreateCloudWatchLogAppender();
                rootLogger.AddAppender(appender);

            }catch(Exception ex)
            {
                Logger.Error(ex, ex.Message);
            }
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

            var session = transactionManager.GetSession();
            var setting = session.QueryOver<ModuleSettingPartRecord>()
                .SelectList(list => list
                    .Select(p => p.AwsAccessKey)
                    .Select(p => p.AwsSecretKey)
                )
                .TransformUsing(Transformers.AliasToEntityMap)
                .List<IDictionary>()
                .FirstOrDefault();
                   

            var appender = new AWSAppender
            {
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