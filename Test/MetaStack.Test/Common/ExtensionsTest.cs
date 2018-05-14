using Xunit;
using S031.MetaStack.Common;
using Xunit.Abstractions;
using System;
using S031.MetaStack.Common.Logging;


namespace MetaStack.Test.Common
{
	public class ExtensionsTest
    {
		public ExtensionsTest()
		{
			FileLogSettings.Default.Filter = (s, i) => i >= LogLevels.Debug;
		}
		[Fact]
		void nullExceptiobnTest()
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
		void stringExtensionsTest()
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
		void stringExtensionsTokenTets()
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
		void byteArrayExtensionsTest()
		{
			Assert.True("0KHQtdGA0LPQtdC5INCS0LjRgtCw0LvRjNC10LLQuNGHINCS0L7RgdGC0YDQuNC60L7Qsg==".IsBase64String());
			Assert.Equal(new byte[] { 1, 2, 3, 4, 5 }.ToBASE64String().ToByteArray().ToBASE64String(), 
				new byte[] { 1, 2, 3, 4, 5 }.ToBASE64String());
		}
    }
}
