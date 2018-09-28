using System;
using System.Collections.Generic;
using System.Text;

namespace S031.MetaStack.Core.ORM
{
	[AttributeUsage(AttributeTargets.Class, Inherited = false, AllowMultiple = true)]
	public sealed class  SchemaDBSyncAttribute : System.Attribute
    {
		public string DBProviderName { get; set; }
    }
}

