using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
using System.Text;

#if NETCOREAPP
namespace S031.MetaStack.Core.Security
#else
namespace S031.MetaStack.WinForms.Security
#endif
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

		public byte[] Export() =>
			SessionID.ToByteArray()
			.Concat(Ticket.ToByteArray())
			.Concat(BitConverter.GetBytes(LastTime.ToBinary()))
			.Concat(CryptoKey)
			.ToArray();

		public LoginInfo Import(byte[] data)
		{
			SessionID = new Guid(data.Take(16).ToArray());
			Ticket = new Guid(data.Skip(16).Take(16).ToArray());
			LastTime = DateTime.FromBinary(BitConverter.ToInt64(data.Skip(32).Take(8).ToArray(), 0));
			CryptoKey = data.Skip(40).Take(52).ToArray();
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
