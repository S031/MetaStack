using System;
using System.Collections.Generic;
namespace S031.MetaStack.WinForms
{
	/// <summary>
	/// Интерфейс используемый для фильтрации типов загружаемых из сборок для расширения свойств объектов
	/// такие как: ObjectEditors, ObjectFormExtenders и т.д.
	/// </summary>
	public interface IObjectBase
	{
		//IObjectHost ObjectHost { get; set; }
		string ObjectName { get; set; }
	}
}
