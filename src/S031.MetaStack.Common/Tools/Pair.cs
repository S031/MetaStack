using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace S031.MetaStack.Common
{
	public readonly struct Pair<T>
	{
		public Pair(T xValue, T yValue)
		{
			X = xValue;
			Y = yValue;
		}
		public readonly T X;
		public readonly T Y;

		public override int GetHashCode()
			=> (X, Y).GetHashCode();

		public override bool Equals(object obj)
			=> obj is Pair<T> p && GetHashCode() == p.GetHashCode();
		public static bool operator ==(Pair<T> f1, Pair<T> f2) => f1.Equals(f2);
		public static bool operator !=(Pair<T> f1, Pair<T> f2) => !f1.Equals(f2);
	}
}
