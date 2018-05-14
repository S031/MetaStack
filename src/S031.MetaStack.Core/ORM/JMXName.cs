using S031.MetaStack.Common;
using System;
using System.Collections.Generic;
using System.Text;

#if NETCOREAPP2_0
namespace S031.MetaStack.Core.ORM
#else
namespace S031.MetaStack.WinForms.ORM
#endif
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

		public string ObjectName => this.Count > 1 ? string.Join(_sep, this) : string.Empty;

		public override string ToString()
		{
			return this.ObjectName;
		}

		public override bool Equals(object obj)
		{
			return this.ToString().Equals(obj.ToString(), StringComparison.CurrentCultureIgnoreCase);
		}

		public override int GetHashCode()
		{
			return base.GetHashCode();
		}
	}
}
