using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data.Linq.Mapping;
using System.IO;
using System.Linq;
using System.Reflection;

namespace Heathmill.WpfUtilities
{
    public static class ReflectionHelper
    {
        public static Dictionary<string, object> GetPropertyValuesWithAttribute<T>
            (this object source) where T : Attribute
        {
            var props = GetPropertiesWithAttribute<T>(source.GetType());
            return props.ToDictionary(p => p.Name, p => p.GetValue(source, null));
        }

        public static PropertyInfo[] GetPropertiesWithAttribute<T>(this Type sourcetype)
            where T : Attribute
        {
            var props = sourcetype.GetProperties();
            return props.Where(p => p.HasAttribute<T>()).ToArray();
        }

        public static bool HasAttribute<T>(this PropertyInfo propinfo) where T : Attribute
        {
            return propinfo.GetCustomAttributes(true).OfType<T>().Any();
        }

        public static bool HasAttribute<T>(this PropertyDescriptor propdesc) where T : Attribute
        {
            return propdesc.Attributes.OfType<T>().Any();
        }

        public static T GetAttribute<T>(this PropertyInfo propinfo) where T : Attribute
        {
            return propinfo.GetCustomAttributes(true).OfType<T>().FirstOrDefault();
        }

        public static T GetAttribute<T>(this PropertyDescriptor propdesc) where T : Attribute
        {
            return propdesc.Attributes.OfType<T>().FirstOrDefault();
        }

        public static T[] GetAttributes<T>(this PropertyInfo propinfo) where T : Attribute
        {
            return propinfo.GetCustomAttributes(true).OfType<T>().ToArray();
        }

        public static T[] GetAttributes<T>(this PropertyDescriptor propdesc) where T : Attribute
        {
            return propdesc.Attributes.OfType<T>().ToArray();
        }

        public static PropertyInfo ResolveSettableProperty(this Type t, string propname)
        {
            PropertyInfo prop = ResolveProperty(t, propname);
            if (!prop.CanSet())
            {
                string format = "Type {0} property {1} cannot be set";
                string message = string.Format(format, t.Name, propname);
                throw new ApplicationException(message);
            }
            return prop;
        }

        public static PropertyInfo ResolveReadableProperty(this Type t, string propname)
        {
            PropertyInfo prop = ResolveProperty(t, propname);
            if (!prop.CanRead)
            {
                string format = "Type {0} property {1} cannot be read";
                string message = string.Format(format, t.Name, propname);
                throw new ApplicationException(message);
            }
            return prop;
        }

        public static PropertyInfo ResolveProperty(this Type t, string propname)
        {
            PropertyInfo prop = t.GetProperty(propname);
            if (prop == null)
            {
                string format = "Type {0} does not contain property {1}";
                string message = string.Format(format, t.Name, propname);
                throw new ApplicationException(message);
            }
            return prop;
        }

        public static bool IsNullableOrValue<T>(this Type type)
        {
            if (!typeof (T).IsValueType) return false;
            return (type == typeof (T) || Nullable.GetUnderlyingType(type) == typeof (T));
        }

        public static bool IsNullable(this Type type)
        {
            if (!type.IsValueType) return true;
            if (Nullable.GetUnderlyingType(type) != null) return true;
            return false;
        }

        public static bool CanSet(this PropertyInfo propinfo)
        {
            return propinfo.CanWrite &&
                   (from a in propinfo.GetAccessors(true)
                       where a.Name.StartsWith("set")
                       select a.Name).Any();
        }

        public static void SetPropertyFromString(
            this object instance,
            string propname,
            string value)
        {
            PropertyInfo propinfo = instance.GetType().GetProperty(propname);
            if (propinfo == null) throw new ArgumentException("Unknown property " + propname);
            Type type = propinfo.PropertyType;
            bool isnullable =
                type.IsGenericType && type.GetGenericTypeDefinition() == typeof (Nullable<>);
            if (isnullable)
            {
                if (string.IsNullOrWhiteSpace(value))
                {
                    propinfo.SetValue(instance, null, null);
                    return;
                }
                type = Nullable.GetUnderlyingType(type);
            }
            TypeCode typecode = Type.GetTypeCode(type);
            object setvalue;
            switch (typecode)
            {
                case TypeCode.Boolean:
                    int i;
                    if (int.TryParse(value, out i)) setvalue = i != 0;
                    else setvalue = value.IgnoreCaseCompare("true");
                    break;
                case TypeCode.Int32:
                    setvalue = int.Parse(value);
                    break;
                case TypeCode.Double:
                    setvalue = double.Parse(value);
                    break;
                case TypeCode.Single:
                    setvalue = Single.Parse(value);
                    break;
                case TypeCode.DateTime:
                    setvalue = DateTime.Parse(value);
                    break;
                case TypeCode.String:
                    setvalue = value;
                    break;
                default:
                    throw new NotSupportedException(type.Name);
            }
            propinfo.SetValue(instance, setvalue, null);
        }

        public static Dictionary<string, object> DtoToDictionary(object dto)
        {
            return GetPropertyValuesWithAttribute<ColumnAttribute>(dto);
        }

        public static string ApplicationFileName()
        {
            Assembly parent = Assembly.GetEntryAssembly() ?? Assembly.GetCallingAssembly();

            if (parent.CodeBase.StartsWith("http://"))
                throw new IOException("Deployed from URL");

            if (File.Exists(parent.Location))
                return parent.Location;

            string dir = AppDomain.CurrentDomain.BaseDirectory;
            if (File.Exists(dir + AppDomain.CurrentDomain.FriendlyName))
                return dir + AppDomain.CurrentDomain.FriendlyName;
            if (File.Exists(Assembly.GetExecutingAssembly().Location))
                return Assembly.GetExecutingAssembly().Location;

            throw new IOException("Assembly not found");
        }

        public static Tuple<string, string> GetProductAndVersion()
        {
            var callingAssembly = Assembly.GetCallingAssembly();

            // Get the product name from the AssemblyProductAttribute.
            //   Usually defined in AssemblyInfo.cs as: [assembly: AssemblyProduct("Hello World Product")]
            var assemblyProductAttribute =
                ((AssemblyProductAttribute[])
                    callingAssembly.GetCustomAttributes(typeof (AssemblyProductAttribute), false))
                    .Single();
            string productName = assemblyProductAttribute.Product;

            // Get the product version from the assembly by using its AssemblyName.
            string productVersion = new AssemblyName(callingAssembly.FullName).Version.ToString();

            return new Tuple<string, string>(productName, productVersion);
        }

        public static string GetAssemblyTitle()
        {
            var assembly = Assembly.GetCallingAssembly();
            var a =
                ((AssemblyTitleAttribute[])
                    assembly.GetCustomAttributes(typeof (AssemblyTitleAttribute), false)).Single();
            return a.Title;
        }
    }
}
