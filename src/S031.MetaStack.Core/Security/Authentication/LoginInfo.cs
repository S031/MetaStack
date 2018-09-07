using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
using System.Text;

namespace S031.MetaStack.Core.Security
{
	public class LoginInfo
	{
		public LoginInfo()
		{
			SessionID = Guid.NewGuid();
			LastTime = DateTime.Now;
			CryptoKey = Aes.Create().ExportBin();
		}

		public Guid SessionID { get; private set; }

		public DateTime LastTime { get; set; }

		public byte[] CryptoKey { get; private set; }

		public byte[] Export() => 
			SessionID.ToByteArray()
			.Concat(BitConverter.GetBytes(LastTime.ToBinary()))
			.Concat(CryptoKey)
			.ToArray();

		public LoginInfo Import(byte[] data)
		{
			SessionID = new Guid(data.Take(16).ToArray());
			LastTime = DateTime.FromBinary(BitConverter.ToInt64(data.Skip(16).Take(4).ToArray()));
			CryptoKey = data.Skip(20).Take(40).ToArray();
			return this;
		}
	}
}
