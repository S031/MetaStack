﻿using S031.MetaStack.Buffers;
using System;
using System.Security.Cryptography;

namespace S031.MetaStack.Security
{
	public class LoginInfo
	{
		public LoginInfo()
		{
			SessionID = Guid.NewGuid();
			Ticket = Guid.Empty;
			LastTime = DateTime.Now;
			CryptoKey = Aes.Create().ExportBin();
		}

		public Guid SessionID { get; private set; }

		public Guid Ticket { get; private set; }

		public DateTime LastTime { get; set; }

		public byte[] CryptoKey { get; private set; }

		public byte[] Export()
			=> new BinaryDataWriter(128)
				.Write(SessionID)
				.Write(Ticket)
				.Write(LastTime)
				.Write(CryptoKey)
				.GetBytes();
			//SessionID.ToByteArray()
			//.Concat(Ticket.ToByteArray())
			//.Concat(BitConverter.GetBytes(LastTime.ToBinary()))
			//.Concat(CryptoKey)
			//.ToArray();

		public LoginInfo Import(byte[] data)
		{
			BinaryDataReader reader = new BinaryDataReader((BinaryDataBuffer)data);
			SessionID = reader.Read<Guid>();
			Ticket = reader.Read<Guid>();
			LastTime = reader.Read<DateTime>();
			CryptoKey = reader.Read<byte[]>();
			//SessionID = new Guid(data.Take(16).ToArray());
			//Ticket = new Guid(data.Skip(16).Take(16).ToArray());
			//LastTime = DateTime.FromBinary(BitConverter.ToInt64(data.Skip(32).Take(8).ToArray(), 0));
			//CryptoKey = data.Skip(40).Take(52).ToArray();
			return this;
		}

		public LoginInfo EmmitTicket()
		{
			Ticket = Guid.NewGuid();
			return this;
		}
		public bool IsLogedOn() => Ticket != Guid.Empty;
	}
}
