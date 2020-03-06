using System;
using Xunit;
using S031.MetaStack.Buffers;
using S031.MetaStack.Common.Logging;
using S031.MetaStack.Common;
using S031.MetaStack.Data;

namespace MetaStack.Test.Buffers
{
	public class BinaryBuffersTest
	{
		const string test_string = "ShipmentID	Выбор	Договор Поставки	Полный Номер Накладной	Дата Поставки	Дата платежа	Валюта	Сумма Накладной	Долг Дебитора	Долг Поставщика	Финансирование	Заявка Номер	Заявка Дата	Менеджер";
		const string ascii_test_string = "AccountID	OwnerTypeID	OwnerID	AccountTypeID	Title	AccountNumber	AccountCurrent	AccountActive	AutoOpen	OpenDate	AddresseeBIK	AddresseeINN	AddresseeTitle	AddresseeAccountNumber	AddresseeBank	XMLDescription	CreationTime	CreatedByManager";
		//const string test_property_name = "ShipmentID";

		private  BinaryDataBuffer _b;
		private  BinaryDataWriter _w;
		private  BinaryDataReader _r;

		public BinaryBuffersTest()
		{
			FileLogSettings.Default.Filter = (s, i) => i >= LogLevels.Debug;
		}

		[Fact]
		public void BufferPerformanceTest()
		{
			using (FileLog _logger = new FileLog("MetaStackBuffers.PerformanceTest", new FileLogSettings() { DateFolderMask = "yyyy-MM-dd" }))
			using (_b = new BinaryDataBuffer(1024))
			{
				_w = new BinaryDataWriter(_b);
				_r = new BinaryDataReader(_b);
				WriterPerformanceTest(_logger);
				ReaderPerformanceTest(_logger);
			}
		}

		void WriterPerformanceTest(FileLog _logger)
		{
			//byte[] data = new byte[10];

			DateTime start = DateTime.Now;
			for (int i = 0; i < 1_000_000; i++)
			{
				//_w.Position = 0;
				_w.Write(i);
				_w.Write(i + 0.12d);
				_w.Write(test_string);
				//fixed (char* source = ascii_test_string)
				//	_w.Write(source, ascii_test_string.Length);
				_w.Write(ascii_test_string);
				_w.Write();
			}

			TimeSpan ts = DateTime.Now - start;
			BinaryDataReader r2 = new BinaryDataReader(_b) { Position = 0 };
			_logger.Debug($"Result: {r2.ReadValues(5)[1]} in {ts.TotalMilliseconds} ms");
		}

		void ReaderPerformanceTest(FileLog _logger)
		{
			DateTime start = DateTime.Now;
			_r.Position = 0;
			for (int i = 0; i < 1_000_000; i++)
			{
				_r.ReadNext();
				_r.ReadInt32();
				//_r.ReadValues(4);
				_r.ReadNext();
				_r.ReadDouble();
				_r.ReadNext();
				_r.ReadString();
				_r.ReadNext();
				_r.ReadString();
				_r.ReadValue();
			}
			TimeSpan ts = DateTime.Now - start;
			BinaryDataReader r2 = new BinaryDataReader(_b) { Position = 0 };
			_logger.Debug($"Result: {r2.ReadValues(5)[1]} in {ts.TotalMilliseconds} ms");
		}

		[Fact]
		public void ComputeHashTest()
		{
			using (FileLog _logger = new FileLog("MetaStackBinaryReader.PerformanceTest", new FileLogSettings() { DateFolderMask = "yyyy-MM-dd" }))
			{
				DateTime start = DateTime.Now;
				for (int i = 0; i < 1000000; i++)
				{
					i.GetType().ToString().ComputeHash();
				}
				TimeSpan ts = DateTime.Now - start;
				_logger.Debug($"Result: {typeof(int).ToString().ComputeHash()} in {ts.Milliseconds} ms");
				_logger.Debug($"{typeof(bool).FullName}: {typeof(bool).Name.ComputeHash()}");
				_logger.Debug($"{typeof(sbyte).Name}: {typeof(sbyte).Name.ComputeHash()}");
				_logger.Debug($"{typeof(byte).Name}: {typeof(byte).Name.ComputeHash()}");
				_logger.Debug($"{typeof(short).Name}: {typeof(short).Name.ComputeHash()}");
				_logger.Debug($"{typeof(ushort).Name}: {typeof(ushort).Name.ComputeHash()}");
				_logger.Debug($"{Type.GetTypeCode(typeof(DataPackage))}: {typeof(DataPackage).Name.ComputeHash()}");
				_logger.Debug($"{typeof(ushort).Name}: {typeof(ushort).Name.ComputeHash()}");
			}
		}
	}
}
