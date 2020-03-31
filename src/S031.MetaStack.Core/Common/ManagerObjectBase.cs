using Microsoft.Extensions.Logging;
using S031.MetaStack.Common;
using S031.MetaStack.Data;
using System;

namespace S031.MetaStack.Core
{
	public enum ContextTypes
	{
		SysCat,
		Work
	}
	public abstract class ManagerObjectBase: IDisposable
	{
		private ILogger _logger;
		private bool _isLocalLog;
		private readonly MdbContext _sc;
		private readonly MdbContext _wc;

		internal bool IsLocalContext { get; set; }

		internal ManagerObjectBase(ConnectInfo schemaConnectInfo, ConnectInfo workConnectInfo, ILogger logger)
		{
			IsLocalContext = true;
			_sc = new MdbContext(schemaConnectInfo);
			_wc = new MdbContext(workConnectInfo);
			Logger = logger;

		}
		public ManagerObjectBase(MdbContext sysCatMdbContext, MdbContext workMdbContext = null)
		{
			sysCatMdbContext.NullTest(nameof(sysCatMdbContext));
			_sc = sysCatMdbContext;
			if (workMdbContext == null)
				_wc = _sc;
			else
				_wc = workMdbContext;
		}

		public void Dispose()
		{
			if (_isLocalLog)
				(_logger as IDisposable)?.Dispose();

			if (IsLocalContext)
			{
				//_sc?.Dispose();
				_wc?.Dispose();
			}
		}

		public ILogger Logger
		{
			get
			{
				if (_logger == null)
				{
					_logger = new Logging.FileLogger(this.GetType().FullName);
					_isLocalLog = true;
				}
				return _logger;
			}
			set
			{
				if (_logger != null)
					throw new InvalidOperationException($"The Logger has already been assigned this instance of the {this.GetType().FullName} class");
				_logger = value;
				_isLocalLog = false;
			}
		}

		public MdbContext GetMdbContext() => _wc;

		public MdbContext GetMdbContext(ContextTypes contextType) => 
			contextType == ContextTypes.SysCat ? _sc : _wc;
	}
}
