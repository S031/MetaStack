using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using System.Text;

namespace S031.MetaStack.Common
{
	public static class SpanCharExtensions
	{
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static ReadOnlySpan<char> Left(this ReadOnlySpan<char> str, int lenght)
		{
			return str.Slice(0, lenght > str.Length ? str.Length : lenght);
		}

		public static ReadOnlySpan<char> GetToken(this ReadOnlySpan<char> str, int index, ReadOnlySpan<char> separator)
		{
			int start = 0;
			int len = separator.Length;

			for (int i = 0; i < index && start != -1; i++)
			{
				int pos = str.Slice(start).IndexOf(separator);
				if (pos < 0)
					return ReadOnlySpan<char>.Empty;
				else
					start += (len + pos);
			}

			int finish = str.Slice(start).IndexOf(separator);
			if (finish > 0)
				return str.Slice(start, finish);
			else
				return str.Slice(start);
		}
	}
}
