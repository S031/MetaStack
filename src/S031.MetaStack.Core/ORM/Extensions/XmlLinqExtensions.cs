using S031.MetaStack.Common;
using System.Xml.Linq;
using System.Xml.XPath;

namespace S031.MetaStack.Core.ORM
{
    static class xmlHelper
    {
        public static string elementValue(this XElement source, string elemName, string defaultValue = "")
        {
            XElement elem = source.selectElement(elemName);
            if (elem == null)
            {
                XAttribute attr = source.Attribute(elemName);
                if (attr == null)
                    return string.Empty;
                return attr.Value;
            }
            return elem.Value;
        }
        public static XElement selectElement(this XElement source, string elemName)
        {
            var sepPos = elemName.IndexOf('/');
            if (sepPos > -1)
            {
                if (sepPos == elemName.Length - 1)
                    elemName = elemName.Left(elemName.Length - 1);
                return source.XPathSelectElement(elemName);
            }
            return source.Element(elemName);
        }

    }
}
