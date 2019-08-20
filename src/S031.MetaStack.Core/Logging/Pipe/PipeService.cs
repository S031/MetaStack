using S031.MetaStack.Core.Actions;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Text;

namespace S031.MetaStack.Core
{
	
	public class PipeService
	{
		readonly ConcurrentDictionary<ActionContext, ConcurrentQueue<string>> _messages = 
			new ConcurrentDictionary<ActionContext, ConcurrentQueue<string>>(); 

		public void Write(ActionContext ctx, string message)
		{
			if (!_messages.TryGetValue(ctx, out  ConcurrentQueue<string> data))
			{
				_messages.TryAdd(ctx, new ConcurrentQueue<string>());
				data = _messages[ctx];
			}
			data.Enqueue(message);
		}

		public string Read(ActionContext ctx)
		{
			if (!_messages.TryGetValue(ctx, out ConcurrentQueue<string> data))
				return null;

			StringBuilder sb = new StringBuilder();
			for (; ; )
				if (data.TryDequeue(out string message))
					sb.Append(message + "\f");
				else
					break;
			return sb.ToString();
		}

		public bool Cancel(ActionContext ctx) => _messages.TryRemove(ctx, out ConcurrentQueue<string> data);
	}
}
