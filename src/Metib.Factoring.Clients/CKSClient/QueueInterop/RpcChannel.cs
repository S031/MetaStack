using System;
using System.Collections.Concurrent;
using System.Text;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;
using System.Xml.Linq;
using S031.MetaStack.Json;
using Microsoft.Extensions.Logging;
using System.Diagnostics;

namespace Metib.Factoring.Clients.CKS
{
	internal class RpcChannel: IDisposable
	{
		//private const string _exchange = "cks.general.exchange";
		//private const string _loginQueue = "cks.login";
		//private const string _operationQueue = "cks.operations";
		//private const string _uploadQueue = "cks.upload";

		private readonly IConnection _connection;
		private readonly IModel _channel;
		private readonly string _replyQueueName;
		private readonly EventingBasicConsumer _consumer;
		private readonly BlockingCollection<string> _respQueue = new BlockingCollection<string>();
		private readonly IBasicProperties _props;
		private readonly Encoding _encoding = Encoding.Default;

		private readonly JsonObject _rabbitConnectorOptions;

		public RpcChannel(string parameters, int poolPosition)
		{
			_rabbitConnectorOptions = JsonObject.Parse(parameters);
			
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
			PoolPosition = poolPosition;
		}

		public object this[string index]
			=>_rabbitConnectorOptions[index].GetValue();

		public int PoolPosition { get; }

		public string Call(string message)
			=> Call("OperationQueue", message);

		public string Call(string queuesettingKey, string message)
		{
			string routingKey = _rabbitConnectorOptions[queuesettingKey];
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

		public bool Closed => _connection == null 
			|| !_connection.IsOpen;

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
