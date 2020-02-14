using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace S031.MetaStack.Common
{	public readonly ref struct Pair<T>
	{
		public Pair(T xValue, T yValue)
		{
			x = xValue;
			y = yValue;
		}
		public T x { get; }
		public T y { get; }
	}
}
