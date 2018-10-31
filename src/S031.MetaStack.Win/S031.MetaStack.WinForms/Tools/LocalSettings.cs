using System;
using System.Collections.Generic;

namespace S031.MetaStack.WinForms
{
	[global::System.Runtime.CompilerServices.CompilerGeneratedAttribute()]
	internal sealed class LocalSettings : global::System.Configuration.ApplicationSettingsBase
	{
		private static LocalSettings defaultInstance = ((LocalSettings)(global::System.Configuration.ApplicationSettingsBase.Synchronized(new LocalSettings())));

		LocalSettings()
		{
			if (!this.Upgraded)
			{
				this.Upgrade();
				this.Upgraded = true;
				this.Save();
			}
		}

		public static LocalSettings Default
		{
			get
			{
				return defaultInstance;
			}
		}

		[global::System.Configuration.UserScopedSettingAttribute()]
		[global::System.Diagnostics.DebuggerNonUserCodeAttribute()]
		public global::System.DateTime DateStart
		{
			get
			{
				if (this["DateStart"] == null) this["DateStart"] = DateTime.Now.Date;
				return ((global::System.DateTime)(this["DateStart"]));
			}
			set
			{
				this["DateStart"] = value;
			}
		}

		[global::System.Configuration.UserScopedSettingAttribute()]
		[global::System.Diagnostics.DebuggerNonUserCodeAttribute()]
		public global::System.DateTime DateFinish
		{
			get
			{
				if (this["DateFinish"] == null) this["DateFinish"] = DateTime.Now.Date;
				return ((global::System.DateTime)(this["DateFinish"]));
			}
			set
			{
				this["DateFinish"] = value;
				this.Save();
			}
		}

		[global::System.Configuration.UserScopedSettingAttribute()]
		[global::System.Diagnostics.DebuggerNonUserCodeAttribute()]
		[global::System.Configuration.DefaultSettingValueAttribute("False")]
		public bool Upgraded
		{
			get
			{
				return ((bool)(this["Upgraded"]));
			}
			set
			{
				this["Upgraded"] = value;
			}
		}

	}
}
