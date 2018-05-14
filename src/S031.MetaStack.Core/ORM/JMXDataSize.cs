using System;
using System.Collections.Generic;
using System.Text;

#if NETCOREAPP2_0
namespace S031.MetaStack.Core.ORM
#else
namespace S031.MetaStack.WinForms.ORM
#endif
{
	public struct JMXDataSize
	{
		public JMXDataSize(int size = 0, int scale = 0, int precision = 0)
		{
			Size = size;
			Scale = scale;
			Precision = precision;
		}
        public int Size { get; set; }
        public int Scale { get; set; }
        public int Precision { get; set; }
		public static bool operator ==(JMXDataSize s1, JMXDataSize s2)
		{
			return (s1.Size == s2.Size && s1.Scale == s2.Scale && s1.Precision == s2.Precision);
		}
		public static bool operator !=(JMXDataSize s1, JMXDataSize s2)
		{
			return (s1.Size != s2.Size || s1.Scale != s2.Scale || s1.Precision != s2.Precision);
		}
		public override bool Equals(object obj)
		{
			return this.GetHashCode() == obj.GetHashCode();
		}
		public override int GetHashCode()
		{
			return new { Size, Scale, Precision }.GetHashCode();
		}
		public bool IsEmpty() => Size == 0 && Scale == 0 && Precision == 0;
	}
}
