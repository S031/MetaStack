using System;
using System.Collections.Concurrent;
using System.Text;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;
using System.Xml.Linq;
using System.Collections.Generic;
using S031.MetaStack.Json;
using Microsoft.Extensions.Logging;

namespace Metib.Factoring.Clients.CKS
{
	public enum RpcClientStatusCodes
	{
		None,
		OK,
		Error
	}
	public class RpcClient : IDisposable
	{
		private const string _exchange = "cks.general.exchange";
		private const string _loginQueue = "cks.login";
		private const string _operationQueue = "cks.operations";
		private const string _uploadQueue = "cks.upload";

		private IConnection _connection;
		private IModel _channel;
		private string _replyQueueName;
		private EventingBasicConsumer _consumer;
		private readonly BlockingCollection<string> _respQueue = new BlockingCollection<string>();
		private IBasicProperties _props;
		private readonly Encoding _encoding = Encoding.Default;
		private string _uuid = string.Empty;

		private readonly JsonObject _rabbitConnectorOptions;
		private readonly ILogger _logger;


		public RpcClientStatusCodes Status { get; private set; } = RpcClientStatusCodes.None;

		public RpcClient(JsonObject parameters, ILogger logger)
		{
			_rabbitConnectorOptions = parameters;
			_logger = logger;
			try
			{
				Initialize();
			}
			catch (Exception ex)
			{
				logger.LogError(ex.Message);
				Status = RpcClientStatusCodes.Error;
			}
		}

		private void Initialize()
		{
			var factory = new ConnectionFactory()
			{
				HostName = _rabbitConnectorOptions["HostName"],
				VirtualHost = _rabbitConnectorOptions["VirtualHost"],
				Port = (int)_rabbitConnectorOptions["Port"],
				UserName = _rabbitConnectorOptions["QueueServiceLogin"],
				Password = _rabbitConnectorOptions["QueueServicePassword"],
			};

			_connection = factory.CreateConnection();
			_channel = _connection.CreateModel();
			_channel.ExchangeDeclare(
				exchange: _rabbitConnectorOptions["Exchange"], 
				type: ExchangeType.Direct, 
				durable: true);
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
			var response = CallInternal(_rabbitConnectorOptions["LoginQueue"], message);
			var info = new CKSLogin(response);
			_uuid = info.UUID;
			Status = RpcClientStatusCodes.OK;
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

			string response = CallInternal(_rabbitConnectorOptions["OperationQueue"], msg.ToString());
			return CKSSubject
				.Load(CKSBase.Parse(response))
				.ToArray();
		}

		public string Call(string message)
			=> CallInternal(_rabbitConnectorOptions["OperationQueue"], message);

		private string CallInternal(string routingKey, string message)
		{
			var messageBytes = _encoding.GetBytes(message);
			_channel.BasicPublish
				(exchange: _rabbitConnectorOptions["Exchange"],
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