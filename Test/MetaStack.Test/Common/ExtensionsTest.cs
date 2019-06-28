using Xunit;
using S031.MetaStack.Common;
using Xunit.Abstractions;
using System;
using System.Linq;
using S031.MetaStack.Common.Logging;
using System.Collections.Generic;
using pair = System.Collections.Generic.KeyValuePair<System.Type, System.ValueType>;
using System.Collections;

namespace MetaStack.Test.Common
{
	public class ExtensionsTest
	{
		public ExtensionsTest()
		{
			FileLogSettings.Default.Filter = (s, i) => i >= LogLevels.Debug;
		}
		[Fact]
		void NullExceptiobnTest()
		{
			using (FileLog l = new FileLog("nullEceptionTest", new FileLogSettings() { DateFolderMask = "yyyy-MM-dd" }))
			{
				object value = null;
				try
				{
					value.NullTest(nameof(value));
				}
				catch (ArgumentException e)
				{
					l.Write(LogLevels.Error, e.Message);
				}
				value = "test object";
				value.NullTest(nameof(value));
				l.Debug(value.ToString());
			}
		}

		[Fact]
		void StringExtensionsTest()
		{
			Assert.True("1234" == "12345677890".Left(4));
			Assert.True("7890" == "12345677890".Right(4));
			Assert.True(12345 == "12345".ToIntOrDefault());

			double d = 98765432.9876543;
			Assert.True(d.ToString().ToDoubleOrDefault() == d);
			Assert.True(d.ToString().ToDecimalOrDefault() == (decimal)d);

			DateTime data = DateTime.Now;
			DateTime data1 = data.ToString("yyyy-MM-dd HH:mm:ss.fffff").ToDateOrDefault();
			Assert.True(data.Year == data.Year && data.Month == data1.Month && data.Day == data1.Day &&
				data.Hour == data1.Hour && data.Minute == data1.Minute && data.Second == data1.Second &&
				data.Millisecond == data1.Millisecond);

			Assert.True("1".ToBoolOrDefault());
			Assert.True("true".ToBoolOrDefault());
			Assert.True(!"100".ToBoolOrDefault());
			Assert.True(!"0".ToBoolOrDefault());
			Assert.True(!"false".ToBoolOrDefault());
			Assert.True(!"jrfd".ToBoolOrDefault());

			Assert.True("abcabc".TruncDub("abc") == "abc");

			Assert.True("123{0}567".ToFormat(4) == "1234567");
			d = "1234567".MatchScore("123567");
			Assert.True(d > 0.8 && d < 0.9);
			Assert.True(d.Format("##0.0000") == "0,8571");

			Assert.True("1234 23456 34567 8901".Wrap(5) == "1234\n23456\n34567\n8901");

			Assert.True("810".SwitchItem("810", "643", "", "643", vbo.s_default, "") == "643");
			Assert.True("".SwitchItem("810", "643", "", "643", vbo.s_default, "default") == "643");
			Assert.True("123".SwitchItem("810", "643", "", "643", vbo.s_default, "default") == "default");
			Assert.True("123".SwitchItem("810", "643", "", "643", vbo.s_default, "") == "123");
			Assert.True("123".SwitchItem("810", "643", "", "643") == "");

			Assert.True('1'.ToIntOrDefault() == 1);
			Assert.True('1'.ToBoolOrDefault());
			Assert.True(!'0'.ToBoolOrDefault());

			Assert.True("0".ToDateOrDefault().IsEmpty());
			Assert.True(!DateTime.Now.IsEmpty());
			Assert.True("123456[[(())]]77890".RemoveChar("[[(())]]".ToCharArray()) == "12345677890");
		}
		[Fact]
		void StringExtensionsTokenTets()
		{
			using (FileLog l = new FileLog("stringExtensionsTest", new FileLogSettings() { DateFolderMask = "yyyy-MM-dd" }))
			{
				l.Write(LogLevels.Debug, "stringExtensionsTest.GetToken Start");
				string stackTrace = Environment.StackTrace;
				for (int i = 0; i < 100000; i++)
				{
					string s = stackTrace.GetToken(2, "\r\n");
				}
				l.Write(LogLevels.Debug, "stringExtensionsTest.GetToken Finish");
				l.Write(LogLevels.Debug, "stringExtensionsTest.GetTokenFromSplit Start");
				for (int i = 0; i < 100000; i++)
				{
					string s = stackTrace.Split("\r\n".ToCharArray(), StringSplitOptions.RemoveEmptyEntries)[3];
				}
				l.Write(LogLevels.Debug, "stringExtensionsTest.GetTokenFromSplit Finish");
				l.Write(LogLevels.Debug, "stringExtensionsTest Finish");
				l.Write(LogLevels.Debug, "Первый токен", stackTrace.GetToken(0, "\r\n"));
				l.Write(LogLevels.Debug, "Пустой токен", stackTrace.GetToken(100, "\r\n"));
				stackTrace = "1;223;444;133456;1;2;3;";
				int j = 0;
				do
				{
					string s = stackTrace.GetToken(j++, ";");
					l.Write(LogLevels.Debug, $"{j} токен", s);
					if (s.IsEmpty()) break;

				} while (true);
				l.Debug("12345".Qt());
				l.Debug(100.GetType().IsNumeric().ToString());
				l.Debug("123455667889".ToDecimalOrDefault().GetType().IsNumeric().ToString());
				l.Debug(this.GetType().IsNumeric().ToString());

			}
		}

		[Fact]
		void ByteArrayExtensionsTest()
		{
			Assert.True("0KHQtdGA0LPQtdC5INCS0LjRgtCw0LvRjNC10LLQuNGHINCS0L7RgdGC0YDQuNC60L7Qsg==".IsBase64String());
			Assert.Equal(new byte[] { 1, 2, 3, 4, 5 }.ToBASE64String().ToByteArray().ToBASE64String(),
				new byte[] { 1, 2, 3, 4, 5 }.ToBASE64String());
		}
		[Fact]
		void ObjectExtensionsTest()
		{
			using (FileLog l = new FileLog("ObjectExtensionsTest", new FileLogSettings() { DateFolderMask = "yyyy-MM-dd" }))
			{
				string ret = 2.Match(
				(n => n == 0, _ => "Zero"),
				(n => n == 1, _ => "One"),
				(n => n == 2, _ => "Two"),
				(n => n == 3, _ => "Three"),
				(n => n == 4, _ => "Four"));

				l.Debug(ret);
				//"Zero,One,Two,Three,Fore".Split(',').fore

				5.ForEach(q =>
				{
					dynamic retd = q.Match<int, dynamic>(
						(n => n == 0, _ => "Zero"),
						(n => n == 5, n => BitConverter.GetBytes(n)),
						(n => n == 4, n => n.ToString()),
						(n => true, n => $"default {n}"));
					if (retd != null)
						l.Debug(retd.ToString());
				});

				DateTime start = DateTime.Now;
				1000000.ForEach(q => (q % 5).Match(
					(n => n == 0, _ => "Zero"),
					(n => n == 1, _ => "One"),
					(n => n == 2, _ => "Two"),
					(n => n == 3, _ => "Three"),
					(n => n == 4, _ => "Four")));
				DateTime stop = DateTime.Now;
				l.Debug("Match Return for 1,000,000 runs took " + (stop - start).TotalMilliseconds + "ms");
			}
		}
		[Fact]
		void DecimalExtensionsTest()
		{
			using (FileLog l = new FileLog("DecimalExtensionsTest", new FileLogSettings() { DateFolderMask = "yyyy-MM-dd" }))
			{
				DateTime start = DateTime.Now;
				for (int i = 0; i < 1000000; i++)
				{
					object val = "0";
					//var a = value.GetType().IsNumeric();
					//var b = decimal.TryParse(value.ToString(), out decimal a) && a == 0;
					//var b = value.ToString().Equals("0");
					//value.Equals(0);
					var a = val.CastOf(typeof(int));
					//var a = vbo.IsEmpty(val);
				}
				object value = 0D;
				DateTime stop = DateTime.Now;
				l.Debug("Match Return for 1,000,000 runs took " + (stop - start).TotalMilliseconds + "ms");
				l.Debug($"String.Empty: {IsEmpty(string.Empty)}");
				l.Debug($"'0': {IsEmpty("0")}");
				l.Debug($"'1234567': {IsEmpty("1234567")}");
				l.Debug($"MinDate: {IsEmpty(DateTime.MinValue)}");
				l.Debug($"MinDate: {IsEmpty(DateTime.MinValue)}");
				l.Debug($"Object(decimal): {IsEmpty(value)}");
				l.Debug($"1234567d: {IsEmpty(1234567d)}");
				value = new int();
				l.Debug($"Object(integer): {IsEmpty(value)}");
				value = new double();
				l.Debug($"Object(double): {IsEmpty(value)}");
			}
		}
		[Fact]
		void TypeExtensionsTest()
		{
			using (FileLog l = new FileLog("TypeExtensionsTest", new FileLogSettings() { DateFolderMask = "yyyy-MM-dd" }))
			{
				DateTime start = DateTime.Now;
				for (int i = 0; i < 1000000; i++)
				{
					object value = typeof(int).GetDefaultValue();
				}
				DateTime stop = DateTime.Now;
				l.Debug("GetDefaultValue Return for 1,000,000 runs took " + (stop - start).TotalMilliseconds + "ms");

				start = DateTime.Now;
				for (int i = 0; i < 1000000; i++)
				{
					var value = GetDefaultValue(typeof(int));
				}
				stop = DateTime.Now;
				l.Debug("GetDefaultValue Return for 1,000,000 runs took " + (stop - start).TotalMilliseconds + "ms");

				start = DateTime.Now;
				for (int i = 0; i < 10000000; i++)
				{
					int j = '7'.ToIntOrDefault2();
				}
				stop = DateTime.Now;
				l.Debug("Char To Int Return for 1,000,000 runs took " + (stop - start).TotalMilliseconds + "ms");

				start = DateTime.Now;
				for (int i = 0; i < 10000000; i++)
				{
					int j = '7'.ToIntOrDefault();
				}
				stop = DateTime.Now;
				l.Debug("Char To Int 2 Return for 1,000,000 runs took " + (stop - start).TotalMilliseconds + "ms");
			}
		}

		public static System.ValueType GetDefaultValue(Type type) => _defaults[type];
		//public static object GetDefaultValue(Type type) => _defaults.FirstOrDefault(p => p.Key == type).Value;


		private static readonly ReadOnlyCache<Type, System.ValueType> _defaults = new ReadOnlyCache<Type, System.ValueType>(32)
			.Add(typeof(string), '\0')
			.Add(typeof(DateTime), DateTime.MinValue)
			.Add(typeof(bool), false)
			.Add(typeof(byte), 0)
			.Add(typeof(char), '\0')
			.Add(typeof(decimal), 0m)
			.Add(typeof(double), 0d)
			.Add(typeof(float), 0f)
			.Add(typeof(int), 0)
			.Add(typeof(long), 0L)
			.Add(typeof(sbyte), 0)
			.Add(typeof(short), 0)
			.Add(typeof(uint), 0)
			.Add(typeof(ulong), 0)
			.Add(typeof(ushort), 0)
			.Add(typeof(Guid), Guid.Empty)
			.Sort();

		private static HashSet<Type> _types = new HashSet<Type>()
		{
			typeof(string)
			,typeof(DateTime)
			,typeof(bool)
			,typeof(byte)
			,typeof(char)
			,typeof(decimal)
			,typeof(double)
			,typeof(float)
			,typeof(int)
			,typeof(long)
			,typeof(sbyte)
			,typeof(short)
			,typeof(uint)
			,typeof(ulong)
			,typeof(ushort)
			,typeof(Guid)
		};

		private static object[] _values = new object[] { "", DateTime.MinValue, false, 0, '\0', 0m, 0d, 0f, 0, 0L, 0, 0, 0, 0, 0, Guid.Empty };

		private static bool IsEmpty(object value)
		{
			return vbo.IsEmpty(value);
		}

	}
	static class TestExt
	{
		public static int BinarySearch(this pair[] array, int searchFor)
		{ 
			int high = array.Length - 1;
			int low = 0;
			int mid;

			if (array[0].Equals(searchFor))
				return 0;
			else if (array[high].Equals(searchFor))
				return high;
			else
			{
				while (low <= high)
				{
					mid = (high + low) / 2;
					int key = array[mid].Key.GetHashCode();
					int result =  key - searchFor;
					if (result == 0)
						return mid;
					else if (result > 0)
						high = mid--;
					else
						low = mid++;
				}
				return -1;
			}
		}

		public static void ForEach(this int n, Action<int> action)
		{
			for (int i = 0; i < n; i++)
			{
				action(i);
			}
		}
	}
	static class TestExt1
	{
		public static IEnumerable<T> ForEach<T>(IEnumerable<T> xs, Action<T> f)
		{
			foreach (var x in xs)
			{
				f(x);
				yield return x;
			}
		}
	}
}
