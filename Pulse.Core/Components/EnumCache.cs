using System;
using System.Linq;

namespace Pulse.Core
{
    public static class EnumCache<T> where T : struct
    {
        public static readonly T[] Values = Enum.GetValues(TypeCache<T>.Type).Cast<T>().ToArray();
        public static readonly string[] Names = Enum.GetNames(TypeCache<T>.Type);

        public static int Count => Values.Length;

        public static bool IsDefined(T value)
        {
            return Values.Contains(value);
        }

        public static T? TryParse(string name, StringComparison nameComparison = StringComparison.InvariantCultureIgnoreCase)
        {
            for (int i = 0; i < Count; i++)
            {
                if (string.Equals(Names[i], name, nameComparison))
                    return Values[i];
            }

            return null;
        }

        public static T Parse(string name, T defaulValue, StringComparison nameComparison = StringComparison.InvariantCultureIgnoreCase)
        {
            return TryParse(name, nameComparison) ?? defaulValue;
        }

        public static T Parse(string name, StringComparison nameComparison = StringComparison.InvariantCultureIgnoreCase)
        {
            T? result = TryParse(name, nameComparison);
            if (result == null)
                throw Exceptions.CreateException("Тег '{0}' não especificado para transferência '{1}'.", name, TypeCache<T>.Type);
            return result.Value;
        }
    }
}
