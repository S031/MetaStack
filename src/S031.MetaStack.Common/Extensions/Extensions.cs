using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Threading.Tasks;

namespace S031.MetaStack.Common
{
	public static class StringExtension
	{
		private static readonly Func<int, string> FastAllocateString =
					(Func<int, string>)typeof(string).GetMethods(BindingFlags.NonPublic | BindingFlags.Static)
						.First(x => x.Name == "FastAllocateString").CreateDelegate(typeof(Func<int, string>));

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static string Left(this string str, int lenght)
		{
			str.NullTest(nameof(str));
			return str.Substring(0, lenght > str.Length ? str.Length : lenght);
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static string Right(this string str, int lenght)
		{
			str.NullTest(nameof(str));
			return str.Substring(str.Length - (lenght > str.Length ? str.Length : lenght));
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static int ToIntOrDefault(this string str)
		{
			str.NullTest(nameof(str));
			return int.TryParse(str, out int result) ? result : 0;
		}
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static long ToLongOrDefault(this string str)
		{
			str.NullTest(nameof(str));
			return long.TryParse(str, out long result) ? result : 0;
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static double ToDoubleOrDefault(this string str)
		{
			str.NullTest(nameof(str));
			return double.TryParse(str, out double result) ? result : 0;
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static decimal ToDecimalOrDefault(this string str)
		{
			str.NullTest(nameof(str));
			return decimal.TryParse(str, out decimal result) ? result : 0;
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static DateTime ToDateOrDefault(this string str)
		{
			if (string.IsNullOrEmpty(str) || str == "0" || str.Left(4) == "0:00" || str.Left(5) == "00:00")
				return DateTime.MinValue;
			else if (DateTime.TryParse(str, out DateTime val))
				return val;
			else
				return DateTime.MinValue;
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static TimeSpan ToTimeSpan(this string str)
		{
			if (string.IsNullOrEmpty(str) || str == "0" || str.Left(4) == "0:00"
				|| str.Left(5) == "00:00")
				return TimeSpan.MinValue;
			else if (TimeSpan.TryParse(str, out TimeSpan val))
				return val;
			return TimeSpan.MinValue;
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static bool ToBoolOrDefault(this string str)
		{
			if (string.IsNullOrEmpty(str))
				return false;

			string source = str.ToLower();
			if (source == "1" || source == "true")
				return true;
			else if (source == "0" || source == "false")
				return false;
			else
				return bool.TryParse(str, out bool val) ? val : false;
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static T To<T>(this string str)
		{
			return (T)str.ToObjectOf(typeof(T));
		}

		public static object ToObjectOf(this string str, Type type)
		{
			if (str.IsEmpty())
				return type.GetDefaultValue();
			else if (type == typeof(bool))
				return str.ToBoolOrDefault();
			else if (type == typeof(DateTime))
				return str.ToDateOrDefault();
			else if (type == typeof(TimeSpan))
				return str.ToTimeSpan();
			else if (type == typeof(Guid))
				return new Guid(str);
			else if (type.IsNumeric(NumericTypesScope.Integral))
				return str.ToIntOrDefault();
			else if (type.IsNumeric(NumericTypesScope.Long))
				return (long)str.ToDoubleOrDefault();
			else if (type.IsNumeric(NumericTypesScope.FloatingPoint))
				return str.ToDecimalOrDefault();
			else
			{
				object ret;
				try { ret = System.Convert.ChangeType(str, type); }
				catch { ret = type.GetDefaultValue(); }
				return ret;
			}

		}
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static byte[] ToByteArray(this string str, System.Text.Encoding encoding = null)
		{
			str.NullTest(nameof(str));
			if (IsBase64String(str))
				return Convert.FromBase64String(str);
			else if (encoding != null)
				return encoding.GetBytes(str);
			return System.Text.Encoding.Default.GetBytes(str);
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static string TruncDub(this string str, string dublicatedChars)
		{
			str.NullTest(nameof(str));
			dublicatedChars.NullTest(nameof(dublicatedChars));
			string dub = dublicatedChars + dublicatedChars;
			for (; str.Contains(dub);) { str = str.Replace(dub, dublicatedChars); }
			return str;
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static string ToFormat(this string str, params object[] values) => string.Format(str, values);

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static float MatchScore(this string str, string stringForCompare)
		{
			str.NullTest(nameof(str));
			stringForCompare.NullTest(nameof(stringForCompare));
			return MatchsMaker.GetScore(str, stringForCompare);
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static bool IsEmpty(this string str) => string.IsNullOrEmpty(str);

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static bool Like(this string str, string template)
		{
			if (string.IsNullOrEmpty(str) || string.IsNullOrEmpty(template))
				return false;
			if (template.Contains(str))
				return true;
			return System.Text.RegularExpressions.Regex.IsMatch(str, template);
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static bool IsBase64String(this string str)
		{
			if (string.IsNullOrWhiteSpace(str))
				return false;

			return (str.Length % 4 == 0) &&
				System.Text.RegularExpressions.Regex.IsMatch(str.Right(64), @"^[a-zA-Z0-9\+/]*={0,3}$",
				System.Text.RegularExpressions.RegexOptions.None);

		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static string Wrap(this string str, int lenght)
		{
			if (string.IsNullOrEmpty(str))
				return str;

			var charCount = 0;
			var lines = str.Split(new[] { ' ', '\n' }, StringSplitOptions.RemoveEmptyEntries);
			return string.Join("\n", lines.GroupBy(w => (charCount += w.Length + 1) / (lenght + 2))
						.Select(g => string.Join(" ", g.ToArray()))
						.ToArray());
		}

		public static string SwitchItem(this string source, params string[] comparePairs)
		{
			string defaultResult = string.Empty;
			for (int i = 0; i < comparePairs.Length; i += 2)
			{
				if (source.Equals(comparePairs[i], StringComparison.CurrentCultureIgnoreCase))
					return comparePairs[i + 1];
				else if (comparePairs[i].Equals(vbo.s_default, StringComparison.CurrentCultureIgnoreCase))
					//Если указать "default", "" то вернет source. Если надо "", то не указавать "default"
					defaultResult = comparePairs[i + 1].IsEmpty() ? source : comparePairs[i + 1];
			}
			return defaultResult;
		}

		public static string GetToken(this string str, int index, string separator)
		{
			str.NullTest(nameof(str));
			separator.NullTest(nameof(separator));
			int start = 0;
			int len = separator.Length;
			for (int i = 0; i < index && start != -1; i++)
			{
				start = str.IndexOf(separator, start += len);
			}
			if (start < 0) return string.Empty;

			if (start > 0) start += len;
			int finish = str.IndexOf(separator, start);
			if (finish > 0)
				return str.Substring(start, finish - start);
			else
				return str.Substring(start);
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public unsafe static string Qt(this string source, char leftChar = '"', char rightChar = '"')
		{
			int len = source.Length;
			string destination = FastAllocateString(len + 2);
			fixed (char* pD = destination)
			fixed (char* pS = source)
			{
				*pD = leftChar;
				wstrcpy(pD + 1, pS, len);
				*(pD + 1 + len) = rightChar;
				return destination;
			}
		}

		//[MethodImpl(MethodImplOptions.AggressiveInlining)]
		//public unsafe static string Qt2(this string source, char leftChar = '"', char rightChar = '"')
		////=> string.Concat(leftChar, source, rightChar);
		//{
		//	int len = source.Length;
		//	string destination = string.Create<char>(len + 2, ' ', (buffer, value) =>
		//	   {
		//		   fixed (char* pD = buffer)
		//		   fixed (char* pS = source)
		//		   {
		//			   *pD = leftChar;
		//			   wstrcpy(pD + 1, pS, len);
		//			   *(pD + 1 + len) = rightChar;
		//		   }
		//	   });
		//	return destination;
		//}

		//[MethodImpl(MethodImplOptions.AggressiveInlining)]
		//public unsafe static string Qt3(this string source, char leftChar = '"', char rightChar = '"')
		//{
		//	int len = source.Length;
		//	char* destination = stackalloc char[len + 2];
		//	destination[0] = leftChar;
		//	fixed (char* pS = source)
		//		wstrcpy(destination + 1, pS, len);
		//	destination[len + 1] = leftChar;			
		//	return new string(destination);
		//}

		internal static unsafe void wstrcpy(char* dmem, char* smem, int charCount)
		{
			//uint len = ((uint)charCount);
			//for (int i = 0; i < len; i++)
			//	*(dmem + i) = *(smem + i);
			Buffer.MemoryCopy(smem, dmem, charCount * 2, charCount * 2);
		}

		public static string RemoveChar(this string source, char[] charsItem)
		{
			foreach (var c in charsItem)
				source = source.RemoveChar(c);
			return source;
		}

		public static string RemoveChar(this string source, char charItem)
		{
			int indexOfChar = source.IndexOf(charItem);
			if (indexOfChar < 0)
			{
				return source;
			}
			return RemoveChar(source.Remove(indexOfChar, 1), charItem);
		}

		public static String Between(this string source, String start, String end)
		{
			int i = source.IndexOf(start) + start.Length;
			int j = source.IndexOf(end, i);

			if (j != -1)
				return source.Substring(i, j - i);
			return string.Empty;
		}

		public static String Between(this string source, char start, char end)
		{
			int i = source.IndexOf(start) + 1;
			int j = source.IndexOf(end, i);
			if (j != -1)
				return source.Substring(i, j - i);
			return string.Empty;
		}
	}

	public static class NumericExtension
	{
		public static string Format(this int num, string format)
		{
			return string.Format("{0:" + format + "}", num);
		}

		public static string Format(this double num, string format)
		{
			return string.Format("{0:" + format + "}", num);
		}

		public static string Format(this decimal num, string format)
		{
			return string.Format("{0:" + format + "}", num);
		}
	}

	public static class CharExtension
	{
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static int ToIntOrDefault(this char c)
		{
			if (char.IsDigit(c))
				return (int)(c - '0');
			return 0;
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static bool ToBoolOrDefault(this char c) 
			=> c.ToIntOrDefault() != 0;

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static bool IsNumeric(this char c) 
			=> char.IsDigit(c);
	}

	public static class EnumerableExtension
	{
		public static int IndexOf<T>(this IEnumerable<T> source, T element, IEqualityComparer<T> comparer = null)
		{
			IList<T> list = (source as IList<T>);
			if (list == null)
			{
				if (source == null)
					return -1;
				if (element == null)
					return -1;

				int i = 0;
				comparer = comparer ?? EqualityComparer<T>.Default;
				foreach (T item in source)
				{
					if (comparer.Equals(item, element))
						return i;
					i++;
				}
				return -1;
			}
			else
			{
				return list.IndexOf(element);
			}
		}
	}

	public static class DateTimeExtension
	{
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static string ToDBFormat(this DateTime srcDate)
			=> srcDate.ToString(vbo.DBDateFormat);

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static string ToFormat(this DateTime srcDate)
			=> srcDate.ToString(vbo.DateFormat);

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static string ToFullFormat(this DateTime srcDate)
			=> srcDate.ToString(vbo.FullDateFormat);

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static bool IsEmpty(this DateTime date)
			=> date == DateTime.MinValue;

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static DateTime FirstDayOfMonth(this DateTime value)
			=> value.Date.AddDays(1 - value.Day);

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static DateTime LastDayOfMonth(this DateTime value)
			=> value.Date.AddDays((-1) * value.Day).AddMonths(1);
	}

	public static class ByteArrayExtension
	{
		public static string ToBASE64String(this byte[] data)
		{
			data.NullTest(nameof(data));
			return Convert.ToBase64String(data);
		}
	}

	public static class IDictionaryExtension
	{
		public static object GetValue(this IDictionary<string, object> d, string key, object defaultValue = null)
		{
			if (d != null && d.TryGetValue(key, out object result))
				return result;
			return defaultValue;
		}
		public static T GetValue<T>(this IDictionary<string, object> d, string key, object defaultValue = null)
		{
			if (d != null && d.TryGetValue(key, out object result))
				return result.CastOf<T>();
			return (T)defaultValue;
		}
	}

	public static class KeyValuePairExtension
	{
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static bool IsEmpty<T, TU>(this KeyValuePair<T, TU> pair) => pair.Equals(new KeyValuePair<T, TU>());
	}

	public static class ObjectExtensions
	{
		public static object CastOf(this object source, Type type)
		{
			if (source == null || Convert.IsDBNull(source))
				return type.GetDefaultValue();

			Type t = source.GetType();
			if (t == type)
				return source;
			else if (t.IsPrimitive && type.IsPrimitive)
				return Convert.ChangeType(source, type);
			else if (type.IsAssignableFrom(t))
				return Convert.ChangeType(source, type);
			else 
				return source.ToString().ToObjectOf(type);
		}

		public static T CastOf<T>(this object source) => (T)CastOf(source, typeof(T));

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static void NullTest(this object value, string valueName) =>
			NullTest(value, valueName, (v, n) => { throw new ArgumentNullException(n); });
		public static void NullTest(this object value, string valueName, Action<object, string> action)
		{
			if (value == null)	action(value, valueName);
		}

#if NETCOREAPP
		public static U Match<T, U>(this T val, params (Func<T, bool> qualifier, Func<T, U> func)[] matches)
		{
			U ret = default;

			foreach (var (qualifier, func) in matches)
			{
				if (qualifier(val))
				{
					ret = func(val);
					break;
				}
			}

			return ret;
		}
		public async static void MatchAsync<T>(this T val, params (Func<T, bool> qualifier, Action<T> action)[] matches)
		{
			foreach (var (qualifier, action) in matches)
			{
				if (await Task.Run(() => qualifier(val)))
				{
					await Task.Run(() => action(val));
					break;
				}
			}
		}
#endif
	}
}
