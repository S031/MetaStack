using System;
using System.Collections.Generic;
using System.Text;

namespace S031.MetaStack.Data
{
	[AttributeUsage(AttributeTargets.Class, Inherited = false, AllowMultiple = true)]
	public sealed class  DBRefAttribute : System.Attribute
    {
		public string DBProviderName { get; set; }
    }
}

