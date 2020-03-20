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
			byte[] data = new byte[10];
			var c = Customer.CreateTestCustomer();

			DateTime start = DateTime.Now;
			for (int i = 0; i < 1000000; i++)
			{
				_w.Position = 0;
				_w.Write(i);
				_w.Write(i + 0.12d);
				_w.Write(test_string);
				_w.Write();
				_w.Write(c);
			}

			TimeSpan ts = DateTime.Now - start;
			_r.Position = 0;
			_logger.Debug($"Result: {_r.ReadValues(5)[4]} in {ts.TotalMilliseconds} ms");
		}

		void ReaderPerformanceTest(FileLog _logger)
		{
			DateTime start = DateTime.Now;
			for (int i = 0; i < 1000000; i++)
			{
				_r.Position = 0;
				_r.ReadNext();
				_r.ReadInt32();
				_r.ReadNext();
				_r.ReadDouble();
				_r.ReadNext();
				_r.ReadString();
				_r.ReadValue();
				_r.Read<Customer>();
			}

			TimeSpan ts = DateTime.Now - start;
			_r.Position = 0;
			_logger.Debug($"Result: {_r.ReadValues(5)[4]} in {ts.TotalMilliseconds} ms with size {_r.Lenght} bytes");
		}

		[Fact]
		public void ComputeHashTest()
		{
			using (FileLog _logger = new FileLog("MetaStackBuffers.ComputeHashTest", new FileLogSettings() { DateFolderMask = "yyyy-MM-dd" }))
			{
				DateTime start = DateTime.Now;
				for (int i = 0; i < 1000000; i++)
				{
					i.GetType().ToString().ComputeHash();
				}
				TimeSpan ts = DateTime.Now - start;
				_logger.Debug($"Result: {typeof(int).ToString().ComputeHash()} in {ts.TotalMilliseconds} ms");
				_logger.Debug($"{typeof(bool).FullName}: {typeof(bool).Name.ComputeHash()}");
				_logger.Debug($"{typeof(sbyte).Name}: {typeof(sbyte).Name.ComputeHash()}");
				_logger.Debug($"{typeof(byte).Name}: {typeof(byte).Name.ComputeHash()}");
				_logger.Debug($"{typeof(short).Name}: {typeof(short).Name.ComputeHash()}");
				_logger.Debug($"{typeof(ushort).Name}: {typeof(ushort).Name.ComputeHash()}");
				_logger.Debug($"{Type.GetTypeCode(typeof(DataPackage))}: {typeof(DataPackage).Name.ComputeHash()}");
				_logger.Debug($"{typeof(ushort).Name}: {typeof(ushort).Name.ComputeHash()}");
			}
		}
		
		[Fact]
		public void GetTypeSpeedTest()
		{
			using (FileLog _logger = new FileLog("MetaStackBuffers.GetTypeSpeedTest", new FileLogSettings() { DateFolderMask = "yyyy-MM-dd" }))
			{
				var c = Customer.CreateTestCustomer();
				string typeName = typeof(Customer).AssemblyQualifiedName;

				DateTime start = DateTime.Now;
				for (int i = 0; i < 1000000; i++)
				{
					_ = Type.GetType(typeName).CreateInstance();
				}
				TimeSpan ts = DateTime.Now - start;
				_logger.Debug($"Result Type.GetType: {ts.TotalMilliseconds} ms");
				
				start = DateTime.Now;
				for (int i = 0; i < 1000000; i++)
				{
					_ = GetType(typeName).CreateInstance();
				}
				ts = DateTime.Now - start;
				_logger.Debug($"Result Fast.GetType: {ts.TotalMilliseconds} ms");
			}
		}

		private static MapTable<string, Type> _typeCache = new MapTable<string, Type>();

		private static Type GetType(string name)
		{
			if (!_typeCache.TryGetValue(name, out Type t))
			{
				t = Type.GetType(name);
				_typeCache.Add(name, t);
			}
			return t;
		}

	}
}
