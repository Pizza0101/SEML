
using System.Collections.Generic;

namespace SpaceEngineers.UWBlockPrograms.Seml
{
    public static class SemlHN
    {
        public static bool TryGetFloat(Dictionary<string, object> dict, string key, out float value, float defaultValue = 0.0f)
        {
            value = defaultValue;

            if (dict == null)
                return false;

            double dval;
            if (!SemlHC.TryGetDouble(dict, key, out dval, defaultValue))
                return false;

            value = (float)dval;
            return true;
        }

        public static bool TryGetInt(Dictionary<string, object> dict, string key, out int value, int defaultValue = 0)
        {
            value = defaultValue;

            if (dict == null)
                return false;

            double dval;
            if (!SemlHC.TryGetDouble(dict, key, out dval, defaultValue))
                return false;

            value = (int)dval;
            return true;
        }

    }
}
