using Microsoft.AspNetCore.Http;
using S031.MetaStack.Common;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Threading.Tasks;

namespace TaskPlus.Server.Middleware
{
	public class PipeLineRouteValues
	{
		public PipeLineRouteValues() { }
		
		public PipeLineRouteValues(HttpContext context)
		{
			var v = context.Request.RouteValues;
			Method = context.Request.Method;
			if (v.TryGetValue("controller", out object routeValue))
				Controller = (string)routeValue;
			if (v.TryGetValue("action", out routeValue))
				Action = (string)routeValue;
			if (v.TryGetValue("version", out routeValue))
				Version = (string)routeValue;
		}

		public string Method { get; set; } = "POST";
		public string Controller { get; set; } = "default";
		public string Action { get; set; } = string.Empty;
		public string Version { get; set; } = "v1";
		public bool Validate()
			=> (Method.Equals("POST", StringComparison.OrdinalIgnoreCase) || Method.Equals("GET", StringComparison.OrdinalIgnoreCase))
			&& !Controller.IsEmpty()
			&& !Action.IsEmpty()
			&& !Version.IsEmpty();

		public override string ToString()
			=> $"{Controller}/{Action}".ToLower();

	}
}
