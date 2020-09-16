using S031.MetaStack.Common;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Linq;

namespace Metib.Factoring.Clients.CKS
{
	public class CKSSubject
	{
		private readonly XElement _xsubj;

		protected internal CKSSubject(XElement xsource)
		{
			_xsubj = xsource;
		}

		protected internal XElement Source
			=> _xsubj;

		public int ID => _xsubj.Element("id").Value.ToIntOrDefault();

		public override string ToString()
			=> _xsubj.ToString();
	}
}
