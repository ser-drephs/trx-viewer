using System;
using System.Xml;

namespace Trx.Viewer.Core.Business.Trx
{
    public static class TrxResultFileReaderExtensions
    {
        public static TEnum ParseEnumFromXmlNode<TEnum>(this TrxResultFileReader _, XmlAttributeCollection attributes, string value) where TEnum : Enum 
        {
            var namedItem = attributes.GetNamedItem(value);
            return namedItem != null ? (TEnum)Enum.Parse(typeof(TEnum), namedItem.Value) : default;
        }

        public static bool TryParseEnumFromXmlNode<TEnum>(this TrxResultFileReader _, XmlAttributeCollection attributes, string value, out TEnum @enum) where TEnum : Enum
        {
            @enum = default;
            try
            {
                @enum = _.ParseEnumFromXmlNode<TEnum>(attributes, value);
                return true;
            } catch (System.Exception) { return false; }
        }
    }
}
