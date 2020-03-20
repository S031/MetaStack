using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace S031.MetaStack.Buffers
{
	public interface IBinaryDataFormatter
	{
		void Write(BinaryDataWriter writer, object value);

		object Read(Type type, BinaryDataReader reader);
	}
}
