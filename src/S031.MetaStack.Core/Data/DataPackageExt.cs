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
			Exception inner = e;
			for (inner = inner.InnerException; inner!= null; )
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
	}
}
