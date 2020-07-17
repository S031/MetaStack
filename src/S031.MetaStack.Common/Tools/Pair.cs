using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace S031.MetaStack.Common
{	public readonly struct Pair<T>
	{
		public Pair(T xValue, T yValue)
		{
			X = xValue;
			Y = yValue;
		}
		public readonly T X;
		public readonly T Y;
	}
}
