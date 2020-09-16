using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Linq;

namespace Metib.Factoring.Clients.CKS
{
	public class CKSPerson : CKSSubject
	{
		protected internal CKSPerson(XElement xsource) : base(xsource)
		{
		}
	}
}
