﻿using Xunit;
using S031.MetaStack.Common.Logging;
using S031.MetaStack.Core;
using System.Collections.Generic;
using System;
using S031.MetaStack.Core.Data;
using System.Text;
using System.Net.Sockets;
using System.IO;

namespace MetaStack.Test.Services
{
	public class TCPServerServicesTest
	{
		public TCPServerServicesTest()
		{
			FileLogSettings.Default.Filter = (s, i) => i >= LogLevels.Debug;
		}
		[Fact]
		void speedTest2()
		{
			using (FileLog l = new FileLog("TCPServerServicesTest", new FileLogSettings() { DateFolderMask = "yyyy-MM-dd" }))
			{
				var p = getTestPackage();
				l.Debug($"Input message: {p.ToString(TsExportFormat.JSON)}");
				using (var client = new AppClient())
				{
					client.Open();
					for (int i = 0; i < 2000; i++)
						p = client.SendAndWaitForResponse1(p);
					l.Debug(p.ToString(TsExportFormat.JSON));
					l.Debug($"Output message: {p.ToString(TsExportFormat.JSON)}");
				}
			}
		}

		[Fact]
		void speedTest()
		{
			using (FileLog l = new FileLog("TCPServerServicesTest", new FileLogSettings() { DateFolderMask = "yyyy-MM-dd" }))
			{
				var p = getTestPackage();
				l.Debug($"Input message: {p.ToString(TsExportFormat.JSON)}");
				for (int i = 0; i < 2000; i++)
					p = SendAndWaitForResponse1(p);
				l.Debug(p.ToString(TsExportFormat.JSON));
				l.Debug($"Output message: {p.ToString(TsExportFormat.JSON)}");
			}
		}
		private DataPackage SendAndWaitForResponse1(DataPackage input)
		{
			var data = input.ToArray();
			using (var client = new TcpClient())
			{
				var stream = ConnectAndSend1(client, data);

				var buffer = new byte[4096];

				int bytesRead;

				List<byte> a = new List<byte>(1024);
				while ((bytesRead = stream.Read(buffer, 0, buffer.Length)) != 0)
				{
					//a.AddRange(buffer);
					for (int i = 0; i < bytesRead; i++)
					{
						a.Add(buffer[i]);
					}

				}
				return new DataPackage(a.ToArray());
			}
		}
		private NetworkStream ConnectAndSend1(TcpClient client, byte[] data)
		{
			client.Connect("localhost", 8001);
			var stream = client.GetStream();
			stream.Write(BitConverter.GetBytes(data.Length), 0, 4);
			stream.Write(data, 0, data.Length);
			return stream;
		}
		DataPackage getTestPackage()
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
		private class testClass
		{
			public testClass()
			{
				ItemList = new Dictionary<string, object>();
			}
			public int ID { get; set; }
			public string Name { get; set; }
			public Dictionary<string, object> ItemList { get; set; }
		}

		public sealed class AppClient : IDisposable
		{
			private string m_ip = "localhost";
			private int m_port = 8001;
			private TcpClient m_tcpClient;

			private Stream stream;
			//private StreamWriter writer;
			//private StreamReader reader;

			public DataPackage SendAndWaitForResponse1(DataPackage input)
			{
				var data = input.ToArray();
				stream.Write(BitConverter.GetBytes(data.Length), 0, 4);
				stream.Write(data, 0, data.Length);
				stream.Flush();
				var buffer = new byte[4096];

				int bytesRead;

				List<byte> a = new List<byte>(1024);
				while ((bytesRead = stream.Read(buffer, 0, buffer.Length)) != 0)
				{
					for (int i = 0; i < bytesRead; i++)
					{
						a.Add(buffer[i]);
					}
				}
				return new DataPackage(a.ToArray());
			}

			public void Open()
			{
				m_tcpClient = new TcpClient(m_ip, m_port);
				stream = m_tcpClient.GetStream();
				//writer = new StreamWriter(stream);
				//reader = new StreamReader(stream);
			}

			void Close()
			{
				m_tcpClient.Client.Dispose();
				//reader.Dispose();
				//writer.Dispose();
				stream.Dispose();
			}

			public void Dispose()
			{
				Close();
			}

			//Plus Dispose implementation
		}
	}
}
