using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;

namespace S031.MetaStack.Logging
{
	public struct CallerInfo
	{
		public readonly string Message;
		public readonly string FilePath;
		public readonly string MemberName;
		public readonly int LineNumber;

		public CallerInfo(
			string message,
			[CallerFilePath] string filePath = "",
			[CallerMemberName] string memberName = "",
			[CallerLineNumber] int lineNumber = 0)
		{
			Message = message;
			FilePath = filePath;
			MemberName = memberName;
			LineNumber = lineNumber;
		}

		public override string ToString()
			=> $@"{FilePath}\{MemberName}({LineNumber})\t{Message}";
	}
}
