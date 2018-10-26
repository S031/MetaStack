using System;
using System.Collections.Generic;
using System.Data;
using System.Text;

namespace S031.MetaStack.Core.Data
{
	public partial class DataPackage : IDataReader
	{
		public static DataPackage CreateErrorPackage(Exception e)
		{
			DataPackage p = new DataPackage(new string[] { "ErrorCode.String.255", "ErrorDescription.String.1024", "Source.String.512", "StackTrace.String.1024" });
			p.Headers["Status"] = "ERROR";
			p.UpdateHeaders();
			addException(p, e);
			for (Exception inner = e.InnerException; inner!= null; inner = inner.InnerException)
				addException(p, inner);
			return p;
		}
		static void addException(DataPackage p, Exception e)
		{
			p.AddNew();
			p["ErrorCode"] = e.GetType().FullName;
			p["ErrorDescription"] = e.Message;
			p["Source"] = e.Source;
			p["StackTrace"] = e.StackTrace;
			p.Update();
		}
		public static DataPackage CreateOKPackage(int result = 0, string message = "Operation status OK")
		{
			return new DataPackage(new string[] { "Result.Int", "Message.String.256" })
				.SetHeader("Status", "OK")
				.AddNew()
				.SetValue("Result", result)
				.SetValue("Message", message)
				.Update();
		}
	}
}
