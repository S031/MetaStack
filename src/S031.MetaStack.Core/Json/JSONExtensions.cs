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
			=>MessagePackSerializer.Typeless.Deserialize(MessagePackSerializer.FromJson(value));

		public static T DeserializeObject<T>(string value)
			=> MessagePackSerializer.Deserialize<T>(MessagePackSerializer.FromJson(value), 
				MessagePack.Resolvers.ContractlessStandardResolver.Instance);

		public static string SerializeObject(object value)
			=> MessagePackSerializer.ToJson(value);
	}
}
