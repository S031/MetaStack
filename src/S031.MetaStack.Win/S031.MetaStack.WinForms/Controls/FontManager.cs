using System;
using System.Linq;
using System.Windows.Forms;
using System.Drawing;

namespace S031.MetaStack.WinForms
{
	public static class FontManager
	{
		static Screen _screen = Screen.AllScreens.First((screen) => screen.Primary);
		static int _width = _screen.Bounds.Width;
		static float _baseFontSize = 9f; //dbs.GetSetting("Newbank;Setup;BaseFontSize", _width >= 1900 ? "9" : "8").ToIntOrDefault();
		public static float BaseFontSize { get => _baseFontSize; }
		public static void SetBaseFontSize(float newFontSize)
		{
			_baseFontSize = newFontSize;
		}
		public static Screen Screen
		{
			get { return _screen; }
		}

		public static void Adop(Control ctrl)
		{
			Type type = ctrl.GetType();
			if (_baseFontSize > 8 && type == typeof(MenuStrip))
			{
				MenuStrip topLevelMenu = ctrl as MenuStrip;
				foreach (ToolStripItem ts in topLevelMenu.Items)
				{
					Adop(ts);
				}
			}
			else if (_baseFontSize > 8 && type == typeof(ContextMenuStrip))
			{
				ContextMenuStrip topLevelMenu = ctrl as ContextMenuStrip;
				foreach (ToolStripItem ts in topLevelMenu.Items)
				{
					Adop(ts);
				}
			}
			else if (_baseFontSize > 8)
			{
				//if (_baseFontSize > 10f)
				//	_baseFontSize = 10f;

				//ctrl.Font = new System.Drawing.Font("Arial", (float)(_baseFontSize - 0.25));
				//ctrl.Font = new Font(ctrl.Font.FontFamily, 9.5f, ctrl.Font.Style, ctrl.Font.Unit, ctrl.Font.GdiCharSet, ctrl.Font.GdiVerticalFont);
				if (ctrl.Font.Size < _baseFontSize)
					ctrl.Font = new Font(ctrl.Font.FontFamily, _baseFontSize, ctrl.Font.Style, ctrl.Font.Unit, ctrl.Font.GdiCharSet, ctrl.Font.GdiVerticalFont);
			}
		}

		public static void Adop(ToolStripItem menu)
		{

			if (_baseFontSize > 10f)
				_baseFontSize = 10f;

			if (_baseFontSize < 9f || menu == null || menu is ToolStripSeparator)
				return;

			float size = (float)_baseFontSize + 0.25f;
			menu.Font = new Font(menu.Font.FontFamily, size, menu.Font.Style, menu.Font.Unit, menu.Font.GdiCharSet, menu.Font.GdiVerticalFont);

			ToolStripDropDownItem item = menu as ToolStripDropDownItem;
			if (item != null)
			{
				foreach (ToolStripItem ts in item.DropDownItems)
				{
					Adop(ts);
				}
			}
		}
	}
}
