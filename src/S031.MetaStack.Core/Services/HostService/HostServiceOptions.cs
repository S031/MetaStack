using System;
using System.Collections.Generic;
using System.Text;

namespace S031.MetaStack.Core.Services
{
    public class HostedServiceOptions
    {
		private string _assemblyName;
		private string _typeName;
		private readonly Dictionary<string, object> _parameters = new Dictionary<string, object>();

		private const int _delay = 60000;

		public HostedServiceOptions()
		{
			Delay = _delay;
		}
		public HostedServiceOptions(string assemblyName, string typeName)
		{
			_assemblyName = assemblyName;
			_typeName = typeName;
			Delay = _delay;
		}

		public string AssemblyName { get => _assemblyName; set { _assemblyName = value; } }

		public string TypeName { get => _typeName; set { _typeName = value; } }


		public string Name => _typeName.Replace('.', '_');

		public int Delay { get; set; }

		public string UserName { get; set; }
		public string Password { get; set; }
		public string LogName { get; set; }

		public Common.Logging.FileLogSettings LogSettings { get; set; }

		public Dictionary<string, object> Parameters => _parameters;

	}
}
