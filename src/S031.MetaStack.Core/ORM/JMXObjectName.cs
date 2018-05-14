﻿using S031.MetaStack.Common;
using System;
using System.Collections.Generic;
using System.Text;

#if NETCOREAPP2_0
namespace S031.MetaStack.Core.ORM
#else
namespace S031.MetaStack.WinForms.ORM
#endif
{
	public struct JMXObjectName
	{
		internal JMXObjectName(string fullObjectName)
		{
			int pos = fullObjectName.IndexOf('.');
			if (pos > -1)
			{
				AreaName = fullObjectName.Left(pos);
				ObjectName = fullObjectName.Substring(pos + 1);
			}
			else
			{
				AreaName = string.Empty;
				ObjectName = fullObjectName;
			}
		}
		public JMXObjectName(string areaName, string objectName)
		{
			AreaName = areaName;
			ObjectName = objectName;
		}
		public string AreaName { get; set; }
		public string ObjectName { get; set; }
		public static bool operator ==(JMXObjectName m1, JMXObjectName m2)
		{
			return (m1.AreaName == m2.AreaName &&
				m1.ObjectName == m2.ObjectName);
		}
		public static bool operator !=(JMXObjectName m1, JMXObjectName m2)
		{
			return (m1.AreaName != m2.AreaName ||
				m1.ObjectName != m2.ObjectName);
		}
		public override bool Equals(object obj)
		{
			return this.ToString().Equals(obj.ToString(), StringComparison.CurrentCultureIgnoreCase);
		}
		public override int GetHashCode()
		{
			return new { AreaName, ObjectName }.GetHashCode();
		}
		public override string ToString()
		{
			return this.ToString(false);
		}
		public string ToString(bool withQuotes)
		{
			if (withQuotes)
				return $"[{AreaName}].[{ObjectName}]";
			return $"{AreaName}.{ObjectName}";
		}

		public static implicit operator JMXObjectName(string s)
		{
			return new JMXObjectName(s);
		}

		public static implicit operator string(JMXObjectName name)
		{
			return name.ToString();
		}

		public bool IsEmpty()
		{
			return ObjectName.IsEmpty();
		}
	}
}
