using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Threading.Tasks;

namespace TaskPlus.Server.Actions
{
	public class ActionResult<T> where T : class
	{
		public ActionResult(HttpStatusCode statusCode)
		{
			StatusCode = statusCode;
		}

		public HttpStatusCode StatusCode { get; }

		public T Value { get; set; }

		public static ActionResult<T> OK(T value)
			=> new ActionResult<T>(HttpStatusCode.OK) { Value = value };

		public static ActionResult<T> Unauthorized(T value = null)
			=> new ActionResult<T>(HttpStatusCode.Unauthorized) { Value = value };
		public static ActionResult<T> Error(T value = null)
			=> new ActionResult<T>(HttpStatusCode.Unauthorized) { Value = value };
	}
}
