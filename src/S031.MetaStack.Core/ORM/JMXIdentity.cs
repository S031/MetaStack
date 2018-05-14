using System;
using System.Collections.Generic;
using System.Text;

#if NETCOREAPP2_0
namespace S031.MetaStack.Core.ORM
#else
namespace S031.MetaStack.WinForms.ORM
#endif
{
	public struct JMXIdentity
	{
		public JMXIdentity(bool isIdentity=false, int seed = 0, int increment=0)
		{
			IsIdentity = isIdentity;
			Seed = isIdentity && seed <= 0 ? 1 : seed;
			Increment = isIdentity && increment == 0 ? 1 : increment;
		}
        public bool IsIdentity { get; set; }
        public int Seed { get; set; }
        public int Increment { get; set; }
    }
}
