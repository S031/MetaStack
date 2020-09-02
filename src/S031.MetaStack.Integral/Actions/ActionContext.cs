using S031.MetaStack.Integral.Security;
using System;
using System.Threading;

namespace S031.MetaStack.Actions
{
	public class ActionContext
	{
		public ActionContext()
		{
		}
		public ActionContext(IServiceProvider services)
		{
			Services = services;
		}
		public string UserName { get; set; }
		public string ConnectionName { get; set; }
		public string SessionID { get; set; }
		public IServiceProvider Services { get; }
		public CancellationToken CancellationToken { get; set; }
		public UserInfo Principal { get; set; }
		public override string ToString() => $"{UserName}.{SessionID}.{ConnectionName}";
		public override int GetHashCode() => new { UserName, SessionID, ConnectionName }.GetHashCode();
		public override bool Equals(object obj) => obj != null && this.GetHashCode() == obj.GetHashCode();
	}
}
