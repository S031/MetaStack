using S031.MetaStack.Common;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Security.Claims;

namespace S031.MetaStack.Integral.Security
{
	public static class ClaimsTypes
	{
		private static readonly ReadOnlyCache<string, string> _claimValues;
		private static readonly ReadOnlyCache<string, string> _claimKeys;
		private static readonly ReadOnlyCache<string, string> _claimTypeValues;
		private static readonly ReadOnlyCache<string, string> _claimTypeKeys;

		static ClaimsTypes()
		{
			var finfo = typeof(ClaimTypes)
				.GetFields(BindingFlags.Public | BindingFlags.Static | BindingFlags.FlattenHierarchy);

			_claimValues = new ReadOnlyCache<string, string>(
				 finfo
				.Select(f => new KeyValuePair<string, string>(f.Name, (string)f.GetValue(null)))
				.ToArray());

			_claimKeys = new ReadOnlyCache<string, string>(
				 finfo
				.Select(f => new KeyValuePair<string, string>((string)f.GetValue(null), f.Name))
				.ToArray());
			
			finfo = typeof(ClaimValueTypes)
				.GetFields(BindingFlags.Public | BindingFlags.Static | BindingFlags.FlattenHierarchy);
			
			_claimTypeValues = new ReadOnlyCache<string, string>(
				 finfo
				.Select(f => new KeyValuePair<string, string>(f.Name, (string)f.GetValue(null)))
				.ToArray());

			_claimTypeKeys = new ReadOnlyCache<string, string>(
				 finfo
				.Select(f => new KeyValuePair<string, string>((string)f.GetValue(null), f.Name))
				.ToArray());
		}
		public static IReadOnlyDictionary<string, string> Keys
			=> _claimKeys;
		public static IReadOnlyDictionary<string, string> Values
			=> _claimValues;
		public static string GetKey(string value)
		{
			if (_claimKeys.TryGetValue(value, out string key))
				return key;
			return value;
		}
		public static string GetValue(string key)
		{
			if (_claimValues.TryGetValue(key, out string value))
				return value;
			return key;
		}
		public static string GetTypeKey(string value)
		{
			if (_claimTypeKeys.TryGetValue(value, out string key))
				return key;
			return value;
		}
		public static string GetTypeValue(string key)
		{
			if (_claimTypeValues.TryGetValue(key, out string value))
				return value;
			return key;
		}
	}
}
