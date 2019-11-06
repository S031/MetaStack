using Microsoft.Extensions.Logging;
using S031.MetaStack.Common;
using S031.MetaStack.Common.Logging;
using S031.MetaStack.Core.Logging;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Xunit;

namespace MetaStack.Test.Common
{
	public class ToolsTest
	{
		public ToolsTest()
		{
			FileLogSettings.Default.Filter = (s, i) => i >= LogLevels.Debug;
		}
		[Fact]
		private void numericTest()
		{
			using (FileLog l = new FileLog("NumericTest", new FileLogSettings() { DateFolderMask = "yyyy-MM-dd" }))
			{
				l.Write(LogLevels.Debug, "NumericTest Start");
				decimal d = 987456123.97m;
				l.Write(LogLevels.Debug, "decimal d = ", d.ToString());
				l.Write(LogLevels.Debug, "Numeric.AsString(100, \"\", false)", Numeric.AsString(100, "", false));
				Assert.True(Numeric.AsString(100, "", false).Trim() == "Сто");
				l.Write(LogLevels.Debug, "Numeric.AsString(d, \"RUR\", true)", Numeric.AsString(d, "RUR", true));
				Assert.True(Numeric.AsString(d, "RUR", true).Trim() == "Девятьсот восемьдесят семь миллионов четыреста пятьдесят шесть тысяч сто двадцать три рубля 97 копеек");
				Numeric.Add("KG", new string[7] { "килограмм", "килограмма", "килограмм", "сотая килограмма", "сотых килограмма", "сотых килограмма", "M" });
				l.Write(LogLevels.Debug, "Numeric.AsString(d, \"KG\", true)", Numeric.AsString(d, "KG", true));
				Assert.True(Numeric.AsString(d, "KG", true).Trim() == "Девятьсот восемьдесят семь миллионов четыреста пятьдесят шесть тысяч сто двадцать три килограмма 97 сотых килограмма");
				l.Write(LogLevels.Debug, "Numeric.AsString(d - 2, \"KG\", true)", Numeric.AsString(d - 2, "KG", true));
				Assert.True(Numeric.AsString(d - 2, "KG", false).Trim() == "Девятьсот восемьдесят семь миллионов четыреста пятьдесят шесть тысяч сто двадцать один килограмм");
				l.Write(LogLevels.Debug, "NumericTest Finish");
			}
		}

		[Fact]
		private void passwordGenTest()
		{
			using (FileLog l = new FileLog("passwordGenTest", new FileLogSettings() { DateFolderMask = "yyyy-MM-dd" }))
			{
				l.Debug("Start");
				for (int i = 0; i < 40; i++)
				{
					l.Debug(PasswordGenerator.Generate(new PasswordGeneratorOptions() { ValidLen = Math.Max(8, i % 32), MinSpecial = 0, MinDigit = 0, MinLower = 0 }));
				}
				for (int i = 0; i < 100; i++)
				{
					l.Debug(PasswordGenerator.Generate(new PasswordGeneratorOptions() { ValidLen = Math.Max(8, i % 32), MinSpecial = 1 }));
				}
				for (int i = 0; i < 100; i++)
				{
					l.Debug(PasswordGenerator.Generate(new PasswordGeneratorOptions() { ValidLen = Math.Max(8, i % 32), MinSpecial = 0, MinUpper = 3, MinLower = 3 }));
				}
				for (int i = 0; i < 1000; i++)
				{
					l.Debug(PasswordGenerator.Generate(new PasswordGeneratorOptions() { ValidLen = Math.Max(8, i % 32), MinSpecial = 2, MinUpper = 2, MinLower = 2, MinDigit = 2 }));
				}
				l.Debug("Finish");
			}
		}

		[Fact]
		private void fileLoggerTest()
		{
			using (FileLog logger = new FileLog("fileTest", new FileLogSettings() { DateFolderMask = "yyyy-MM-dd", CacheSize = 1000 }))
			{
				//logger.Write(LogLevels.Information, "loggerTest", "Старт");
				logger.Debug("Старт");
				List<Task> ts = new List<Task>(10);

				for (int j = 0; j < 10; j++)
				{
					ts.Add(Task.Factory.StartNew(() =>
					//System.Threading.ThreadPool.QueueUserWorkItem((t) =>
					{
						int id = System.Threading.Thread.CurrentThread.ManagedThreadId;
						using (FileLog logger2 = new FileLog($"fileTestdata{id}", new FileLogSettings() { DateFolderMask = "yyyy-MM-dd", CacheSize = 1000 }))
						{
							for (int i = 1; i <= 100000; i++)
							{
								//logger2.Write(LogLevels.Information, "loggerTest", "Сообщение № {0} в потоке {1}".ToFormat(i, id));
								logger2.Debug("Сообщение № {0} в потоке {1}".ToFormat(i, id));
							}
						}
					}));
				}
				Task.WaitAll(ts.ToArray());
				logger.Write(LogLevels.Information, "loggerTest", "Финиш");
			}
		}
		[Fact]
		private void fileLoggerTestAsync()
		{
			using (FileLog logger = new FileLog("fileTestAsync", new FileLogSettings() { DateFolderMask = "yyyy-MM-dd", CacheSize = 1000 }))
			{
				logger.Write(LogLevels.Information, "loggerTest", "Старт");
				List<Task> ts = new List<Task>(10);
				for (int j = 0; j < 10; j++)
				{
					ts.Add(Task.Factory.StartNew(() =>
					{
						_fileLoggerTestAsync(logger);
					}));
				}
				Task.WaitAll(ts.ToArray());
				logger.Write(LogLevels.Information, "loggerTest", "Финиш");
			}
		}

		private void _fileLoggerTestAsync(FileLog logger)
		{
			for (int i = 1; i <= 100000; i++)
			{
				logger.Write(LogLevels.Information, "loggerTest", "Сообщение № {0} в потоке {1}".ToFormat(i,
					System.Threading.Thread.CurrentThread.ManagedThreadId));
			}
		}
		/// <summary>
		/// Работает в 3 раза медленнее
		/// </summary>
		[Fact]
		private void fileLoggerFactoryTest()
		{
			using (ILoggerFactory factory = new LoggerFactory())
			{
				factory.AddFileLog(new FileLogSettings() { DateFolderMask = "yyyy-MM-dd", CacheSize = 1000 });
				//ILogger loger = factory.CreateLogger<ToolsTest>();
				ILogger loger = factory.CreateLogger("ToolsTest");
				loger.LogInformation("Старт");
				List<Task> ts = new List<Task>(10);

				for (int j = 0; j < 10; j++)
				{
					ts.Add(Task.Factory.StartNew(() =>
					{
						for (int i = 1; i <= 100000; i++)
						{
							string data = $"Сообщение № {i} в потоке {System.Threading.Thread.CurrentThread.ManagedThreadId}";
							loger.LogInformation(new EventId(1001, "fileLoggerFactoryTest"), data);
							//loger.LogDebug(new EventId(1001, "fileLoggerFactoryTest"), "Test debug with eventID");
							////очень медленно будет
							//loger.LogDebug("Test debug without eventID");
							////
							//loger.Log<string>(LogLevel.Information, new EventId(0, "fileLoggerFactoryTest"), data, null, null);
						}
					}));
				}
				Task.WaitAll(ts.ToArray());
				loger.LogInformation("Финиш");
			}
		}

		[Fact]
		private void schedulerTestSecond()
		{
			using (FileLog l = new FileLog("schedulerTest", new FileLogSettings() { DateFolderMask = "yyyy-MM-dd", CacheSize = 1000 }))
			{
				Scheduler s = new Scheduler(10, UnitOfQt.Second)
				{
					DayStartTime = "10:30:00",
					DayEndTime = "21:30:00"
				};
				DateTime d1 = DateTime.Now;
				DateTime d2 = d1.AddDays(2);
				for (DateTime d = s.GetNextStartTime(d1); d < d2; d = s.GetNextStartTime(d))
				{
					l.Write(LogLevels.Information, "schedulerTestSecond", $"GetNextStartTime={d.ToString(vbo.FullDateFormat)}");
				}
			}
		}

		[Fact]
		private void schedulerTestDay()
		{
			using (FileLog l = new FileLog("schedulerTest", new FileLogSettings() { DateFolderMask = "yyyy-MM-dd", CacheSize = 1000 }))
			{
				Scheduler s = new Scheduler(1, UnitOfQt.Day)
				{
					DayStartTime = "10:30:00",
					WeekDayMask = "2,5"
				};
				DateTime d3 = DateTime.Now;
				for (int i = 0; i < 1000; i++)
				{
					d3 = s.GetNextStartTime(d3);
					l.Write(LogLevels.Information, "schedulerTestDay", $"GetNextStartTime={d3.ToString(vbo.FullDateFormat)}");
				}
			}
		}
	}
}
