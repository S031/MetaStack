using System;
using System.Collections.Generic;
using System.Text;
using Xunit;
using S031.MetaStack.Buffers;
using S031.MetaStack.Common.Logging;

namespace MetaStack.Test.Buffers
{
	public class BinaryBuffersTest
	{
		public BinaryBuffersTest()
		{
			FileLogSettings.Default.Filter = (s, i) => i >= LogLevels.Debug;
		}

		[Fact]
		public void ReaderPerformanceTest()
		{
			const string test_string = "ShipmentID	Выбор	Договор Поставки	Полный Номер Накладной	Дата Поставки	Дата платежа	Валюта	Сумма Накладной	Долг Дебитора	Долг Поставщика	Финансирование	Заявка Номер	Заявка Дата	Менеджер";

			using (FileLog _logger = new FileLog("MetaStackBinaryReader.PerformanceTest", new FileLogSettings() { DateFolderMask = "yyyy-MM-dd" }))
			{
				DateTime start = DateTime.Now;
				List<string> l = new List<string>();
				byte[] data = new byte[1];
				BinaryDataWriter w = new BinaryDataWriter(16);
				w.Write(data);
				object o = data;
				for (int i = 0; i < 1_000_000; i++)
				{
					BinaryDataReader r = new BinaryDataReader(w.Source);
					r.Position = 0;
					r.ReadNext();
					r.ReadBytes();
					//w.Position = 0;
					//w.Write(0);

				}
				//test_string.ComputeHash();
				//i.GetType().ToString().ComputeHash();
				//Type.GetTypeCode(i.GetType());
				//typeof(IList).IsAssignableFrom(i.GetType());
				//GetExportType(data);


				TimeSpan ts = DateTime.Now - start;
				BinaryDataReader r2 = new BinaryDataReader(w.Source);
				r2.Position = 0;
				_logger.Debug($"Result: {r2.ReadValue()} in {ts.Milliseconds} ms");
			}
		}

		[Fact]
		public void WriterPerformanceTest()
		{
			const string test_string = "ShipmentID	Выбор	Договор Поставки	Полный Номер Накладной	Дата Поставки	Дата платежа	Валюта	Сумма Накладной	Долг Дебитора	Долг Поставщика	Финансирование	Заявка Номер	Заявка Дата	Менеджер";

			using (FileLog _logger = new FileLog("MetaStackBinaryReader.PerformanceTest", new FileLogSettings() { DateFolderMask = "yyyy-MM-dd" }))
			{
				DateTime start = DateTime.Now;
				List<string> l = new List<string>();
				byte[] data = new byte[1];
				BinaryDataWriter w = new BinaryDataWriter(16);
				w.Write(data);
				object o = data;
				for (int i = 0; i < 1_000_000; i++)
				{
					w.Position = 0;
					w.Write(test_string);
				}
				TimeSpan ts = DateTime.Now - start;
				BinaryDataReader r2 = new BinaryDataReader(w.Source);
				r2.Position = 0;
				_logger.Debug($"Result: {r2.ReadValue()} in {ts.Milliseconds} ms");
			}
		}
	}
}
