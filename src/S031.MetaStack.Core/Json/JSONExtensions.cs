using System;
using MessagePack;

#if NETCOREAPP
namespace S031.MetaStack.Core.Json
#else
namespace S031.MetaStack.WinForms.Json
#endif
{
	public static class JSONExtensions
    {
 
		public static object DeserializeObject(string value)
			=>MessagePackSerializer.Typeless.Deserialize(Convert.FromBase64String(value));

		public static T DeserializeObject<T>(string value)
			=> (T)MessagePackSerializer.Typeless.Deserialize(Convert.FromBase64String(value));

		public static string SerializeObject(object value)
			=> Convert.ToBase64String(MessagePackSerializer.Typeless.Serialize(value));
	}
}
