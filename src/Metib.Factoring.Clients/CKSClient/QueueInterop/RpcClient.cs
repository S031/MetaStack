using System;
using System.Collections.Concurrent;
using System.Text;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;
using System.Xml.Linq;
using System.Collections.Generic;

namespace Metib.Factoring.Clients.CKS
{
	public class RpcClient : IDisposable
	{
		private readonly IConnection _connection;
		private readonly IModel _channel;
		private readonly string _replyQueueName;
		private readonly EventingBasicConsumer _consumer;
		private readonly BlockingCollection<string> _respQueue = new BlockingCollection<string>();
		private readonly IBasicProperties _props;
		private readonly Encoding _encoding = Encoding.Default;
		private string _uuid = string.Empty;

		private const string _un = "rabbit_service";
		private const string _pwd = "";
		private const string _exchange = "cks.general.exchange";
		private const string _loginQueue = "cks.login";
		private const string _operationQueue = "cks.operations";
		private const string _uploadQueue = "cks.upload";

		public RpcClient()
		{
			var factory = new ConnectionFactory()
			{
				HostName = "192.168.247.72",
				VirtualHost = "test",
				Port = 5672,
				UserName = _un,
				Password = _pwd,
			};

			_connection = factory.CreateConnection();
			_channel = _connection.CreateModel();
			_channel.ExchangeDeclare(exchange: _exchange, type: ExchangeType.Direct, durable: true);
			_replyQueueName = _channel.QueueDeclare().QueueName;
			_consumer = new EventingBasicConsumer(_channel);

			_props = _channel.CreateBasicProperties();
			var correlationId = Guid.NewGuid().ToString();
			_props.CorrelationId = correlationId;
			_props.ReplyTo = _replyQueueName;

			_consumer.Received += (model, ea) =>
			{
				var body = ea.Body.ToArray();
				var response = Encoding.UTF8.GetString(body);
				if (ea.BasicProperties.CorrelationId == correlationId)
				{
					_respQueue.Add(response);
				}
			};
		}

		public CKSLogin Login(string userName, string password)
		{
			string message = $@"
				<rabbitMsg>
					<username>{userName}</username>
					<password>{password}</password>
				</rabbitMsg>";
			var response = CallInternal(_loginQueue, message);
			var info = new CKSLogin(response);
			_uuid = info.UUID;
			return info;
		}

		public bool Connected
			=> !string.IsNullOrEmpty(_uuid);

		public CKSSubject[] Read(params object[] searchParameters)
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

			string response = CallInternal(_operationQueue, msg.ToString());
			return CKSSubject
				.Load(CKSBase.Parse(response))
				.ToArray();
		}

		public string Call(string message)
			=> CallInternal(_operationQueue, message);

		private string CallInternal(string routingKey, string message)
		{
			var messageBytes = _encoding.GetBytes(message);
			_channel.BasicPublish
				(exchange: _exchange,
				routingKey: routingKey,
				basicProperties: _props,
				body: messageBytes);

			_channel.BasicConsume(
				consumer: _consumer,
				queue: _replyQueueName,
				autoAck: true);

			return _respQueue.Take();
		}

		public void Close()
		{
			if (_connection.IsOpen)
				_connection.Close();
		}

		public void Dispose()
		{
			Close();
			_channel.Dispose();
			_connection.Dispose();
		}
	}
}