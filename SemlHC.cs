
using System;
using System.Collections.Generic;

namespace SpaceEngineers.UWBlockPrograms.Seml
{
    public static class SemlHC
    {
        public static bool TryGetString(Dictionary<string, object> dict, string key, out string value, string defaultValue=null)
        {            
            value = defaultValue;
            if (dict == null)
                return false;

            object obj;
            if (!dict.TryGetValue(key, out obj))
            {
                return false;
            }
                
            string strValue = obj as string;
            if (string.IsNullOrEmpty(strValue))
                return false;

            value = strValue;
            return true;
        }

        public static bool TryGetEnum<TE>(Dictionary<string, object> dict, string key, out TE value, TE defaultValue) where TE : struct
        {
            value = defaultValue;

            if (dict == null)
                return false;

            string strValue;
            if (!TryGetString(dict, key, out strValue))
                return false;

            TE result;
            if (Enum.TryParse(strValue, true, out result))
            {
                value = result;
                return true;
            }

            return false;
        }

        public static bool TryGetDouble(object value, out double result, double defaultValue = 0.0)
        {
            var d = value as double?;
            if (d.HasValue)
            {
                result = d.Value;
                return true;
            }

            result = defaultValue;
            return false;
        }

        public static bool TryGetDouble(Dictionary<string, object> dict, string key, out double value, double defaultValue = 0.0)
        {
            object obj;
            value = defaultValue;

            if (dict == null)
                return false;

            if (!dict.TryGetValue(key, out obj))
            {
                return false;
            }

            return TryGetDouble(obj, out value, defaultValue);
        }

        public static bool TryGetBool(Dictionary<string, object> dict, string key, out bool value, bool defaultValue = false)
        {
            value = defaultValue;

            if (dict == null)
                return false;

            object obj;
            if (!dict.TryGetValue(key, out obj))
            {
                return false;
            }

            var b = obj as bool?;
            if (!b.HasValue)
                return false;

            value = b.Value;
            return true;
        }
    
        public static bool TryGetList(Dictionary<string, object> dict, string key, out List<object> value)
        {
            value = null;

            if (dict == null)
                return false;

            object obj;           
            if (!dict.TryGetValue(key, out obj))
            {
                return false;
            }

            value = obj as List<object>;
            if (value == null)
                return false;

            return true;
        }
    }
}
