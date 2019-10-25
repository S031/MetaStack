using S031.MetaStack.Common;
using System;
using System.Collections.Generic;
using System.Text;

namespace S031.MetaStack.Core.Services
{
    public class HostedServiceOptions
    {
		private const int _delay = 60000;

		public HostedServiceOptions() { }

		public HostedServiceOptions(string assemblyName, string typeName)
		{
			AssemblyName = assemblyName;
			TypeName = typeName;
		}

		public string AssemblyName { get; set; }

		public string TypeName { get; set; }

		public string Name => TypeName.Replace('.', '_');

		public int Delay { get; set; } = _delay;

		public string UserName { get; set; }

		public string Password { get; set; }

		public string LogName { get; set; }

		public Common.Logging.FileLogSettings LogSettings { get; set; }

		public MapTable<string, object> Parameters { get; } = new MapTable<string, object>(4);

	}
}
