using System;
using System.Collections.Generic;
using System.Text;

namespace S031.MetaStack.Common.Logging
{
	public static class FileLogExtensions
	{
		public static string ToString(this LogLevels level)
		{
			return level.ToString("G");
		}
	}
}
