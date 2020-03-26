using System;
using System.Collections.Generic;

namespace S031.MetaStack.ORM
{
	public class JMXName: List<string>
    {
		private const char _sep = '.';
		public JMXName(string fullName)
		{
			this.AddRange(fullName.Split(_sep));
		}

		public JMXName(params string[] names)
		{
			this.AddRange(names);
		}

		public string AreaName => this.Count > 0 ? this[0] : string.Empty;

#if NETCOREAPP
		public string ObjectName => this.Count > 1 ? string.Join(_sep, this) : string.Empty;
#else
		public string ObjectName => this.Count > 1 ? string.Join(_sep.ToString(), this) : string.Empty;
#endif

		public override string ToString()
		{
			return this.ObjectName;
		}

		public override bool Equals(object obj)
		{
			return this
				.ToString()
				.Equals(obj.ToString(), StringComparison.OrdinalIgnoreCase);
		}

		public override int GetHashCode()
		{
			return base.GetHashCode();
		}
	}
}
