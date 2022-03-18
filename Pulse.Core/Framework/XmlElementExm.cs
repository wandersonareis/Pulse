using System;
using System.Globalization;
using System.Xml;

namespace Pulse.Core
{
    public static class XmlElementExm
    {
        public static XmlDocument GetOwnerDocument(this XmlElement self)
        {
            Exceptions.CheckArgumentNull(self, "self");

            XmlDocument doc = self.OwnerDocument;
            if (doc == null)
                throw new ArgumentException("XmlDocument не найден.", nameof(self));

            return doc;
        }

        public static XmlElement GetChildElement(this XmlElement self, string name)
        {
            Exceptions.CheckArgumentNull(self, "self");
            Exceptions.CheckArgumentNullOrEmprty(name, "name");

            XmlElement child = self[name];
            if (child == null)
                throw Exceptions.CreateArgumentException("self", "Дочерний XmlElement '{0}' не найден.", name);

            return child;
        }

        public static XmlElement GetChildElement(this XmlElement self, int index)
        {
            Exceptions.CheckArgumentNull(self, "self");

            XmlElement child = self.ChildNodes[index] as XmlElement;
            if (child == null)
                throw Exceptions.CreateArgumentException("self", "Дочерний XmlElement '{0}' не найден.", index);

            return child;
        }

        public static XmlAttribute GetChildAttribute(this XmlElement self, string name)
        {
            Exceptions.CheckArgumentNull(self, "self");
            Exceptions.CheckArgumentNullOrEmprty(name, "name");

            XmlAttribute attr = self.Attributes[name];
            if (attr == null)
                throw Exceptions.CreateArgumentException("self", "XmlAttribute '{0}' не найден.", name);
            
            return attr;
        }

        public static XmlElement CreateChildElement(this XmlElement self, string name)
        {
            Exceptions.CheckArgumentNull(self, "self");
            Exceptions.CheckArgumentNullOrEmprty(name, "name");

            XmlDocument doc = self.GetOwnerDocument();
            XmlElement element = doc.CreateElement(name);
            self.AppendChild(element);

            return element;
        }

        public static XmlElement EnsureChildElement(this XmlElement self, string name)
        {
            Exceptions.CheckArgumentNull(self, "self");
            Exceptions.CheckArgumentNullOrEmprty(name, "name");

            XmlElement element = self[name];
            if (element != null)
                return element;

            XmlDocument doc = self.GetOwnerDocument();
            element = doc.CreateElement(name);
            self.AppendChild(element);

            return element;
        }

        public static XmlAttribute EnsureChildAttribute(this XmlElement self, string name)
        {
            Exceptions.CheckArgumentNull(self, "self");
            Exceptions.CheckArgumentNullOrEmprty(name, "name");

            XmlAttribute attr = self.Attributes[name];
            if (attr != null)
                return attr;

            XmlDocument doc = self.GetOwnerDocument();
            attr = doc.CreateAttribute(name);
            self.Attributes.Append(attr);

            return attr;
        }

        public static bool? FindBoolean(this XmlElement self, string name)
        {
            Exceptions.CheckArgumentNull(self, "self");
            Exceptions.CheckArgumentNullOrEmprty(name, "name");

            XmlAttribute arg = self.Attributes[name];
            if (arg == null)
                return null;

            return Convert.ToBoolean(arg.Value, CultureInfo.InvariantCulture);
        }

        public static char? FindChar(this XmlElement self, string name)
        {
            Exceptions.CheckArgumentNull(self, "self");
            Exceptions.CheckArgumentNullOrEmprty(name, "name");

            XmlAttribute arg = self.Attributes[name];
            if (arg == null)
                return null;

            return Convert.ToChar(arg.Value, CultureInfo.InvariantCulture);
        }

        public static sbyte? FindSByte(this XmlElement self, string name)
        {
            Exceptions.CheckArgumentNull(self, "self");
            Exceptions.CheckArgumentNullOrEmprty(name, "name");

            XmlAttribute arg = self.Attributes[name];
            if (arg == null)
                return null;

            return Convert.ToSByte(arg.Value, CultureInfo.InvariantCulture);
        }

        public static byte? FindByte(this XmlElement self, string name)
        {
            Exceptions.CheckArgumentNull(self, "self");
            Exceptions.CheckArgumentNullOrEmprty(name, "name");

            XmlAttribute arg = self.Attributes[name];
            if (arg == null)
                return null;

            return Convert.ToByte(arg.Value, CultureInfo.InvariantCulture);
        }

        public static short? FindInt16(this XmlElement self, string name)
        {
            Exceptions.CheckArgumentNull(self, "self");
            Exceptions.CheckArgumentNullOrEmprty(name, "name");

            XmlAttribute arg = self.Attributes[name];
            if (arg == null)
                return null;

            return Convert.ToInt16(arg.Value, CultureInfo.InvariantCulture);
        }

        public static ushort? FindUInt16(this XmlElement self, string name)
        {
            Exceptions.CheckArgumentNull(self, "self");
            Exceptions.CheckArgumentNullOrEmprty(name, "name");

            XmlAttribute arg = self.Attributes[name];
            if (arg == null)
                return null;

            return Convert.ToUInt16(arg.Value, CultureInfo.InvariantCulture);
        }

        public static int? FindInt32(this XmlElement self, string name)
        {
            Exceptions.CheckArgumentNull(self, "self");
            Exceptions.CheckArgumentNullOrEmprty(name, "name");

            XmlAttribute arg = self.Attributes[name];
            if (arg == null)
                return null;

            return Convert.ToInt32(arg.Value, CultureInfo.InvariantCulture);
        }

        public static uint? FindUInt32(this XmlElement self, string name)
        {
            Exceptions.CheckArgumentNull(self, "self");
            Exceptions.CheckArgumentNullOrEmprty(name, "name");

            XmlAttribute arg = self.Attributes[name];
            if (arg == null)
                return null;
            
            return Convert.ToUInt32(arg.Value, CultureInfo.InvariantCulture);
        }

        public static long? FindInt64(this XmlElement self, string name)
        {
            Exceptions.CheckArgumentNull(self, "self");
            Exceptions.CheckArgumentNullOrEmprty(name, "name");

            XmlAttribute arg = self.Attributes[name];
            if (arg == null)
                return null;

            return Convert.ToInt64(arg.Value, CultureInfo.InvariantCulture);
        }

        public static ulong? FindUInt64(this XmlElement self, string name)
        {
            Exceptions.CheckArgumentNull(self, "self");
            Exceptions.CheckArgumentNullOrEmprty(name, "name");

            XmlAttribute arg = self.Attributes[name];
            if (arg == null)
                return null;

            return Convert.ToUInt64(arg.Value, CultureInfo.InvariantCulture);
        }

        public static double? FindDouble(this XmlElement self, string name)
        {
            Exceptions.CheckArgumentNull(self, "self");
            Exceptions.CheckArgumentNullOrEmprty(name, "name");

            XmlAttribute arg = self.Attributes[name];
            if (arg == null)
                return null;

            return Convert.ToDouble(arg.Value, CultureInfo.InvariantCulture);
        }

        public static string FindString(this XmlElement self, string name)
        {
            Exceptions.CheckArgumentNull(self, "self");
            Exceptions.CheckArgumentNullOrEmprty(name, "name");

            XmlAttribute arg = self.Attributes[name];
            return arg?.Value;
        }

        public static bool GetBoolean(this XmlElement self, string name)
        {
            bool? value = FindBoolean(self, name);
            if (value == null)
                throw Exceptions.CreateException("Аттрибут '{0}' не найден.", name);

            return value.Value;
        }

        public static char GetChar(this XmlElement self, string name)
        {
            char? value = FindChar(self, name);
            if (value == null)
                throw Exceptions.CreateException("Аттрибут '{0}' не найден.", name);

            return value.Value;
        }

        public static sbyte GetSByte(this XmlElement self, string name)
        {
            sbyte? value = FindSByte(self, name);
            if (value == null)
                throw Exceptions.CreateException("Аттрибут '{0}' не найден.", name);

            return value.Value;
        }

        public static byte GetByte(this XmlElement self, string name)
        {
            byte? value = FindByte(self, name);
            if (value == null)
                throw Exceptions.CreateException("Аттрибут '{0}' не найден.", name);

            return value.Value;
        }

        public static short GetInt16(this XmlElement self, string name)
        {
            short? value = FindInt16(self, name);
            if (value == null)
                throw Exceptions.CreateException("Аттрибут '{0}' не найден.", name);

            return value.Value;
        }

        public static ushort GetUInt16(this XmlElement self, string name)
        {
            ushort? value = FindUInt16(self, name);
            if (value == null)
                throw Exceptions.CreateException("Аттрибут '{0}' не найден.", name);

            return value.Value;
        }

        public static int GetInt32(this XmlElement self, string name)
        {
            int? value = FindInt32(self, name);
            if (value == null)
                throw Exceptions.CreateException("Аттрибут '{0}' не найден.", name);

            return value.Value;
        }

        public static uint GetUInt32(this XmlElement self, string name)
        {
            uint? value = FindUInt32(self, name);
            if (value == null)
                throw Exceptions.CreateException("Аттрибут '{0}' не найден.", name);

            return value.Value;
        }

        public static long GetInt64(this XmlElement self, string name)
        {
            long? value = FindInt64(self, name);
            if (value == null)
                throw Exceptions.CreateException("Аттрибут '{0}' не найден.", name);

            return value.Value;
        }

        public static ulong GetUInt64(this XmlElement self, string name)
        {
            ulong? value = FindUInt64(self, name);
            if (value == null)
                throw Exceptions.CreateException("Аттрибут '{0}' не найден.", name);

            return value.Value;
        }

        public static double GetDouble(this XmlElement self, string name)
        {
            double? value = FindDouble(self, name);
            if (value == null)
                throw Exceptions.CreateException("Аттрибут '{0}' не найден.", name);

            return value.Value;
        }

        public static string GetString(this XmlElement self, string name)
        {
            string value = FindString(self, name);
            if (value == null)
                throw Exceptions.CreateException("Аттрибут '{0}' не найден.", name);

            return value;
        }

        public static void SetBoolean(this XmlElement self, string name, bool value)
        {
            Exceptions.CheckArgumentNull(self, "self");
            Exceptions.CheckArgumentNullOrEmprty(name, "name");

            XmlAttribute attr = self.EnsureChildAttribute(name);
            attr.Value = Convert.ToString(value, CultureInfo.InvariantCulture);
        }

        public static void SetChar(this XmlElement self, string name, char value)
        {
            Exceptions.CheckArgumentNull(self, "self");
            Exceptions.CheckArgumentNullOrEmprty(name, "name");

            XmlAttribute attr = self.EnsureChildAttribute(name);
            attr.Value = Convert.ToString(value, CultureInfo.InvariantCulture);
        }

        public static void SetSByte(this XmlElement self, string name, sbyte value)
        {
            Exceptions.CheckArgumentNull(self, "self");
            Exceptions.CheckArgumentNullOrEmprty(name, "name");

            XmlAttribute attr = self.EnsureChildAttribute(name);
            attr.Value = Convert.ToString(value, CultureInfo.InvariantCulture);
        }

        public static void SetByte(this XmlElement self, string name, byte value)
        {
            Exceptions.CheckArgumentNull(self, "self");
            Exceptions.CheckArgumentNullOrEmprty(name, "name");

            XmlAttribute attr = self.EnsureChildAttribute(name);
            attr.Value = Convert.ToString(value, CultureInfo.InvariantCulture);
        }

        public static void SetInt16(this XmlElement self, string name, short value)
        {
            Exceptions.CheckArgumentNull(self, "self");
            Exceptions.CheckArgumentNullOrEmprty(name, "name");

            XmlAttribute attr = self.EnsureChildAttribute(name);
            attr.Value = Convert.ToString(value, CultureInfo.InvariantCulture);
        }

        public static void SetUInt16(this XmlElement self, string name, ushort value)
        {
            Exceptions.CheckArgumentNull(self, "self");
            Exceptions.CheckArgumentNullOrEmprty(name, "name");

            XmlAttribute attr = self.EnsureChildAttribute(name);
            attr.Value = Convert.ToString(value, CultureInfo.InvariantCulture);
        }

        public static void SetInt32(this XmlElement self, string name, int value)
        {
            Exceptions.CheckArgumentNull(self, "self");
            Exceptions.CheckArgumentNullOrEmprty(name, "name");

            XmlAttribute attr = self.EnsureChildAttribute(name);
            attr.Value = Convert.ToString(value, CultureInfo.InvariantCulture);
        }

        public static void SetUInt32(this XmlElement self, string name, uint value)
        {
            Exceptions.CheckArgumentNull(self, "self");
            Exceptions.CheckArgumentNullOrEmprty(name, "name");

            XmlAttribute attr = self.EnsureChildAttribute(name);
            attr.Value = Convert.ToString(value, CultureInfo.InvariantCulture);
        }

        public static void SetInt64(this XmlElement self, string name, long value)
        {
            Exceptions.CheckArgumentNull(self, "self");
            Exceptions.CheckArgumentNullOrEmprty(name, "name");

            XmlAttribute attr = self.EnsureChildAttribute(name);
            attr.Value = Convert.ToString(value, CultureInfo.InvariantCulture);
        }

        public static void SetUInt64(this XmlElement self, string name, ulong value)
        {
            Exceptions.CheckArgumentNull(self, "self");
            Exceptions.CheckArgumentNullOrEmprty(name, "name");

            XmlAttribute attr = self.EnsureChildAttribute(name);
            attr.Value = Convert.ToString(value, CultureInfo.InvariantCulture);
        }

        public static void SetDouble(this XmlElement self, string name, double value)
        {
            Exceptions.CheckArgumentNull(self, "self");
            Exceptions.CheckArgumentNullOrEmprty(name, "name");

            XmlAttribute attr = self.EnsureChildAttribute(name);
            attr.Value = Convert.ToString(value, CultureInfo.InvariantCulture);
        }

        public static void SetString(this XmlElement self, string name, string value)
        {
            Exceptions.CheckArgumentNull(self, "self");
            Exceptions.CheckArgumentNullOrEmprty(name, "name");

            XmlAttribute attr = self.EnsureChildAttribute(name);
            attr.Value = value;
        }
    }
}