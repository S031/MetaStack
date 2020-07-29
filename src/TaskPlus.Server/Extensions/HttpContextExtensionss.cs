using Microsoft.AspNetCore.Http;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Microsoft.AspNetCore.Http
{
	public static class HttpContextExtensionss
	{
		public static HttpContext AddItem<T>(this HttpContext context, T value) where T : class
		{
			var key = typeof(T);
			if (context.Items.ContainsKey(key))
				context.Items[key] = value;
			else
				context.Items.Add(key, value);
			return context;
		}

		public static T GetItem<T>(this HttpContext context) where T : class
			=> (T)context.Items[typeof(T)];
	}
}
