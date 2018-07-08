using Microsoft.VisualStudio.TestTools.UnitTesting;
using S031.MetaStack.Common.Logging;
using S031.MetaStack.Core;
using System.Collections.Generic;
using System;
using S031.MetaStack.Core.Data;
using System.Text;
using System.Net.Sockets;
using System.IO;
using System.Threading.Tasks;

namespace MetaStack.UnitTest.Services
{
	[TestClass]
	public class TCPServerServicesTest
	{
		public TCPServerServicesTest()
		{
			FileLogSettings.Default.Filter = (s, i) => i >= LogLevels.Debug;
		}
		[TestMethod]
		public void speedTest4ConnectedSocket()
		{
			using (FileLog l = new FileLog("TCPServerServicesTest", new FileLogSettings() { DateFolderMask = "yyyy-MM-dd" }))
			{
				var p = getTestPackage();
				l.Debug($"Input message: {p.ToString(TsExportFormat.JSON)}");
				DataPackage response = null;
				using (var client = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp))
				{
					client.Connect("localhost", 8001);
					using (var stream = new NetworkStream(client))
					{
						for (int i = 0; i < 2000; i++)
							response = sendAndRecieve(stream, p);
						close(stream);
					}
				}
				l.Debug($"Output message: {response}");
			}
		}

		[TestMethod]
		public void speedTest4Mixed()
		{
			using (FileLog l = new FileLog("TCPServerServicesTest", new FileLogSettings() { DateFolderMask = "yyyy-MM-dd" }))
			{
				var p = getTestPackage();
				l.Debug($"Input message: {p.ToString(TsExportFormat.JSON)}");
				DataPackage response = null;
				List<Task> tasks = new List<Task>();
				for (int j = 0; j < 100; j++)
				{
					tasks.Add(Task.Factory.StartNew(() =>
					{
						using (var client = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp))
						{
							client.Connect("localhost", 8001);
							using (var stream = new NetworkStream(client))
							{
								for (int i = 0; i < 2000; i++)
									response = sendAndRecieve(stream, p);
								close(stream);
							}
						}
					}));
				}
				Task.WaitAll(tasks.ToArray());
				l.Debug($"Output message: {response}");
			}
		}

		private static DataPackage sendAndRecieve(NetworkStream stream, DataPackage p)
		{
			var data = p.ToArray();
			stream.Write(BitConverter.GetBytes(data.Length), 0, 4);
			stream.Write(data, 0, data.Length);
			var buffer = new byte[4];

			stream.Read(buffer, 0, 4);
			var byteCount = BitConverter.ToInt32(buffer, 0);
			var res = new byte[byteCount];
			stream.Read(res, 0, byteCount);
			return new DataPackage(res);
		}

		void close(NetworkStream stream)
		{
			stream.Write(BitConverter.GetBytes(0), 0, 4);
		}

		[TestMethod]
		public void speedTestForDisconnectedSocket()
		{
			using (FileLog l = new FileLog("TCPServerServicesTest", new FileLogSettings() { DateFolderMask = "yyyy-MM-dd" }))
			{
				var p = getTestPackage();
				l.Debug($"Input message: {p.ToString(TsExportFormat.JSON)}");
				DataPackage response = null;
				for (int i = 0; i < 2000; i++)
				{
					using (var client = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp))
					{
						client.Connect("localhost", 8001);
						using (var stream = new NetworkStream(client))
						{
							response = sendAndRecieve(stream, p);
							close(stream);
						}
					}
				}
				l.Debug($"Output message: {response}");
			}
		}

		static DataPackage getTestPackage()
		{
			var p = new DataPackage(new string[] { "Col1.int", "Col2.string.255", "Col3.datetime.10", "Col4.Guid.34", "Col5.object" });
			p.Headers.Add("Username", "Сергей");
			p.Headers.Add("Password", "1234567T");
			p.Headers.Add("Sign", UnicodeEncoding.UTF8.GetBytes("Сергей"));
			p.UpdateHeaders();
			int i = 0;
			for (i = 0; i < 2; i++)
			{
				p.AddNew();
				p["Col1"] = i;
				p["Col2"] = $"Строка # {i}";
				p["Col3"] = DateTime.Now.AddDays(i);
				p["Col4"] = Guid.NewGuid();
				p["Col5"] = null;
				//p["Col5"] = new testClass() { ID = i, Name = (string)p["Col2"] };
				p.Update();
			}
			return p;
		}
	}
}
