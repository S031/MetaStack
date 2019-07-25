using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace S031.MetaStack.Common
{
	public enum NumericTypesScope
	{
		All,
		Integral,
		Long,
		FloatingPoint
	}

	public static class vbo
	{
		public const string s_default = "&&&";

		public const string vbCrLf = "\r\n";
		public const string vbCr = "\r";
		public const string vbLf = "\n";
		public const string vbTab = "\t";
		public const string vbQuot = "\"";
		public const string vbApos = "'";
		public const string vbSep = ";";
		public const string vbCom = ",";

		public const char chrSep = ';';
		public const char chrQuot = '"';
		public const char chrCom = ',';
		public const char chrTab = '	';
		public const char chrDot = '.';

		public const string DateFormat = @"dd.MM.yyyy";
		public const string SortDateFormat = @"yyyy-MM-dd";
		public const string DBDateFormat = @"yyyyMMdd";
		public const string FullDateFormat = @"yyyy-MM-dd HH:mm:ss";
		public const string TimeFormat = @"HH:mm:ss";
		public const string CurrencyFormat = @"###,##0.00";
		public const string RateFormat = @"##0.0000";

		/// <summary>
		/// Золотое сечение
		/// </summary>
		public static readonly double GoldenRatio = (Math.Sqrt(5) + 1) / 2;

		/// <summary>
		/// Соотношение большего катета к меньшему, при соотношении гипотенузы к меньшему катету 1.618х1
		/// </summary>
		public static readonly double GoldenRatio2 = Math.Sqrt(GoldenRatio * GoldenRatio - 1);

		internal static readonly ReadOnlyCache<Type> AllNumericTypes =
			new ReadOnlyCache<Type>(
				typeof(int),
				typeof(long),
				typeof(decimal),
				typeof(char),
				typeof(sbyte),
				typeof(byte),
				typeof(short),
				typeof(ushort),
				typeof(uint),
				typeof(ulong),
				typeof(float),
				typeof(double));

		internal static readonly ReadOnlyCache<Type> IntegralTypes =
			new ReadOnlyCache<Type>(
				typeof(int),
				typeof(long),
				typeof(char),
				typeof(sbyte),
				typeof(byte),
				typeof(short),
				typeof(ushort),
				typeof(uint),
				typeof(ulong));

		internal static readonly ReadOnlyCache<Type> FloatingPointTypes =
			new ReadOnlyCache<Type>(
				typeof(decimal),
				typeof(float),
				typeof(double));

		public static DateTime Date()
			=> DateTime.Now.Date;

		public static bool IsEmpty(object value)
			=> value == null
				|| Convert.IsDBNull(value)
				|| value.Equals(value.GetType().GetDefaultValue());
    }
}
