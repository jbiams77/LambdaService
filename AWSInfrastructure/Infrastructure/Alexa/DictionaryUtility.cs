using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Text;
using Infrastructure.Logger;

namespace Infrastructure.Alexa
{
    // Utiltiy class used to convert between SessionAttribute object and dictionary
    public static class DictionaryUtility
    {
        public static IDictionary<string, object> AsDictionary(this object source)
        {
            return source.ToDictionary<object>();
        }

        public static IDictionary<string, T> ToDictionary<T>(this object source)
        {
            if (source == null) ThrowExceptionWhenSourceArgumentIsNull();

            var dictionary = new Dictionary<string, T>();
            foreach (PropertyDescriptor property in TypeDescriptor.GetProperties(source))
            {
                try
                {
                    object value = property.GetValue(source);
                    if (IsOfType<T>(value))
                    {
                        dictionary.Add(property.Name, (T)value);
                    }
                }
                catch (Exception e)
                {
                    //log.WARN("SessionAttributes", "ToDictionary", "Failed to convert SessionAttributes to dictionary. " + e.Message);
                }
            }
            return dictionary;
        }

        private static bool IsOfType<T>(object value)
        {
            return value is T;
        }

        private static void ThrowExceptionWhenSourceArgumentIsNull()
        {
            throw new NullReferenceException("Unable to convert anonymous object to a dictionary. The source anonymous object is null.");
        }
    }
}
