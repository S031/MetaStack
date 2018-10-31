using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Runtime.Serialization;

namespace S031.MetaStack.WinForms
{
	/// <summary>
	/// Формат для ID лучше всего [AssemblyID.ObjectName.ActionID], чтоб небыло одинаковых ID
	/// в разных DLL
	/// </summary>
	public interface IObjectActionPack : IObjectBase
	{
		/// <summary>
		/// Запрос параметров для выполнения процедуры
		/// </summary>
		/// <param name="actionID">ID Процедуры Формат для ID лучше всего [AssemblyID.ObjectName.ActionID]</param>
		/// <returns>ActionParameters - Коллекция параметров, результат (bool) в свойстве QueryParamResult</returns>
		ActionParameters QueryParameters(string actionID);

		/// <summary>
		/// Выполняет код в зависимости от переданного actionID параметров ActionParameters полученных
		/// при выполнении метода QueryParameters()
		/// </summary>
		/// <param name="actionID"></param>
		/// <param name="p"></param>
		/// <returns></returns>
		ActionResult Execute(string actionID, ActionParameters p);

		/// <summary>
		/// Получения списка процедур доступных для выполнения в данном приложении
		/// </summary>
		/// <returns>ActionConfig[] - массив доступных для выполнения процедур в данном приложении</returns>
		ActionConfig[] ActionList();

		IObjectHost ObjectHost { get; set; }
	}

	public class ActionConfig
	{
		public ActionConfig(IObjectActionPack actionPack)
		{
			ActionPack = actionPack;
		}
		public IObjectActionPack ActionPack { get; private set; }
		public string ActionID { get; set; }
		public string Name { get; set; }
		public string Description { get; set; }
		public bool AddContextMenu { get; set; }
		public IEnumerable<string> IncludeForms { get; set; }
		public IEnumerable<string> ExcludeForms { get; set; }
		public System.Drawing.Image Image { get; set; }
	}

	[Serializable]
	public class ActionResult : Dictionary<string, object>
	{
		protected ActionResult(SerializationInfo info, StreamingContext context)
			: base(info, context) { }

		public ActionResult()
			: base(StringComparer.CurrentCultureIgnoreCase)
		{
		}

		public new object this[string index]
		{
			get
			{
				if (this.ContainsKey(index))
					return base[index];
				else
					return null;
			}
			set
			{
				base[index] = value;
			}
		}
	}

	[Serializable]
	public class ActionParameters : Dictionary<string, object>
	{
		protected ActionParameters(SerializationInfo info, StreamingContext context)
			: base(info, context)
		{
			if (info != null)
			{
				this.QueryParamResult = info.GetBoolean("QueryParamResult");
			}
		}

		public ActionParameters()
			: base(StringComparer.CurrentCultureIgnoreCase)
		{
		}

		public bool QueryParamResult { get; set; }

		public new object this[string index]
		{
			get
			{
				if (this.ContainsKey(index))
					return base[index];
				else
					return null;
			}
			set
			{
				base[index] = value;
			}
		}

		public override void GetObjectData(SerializationInfo info, StreamingContext context)
		{
			base.GetObjectData(info, context);
			info.AddValue("QueryParamResult", this.QueryParamResult);
		}
	}
}
