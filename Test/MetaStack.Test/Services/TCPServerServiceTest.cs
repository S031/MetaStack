using S031.MetaStack.Common.Logging;
using S031.MetaStack.Core.Data;
using System;
using System.Collections.Generic;
using System.IO;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;
using Xunit;

namespace MetaStack.Test.Services
{
	public class TCPServerServicesTest
	{
		public TCPServerServicesTest()
		{
			FileLogSettings.Default.Filter = (s, i) => i >= LogLevels.Debug;
		}
		[Fact]
		private void SpeedTest4ConnectedSocket()
		{
			using (FileLog l = new FileLog("TCPServerServicesTest", new FileLogSettings() { DateFolderMask = "yyyy-MM-dd" }))
			{
				var p = GetTestPackage();
				l.Debug($"Input message: {p.ToString(TsExportFormat.JSON)}");
				DataPackage response = null;
				using (var client = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp))
				{
					client.Connect("localhost", 8001);
					using (var stream = new NetworkStream(client))
					{
						for (int i = 0; i < 2000; i++)
						{
							response = SendAndRecieve(stream, p);
						}

						Close(stream);
					}
				}
				l.Debug($"Output message: {response}");
			}
		}

		[Fact]
		private void SpeedTest4Mixed()
		{
			using (FileLog l = new FileLog("TCPServerServicesTest", new FileLogSettings() { DateFolderMask = "yyyy-MM-dd" }))
			{
				var p = GetTestPackage();
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
								{
									response = SendAndRecieve(stream, p);
								}

								Close(stream);
							}
						}
					}));
				}
				Task.WaitAll(tasks.ToArray());
				l.Debug($"Output message: {response}");
			}
		}

		private static DataPackage SendAndRecieve(NetworkStream stream, DataPackage p)
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

		private void Close(NetworkStream stream)
		{
			stream.Write(BitConverter.GetBytes(0), 0, 4);
		}

		[Fact]
		private void SpeedTestForDisconnectedSocket()
		{
			using (FileLog l = new FileLog("TCPServerServicesTest", new FileLogSettings() { DateFolderMask = "yyyy-MM-dd" }))
			{
				var p = GetTestPackage();
				l.Debug($"Input message: {p.ToString(TsExportFormat.JSON)}");
				DataPackage response = null;
				for (int i = 0; i < 2000; i++)
				{
					using (var client = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp))
					{
						client.Connect("localhost", 8001);
						using (var stream = new NetworkStream(client))
						{
							response = SendAndRecieve(stream, p);
							Close(stream);
						}
					}
				}
				l.Debug($"Output message: {response}");
			}
		}

		private static DataPackage GetTestPackage()
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
				//без сериализации работает в 1.5 раза быстрееp
				p["Col5"] = null;
				//p["Col5"] = new testClass() { ID = i, Name = (string)p["Col2"] };
				p.Update();
			}
			return p;
		}
		private class TestClass
		{
			public TestClass()
			{
				ItemList = new Dictionary<string, object>();
			}
			public int ID { get; set; }
			public string Name { get; set; }
			public Dictionary<string, object> ItemList { get; set; }
		}

		public sealed class AppClient : IDisposable
		{
			private readonly TcpClient _client;
			private readonly Stream _stream;

			public DataPackage SendAndWaitForResponse(DataPackage input)
			{
				var data = input.ToArray();
				Send(data);
				return Resieve();
			}
			private void Send(byte[] data)
			{
				_stream.Write(BitConverter.GetBytes(data.Length), 0, 4);
				_stream.Write(data, 0, data.Length);
			}

			private DataPackage Resieve()
			{
				byte[] buffer = new byte[4096];
				var bytesRead = 0;

				var headerRead = 0;
				while (headerRead < 4 && (bytesRead = _stream.Read(buffer, headerRead, 4 - headerRead)) > 0)
				{
					headerRead += bytesRead;
				}

				var bytesRemaining = BitConverter.ToInt32(buffer, 0);

				List<byte> l = new List<byte>();
				while (bytesRemaining > 0 && (bytesRead = _stream.Read(buffer, 0, Math.Min(bytesRemaining, buffer.Length))) != 0)
				{
					for (int i = 0; i < bytesRead; i++)
					{
						l.Add(buffer[i]);
					}
					bytesRemaining -= bytesRead;
				}
				return new DataPackage(l.ToArray());
			}

			public AppClient(string ip, int port)
			{
				_client = new TcpClient(ip, port);
				_stream = _client.GetStream();
			}
			public void Dispose()
			{
				_stream.Close();
				_client.Client.Dispose();
			}

			//Plus Dispose implementation
		}
	}
}
