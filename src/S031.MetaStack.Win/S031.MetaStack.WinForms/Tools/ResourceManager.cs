using S031.MetaStack.Common;
using System.Collections.Generic;
using System.Drawing;
using System.Reflection;

namespace S031.MetaStack.WinForms
{
	public static class ResourceManager
	{
		private readonly static Dictionary<string, Image> images = new Dictionary<string, Image>();
		private readonly static Dictionary<string, Icon> icons = new Dictionary<string, Icon>();
		private readonly static Dictionary<string, string> objectFormats = new Dictionary<string, string>();
		private readonly static Dictionary<string, string> objectForms = new Dictionary<string, string>();

		
		public static Image GetImage(string imageName, Assembly asm = null)
		{
			if (!imageName.Contains(".")) imageName += ".png";
			if (images.TryGetValue(imageName, out Image img)) return img;
			if (asm == null)
				asm = typeof( ResourceManager).Assembly;
			if (imageName.Right(4).ToLower() == ".ico")
			{
				Icon ico = new Icon(asm.GetManifestResourceStream(asm.FullName.Split(',')[0] + ".Resources." + imageName), new Size(16, 16));
				img = Bitmap.FromHicon(ico.Handle);
			}
			else
			{
				if ((img = Bitmap.FromStream(asm.GetManifestResourceStream(asm.FullName.Split(',')[0] + ".Resources." + imageName))) != null)
				{
					images[imageName] = img;
				}
			}
			return img;
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Performance", "CA1811:AvoidUncalledPrivateCode")]
		public static Icon GetIcon(string iconName, Size size, Assembly asm = null)
		{
			if (!iconName.Contains(".")) iconName += ".ico";
			if (icons.TryGetValue(iconName, out Icon ico)) return ico;
			if (asm == null)
				asm = typeof(ResourceManager).Assembly;
			return new Icon(asm.GetManifestResourceStream(asm.FullName.Split(',')[0] + ".Resources." + iconName), size);
		}

		public static string GetObjectFormat(string objectName)
		{
			if (!objectFormats.ContainsKey(objectName))
			{

				Assembly asm = typeof(ResourceManager).Assembly;
				objectFormats[objectName] =
					new System.IO.StreamReader(asm.GetManifestResourceStream(
						asm.FullName.Split(',')[0] + ".Resources.ObjectFormats." + objectName + ".xml")).ReadToEnd();
			}
			return objectFormats[objectName];
		}

		public static string GetObjectForm(string objectName)
		{
			if (!objectForms.ContainsKey(objectName))
			{

				Assembly asm = typeof(ResourceManager).Assembly;
				objectForms[objectName] =
					new System.IO.StreamReader(asm.GetManifestResourceStream(
						asm.FullName.Split(',')[0] + ".Resources.ObjectForms." + objectName + ".xml")).ReadToEnd();
			}
			return objectForms[objectName];
		}
	}
}
