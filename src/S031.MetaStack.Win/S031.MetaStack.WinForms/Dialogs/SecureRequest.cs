using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace S031.MetaStack.WinForms
{
	public static class SecureRequest
	{
		public static string Show(string userName)
		{
			return (string)InputBox.Show(new WinFormItem("Password")
			{
				Caption = "Введите пароль",
				PresentationType = typeof(TextBox),
				ControlTrigger = (i, c) =>
				{
					//c.FindForm().Text = $"Вход в систему пользователя {userName}";
					TextBox tb = (c as TextBox);
					tb.Width = 100;
					tb.PasswordChar = '*';
				}
			})[0];
		}
	}
}
