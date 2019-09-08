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
			=>MessagePackSerializer.Typeless.Deserialize(MessagePack.MessagePackSerializer.FromJson(value));

		public static T DeserializeObject<T>(string value)
			=> (T)MessagePackSerializer.Deserialize<T>(MessagePack.MessagePackSerializer.FromJson(value));

		public static string SerializeObject(object value)
			=> MessagePack.MessagePackSerializer.ToJson(MessagePackSerializer.Typeless.Serialize(value));
	}
}
