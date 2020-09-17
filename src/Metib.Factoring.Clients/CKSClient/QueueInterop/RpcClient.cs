using System;
using System.Collections.Concurrent;
using System.Text;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;
using System.Xml.Linq;
using System.Collections.Generic;
using S031.MetaStack.Json;
using Microsoft.Extensions.Logging;
using System.Xml;
using System.Threading.Tasks;

namespace Metib.Factoring.Clients.CKS
{
	public class RpcClient : IDisposable
	{
		private readonly RpcChannel _cahannel;
		private string _uuid = string.Empty;

		public RpcClient(string parameters)
		{
			_cahannel = RpcChannelPool.Rent(parameters);
		}

		public async Task<CKSLogin> Login()
		{
			string message = $@"
				<rabbitMsg>
					<username>{(string)_cahannel["UserLogin"]}</username>
					<password>{(string)_cahannel["UserPassword"]}</password>
				</rabbitMsg>";

			var response = await Task.Run(() => _cahannel.Call("LoginQueue", message));
			var info = new CKSLogin(response);
			if (info.Status == CKSOperationStatus.error)
				throw new CKSOperationException(info.Error.Status, info.Error.ErrorMessage);

			_uuid = info.UUID;
			return info;
		}

		public async Task<CKSSubjects> PerformClientSearch(params object[] searchParameters)
		{
			XDocument msg = new XDocument();
			XElement root = new XElement("rabbitMsg");
			root.Add(
				new XElement("operation", "Perform_Client_Search"),
				new XElement("UUID", _uuid)
				);

			XElement xsearch = new XElement("search");
			XElement xcks = new XElement("cks", xsearch);

			string pname = string.Empty;
			for (int i = 0; i < searchParameters.Length; i++)
			{
				if (i % 2 == 0)
					pname = (string)searchParameters[i];
				else
					xsearch.Add(new XElement(pname, searchParameters[i]));
			}
			root.Add(new XElement("xml", xcks.ToString()));
			msg.Add(root);

			string response = await Task.Run(() => _cahannel.Call(msg.ToString()));
			var info = new CKSSubjects(response);
			if (info.Status == CKSOperationStatus.error)
				throw new CKSOperationException(info.Error.Status, info.Error.ErrorMessage);
			return info;
		}

		public async Task<CKSSubjects> GetClientInfo(long id)
		{
			XDocument msg = new XDocument(
				new XElement("rabbitMsg",
					new XElement("operation", "Get_Client_Info"),
					new XElement("UUID", _uuid),
					new XElement("xml", 
						new XElement("cks", 
							new XElement("search",
								new XElement("clients",
									new XElement("client",
										new XAttribute("id", id)
									)
								)
							)
						).ToString()
					)
				)
			);
			string response = await Task.Run(() => _cahannel.Call(msg.ToString()));
			var info = new CKSSubjects(response);
			if (info.Status == CKSOperationStatus.error)
				throw new CKSOperationException(info.Error.Status, info.Error.ErrorMessage);
			return info;
		}

		public void Dispose() => RpcChannelPool.Return(_cahannel);
	}
}