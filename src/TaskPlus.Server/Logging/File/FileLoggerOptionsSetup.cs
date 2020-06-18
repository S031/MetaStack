using Microsoft.Extensions.Logging.Configuration;
using Microsoft.Extensions.Options;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace TaskPlus.Server.Logging.File
{
    internal class FileLoggerOptionsSetup : ConfigureFromConfigurationOptions<FileLoggerOptions>
    {
        public FileLoggerOptionsSetup(ILoggerProviderConfiguration<FileLoggerProvider>
                                      providerConfiguration)
            : base(providerConfiguration.Configuration)
        {
        }
    }
}
