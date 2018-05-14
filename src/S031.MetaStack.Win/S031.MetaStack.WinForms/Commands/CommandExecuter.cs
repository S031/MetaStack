using System;
using System.Collections.Generic;

namespace S031.MetaStack.WinForms
{
	public class CommandExecuter<T> where T : struct
	{
		IObjectHost _objHost;

		Dictionary<T, Action> _commands;

		public CommandExecuter() : this(null) { }

		public CommandExecuter(IObjectHost objectHost)
		{
			_commands = new Dictionary<T, Action>(64);
			_objHost = objectHost;
		}

		public Action this[T commandID]
		{
			get { return _commands.ContainsKey(commandID) ? _commands[commandID] : null; }
			set { _commands[commandID] = value; }
		}

		public Action this[string commandName]
		{
			get
			{
				if (typeof(T).IsEnum)
				{
					T commandID = (T)Enum.Parse(typeof(T), commandName);
					return _commands.ContainsKey(commandID) ? _commands[commandID] : null;
				}
				return null;
			}
			set
			{
				T commandID = (T)Enum.Parse(typeof(T), commandName);
				_commands[commandID] = value;
			}
		}

		public IObjectHost ObjectHost
		{
			get { return _objHost; }
			set { _objHost = value; }
		}

		public T[] FreeCustomCommandCells()
		{
			List<T> keys = new List<T>();
			for (int i = 1; i <= 16; i++)
			{
				T key = (T)Enum.Parse(typeof(DBBrowseCommandsEnum), "CustomItem" + i.ToString());
				if (!_commands.ContainsKey(key) || _commands[key] == null)
					keys.Add(key);
			}
			return keys.ToArray();
		}
	}

	public class CommandExecuter<T, TArg, TRet>
		where T : struct
	{
		IObjectHost _objHost;

		Dictionary<T, Func<TArg, TRet>> _commands;

		public CommandExecuter() : this(null) { }

		public CommandExecuter(IObjectHost objectHost)
		{
			_commands = new Dictionary<T, Func<TArg, TRet>>(64);
			_objHost = objectHost;
		}

		public Func<TArg, TRet> this[T commandID]
		{
			get { return _commands.ContainsKey(commandID) ? _commands[commandID] : null; }
			set { _commands[commandID] = value; }
		}

		public Func<TArg, TRet> this[string commandName]
		{
			get
			{
				if (typeof(T).IsEnum)
				{
					T commandID = (T)Enum.Parse(typeof(T), commandName);
					return _commands.ContainsKey(commandID) ? _commands[commandID] : null;
				}
				return null;
			}
			set
			{
				T commandID = (T)Enum.Parse(typeof(T), commandName);
				_commands[commandID] = value;
			}
		}

		public IObjectHost ObjectHost
		{
			get { return _objHost; }
			set { _objHost = value; }
		}
	}
}

