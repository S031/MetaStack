using S031.MetaStack.Common;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Security.Claims;

namespace TaskPlus.Server.Security.Claims
{
	public static class Claim
	{
		private static readonly ReadOnlyCache<string, string> _claimValues;
		private static readonly ReadOnlyCache<string, string> _claimNames;

		static Claim()
		{
			_claimValues = new ReadOnlyCache<string, string>(
				 typeof(ClaimTypes)
				.GetFields(BindingFlags.Public | BindingFlags.Static | BindingFlags.FlattenHierarchy)
				.Select(f => new KeyValuePair<string, string>(f.Name, (string)f.GetValue(null)))
				.ToArray());

			_claimNames = new ReadOnlyCache<string, string>(
				_claimValues.Select(c => new KeyValuePair<string, string>(c.Value, c.Key))
				.ToArray());
		}

		public static IReadOnlyDictionary<string, string> Names
			=> _claimNames;

		public static IReadOnlyDictionary<string, string> Values
			=> _claimValues;
	}
}
