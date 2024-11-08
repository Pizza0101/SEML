
using System;
using System.Text;
using System.Collections.Generic;
using System.Linq;

namespace SpaceEngineers.UWBlockPrograms.Misc
{
    public static class Seml
    {
        public enum E
        {
            InvalidLine,
            IndentNonTab,
            IndentWrongIncrease,
            ListItemMissing,
            SemlValueInvalid,
            QuoteUnclosed
        }

        const int TAB = 4;

        /// <returns>
        /// 'Dictionary<string, object>' or 'List<object>'
        /// The values in the Dictionary or the List can again be 'Dictionary<string, object>' or 'List<object>',
        /// or string, double or bool.
        /// </returns>
        public static object ParseSemlString(string data)
        {
            if (string.IsNullOrEmpty(data))
                return null;
            string[] lines = data.Split(new[] { "\r\n", "\n" }, StringSplitOptions.None);
            int startIndex = 0;
            
            return Parse(lines, ref startIndex, 0);
        }

        public static object GetValue(Dictionary<string, object> data, params string[] keyPath)
        {
            object current = data;

            foreach (var key in keyPath)
            {
                var dict = current as Dictionary<string, object>;
                if (dict != null && dict.TryGetValue(key, out current))
                    continue;
                    
                return null;
            }

            return current;
        }

        public static double GetDouble(object value, double defaultValue = 0.0f)
        {
            double result;
            TryGetDouble(value, out result, defaultValue);
            return result;
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

        static object Parse(string[] lines, ref int index, int indent)
        {
            Dictionary<string, object> currentDict = null;
            List<object> currentList = null;

            while (index < lines.Length)
            {
                string trimmedLine;
                bool blockEnd;
                bool skipLine;
                InitLine(lines, index, indent, out trimmedLine, out skipLine, out blockEnd);
                
                if (skipLine)
                {
                    index++;
                    continue;
                }

                if (blockEnd)
                    return (object)currentDict ?? currentList;

                // Handle list
                object listItem;
                if (currentDict == null && TryParseListItem(lines, ref index, trimmedLine, indent, out listItem))
                {
                    if (currentList == null)
                        currentList = new List<object>();
                    currentList.Add(listItem);
                    index++;
                    continue;
                }

                // Handle dict
                KeyValuePair<string, object> kvp;
                if (currentList == null && TryParseKeyValuePair(lines, ref index, trimmedLine, indent, out kvp))
                {
                    if (currentDict == null)
                        currentDict = new Dictionary<string, object>();
                    currentDict.Add(kvp.Key, kvp.Value);
                    index++;
                    continue;
                }

                // throw when a line is neither a key, nor a key-value-pair
                throw new Exception($"{E.InvalidLine}: Line {index + 1}: '{lines[index]}' is not valid.");
            }

            return (object)currentDict ?? currentList;
        }

        static void InitLine(string[] lines, int index, int currentIndent, out string trimmedLine, out bool skipLine, out bool blockEnd)
        {
            string origLine = lines[index];
            int lineIndent = origLine.TakeWhile(char.IsWhiteSpace).Count();

            if (lineIndent % TAB != 0)
                throw new Exception($"{E.IndentNonTab}: Wrong indent on line {index + 1} - '{origLine}'. Indent was {lineIndent}, but must be a multiple of {TAB}");
            if (lineIndent > currentIndent)
                throw new Exception($"{E.IndentWrongIncrease}: Wrong indent on line {index + 1} - '{origLine}'. Increasing indent is only allowed by {TAB} and after a line that contains a key only (text ending with a ':'. Expected indent was {currentIndent} but was {lineIndent}.");
            
            skipLine = false;
            blockEnd = false;
            trimmedLine = "";

            trimmedLine = origLine.Trim();

            if (string.IsNullOrWhiteSpace(trimmedLine) || trimmedLine.StartsWith("#"))
            {
                skipLine = true;
                return;
            }

            if (lineIndent < currentIndent)
            {
                blockEnd = true;
                return;
            }
        }

        static bool TryParseKeyValuePair(string[] lines, ref int index, string trimmedLine, int currentIndent, out KeyValuePair<string, object> kvp)
        {   
            if (trimmedLine.StartsWith("-"))
            {
                kvp = new KeyValuePair<string, object>();
                return false;
            }    

            if (HasKeyValueColon(trimmedLine))
            {
                var split = trimmedLine.Split(new[] { ':' }, 2);
                
                string currentKey = split[0].Trim();
                string currentValue = split.Length > 1 ? string.Join(":", split.Skip(1)).Trim() : null;
                
                // No value? Go deeper.
                if (string.IsNullOrEmpty(currentValue))
                {
                    index++;
                    kvp = new KeyValuePair<string, object>(currentKey, Parse(lines, ref index, currentIndent + TAB));
                    index--;
                }
                // Has value
                else
                {
                    kvp = new KeyValuePair<string, object>(currentKey, ParseSemlValue(currentValue, ref index, lines));
                }

                return true;
            }

            kvp = new KeyValuePair<string, object>();
            return false;
        }

        static bool TryParseListItem(string[] lines, ref int index, string trimmedLine, int currentIndent, out object listItem)
        {
            if (!trimmedLine.StartsWith("-"))
            {
                listItem = null;
                return false;
            }

            string listItemLine = trimmedLine.Substring(1).Trim();

            if (string.IsNullOrEmpty(listItemLine))
            {
                throw new Exception($"{E.ListItemMissing}: Line {index + 1}: List item indicator (-) without list item");
            }

            KeyValuePair<string, object> kvp;
            if (TryParseKeyValuePair(lines, ref index, listItemLine, currentIndent, out kvp))
            {
                var dict = new Dictionary<string, object>
                {
                    { kvp.Key, kvp.Value }
                };
                listItem = dict;
            }
            else
            {
                listItem = ParseSemlValue(listItemLine, ref index, lines);
            }

            return true;
        }

        static object ParseSemlValue(string value, ref int index, string[] lines)
        {
            // Multi line quoted strings (start with ")
            if (value.StartsWith("\"") && !value.EndsWith("\""))
            {
                return ParseMultiLineString(value, ref index, lines);
            }

            // Single-line string enclosed in quotes
            if (value.StartsWith("\"") && value.EndsWith("\""))
            {
                return value.Substring(1, value.Length - 2);
            }

            // Other types: double, bool
            double doubleValue;
            bool boolValue;
            if (double.TryParse(value, System.Globalization.NumberStyles.Any, System.Globalization.CultureInfo.InvariantCulture, out doubleValue)) return doubleValue;
            if (bool.TryParse(value, out boolValue)) return boolValue;
            
            throw new Exception($"{E.SemlValueInvalid}: Line {index + 1}: {value} is not a valid SEML value.");
        }

        static string ParseMultiLineString(string value, ref int index, string[] lines)
        {
            char quoteChar = value[0];
            var multiLineText = new StringBuilder(value.Substring(1) + "\n"); // Start after the initial quote
            int firstLineIndex = index;

            // Read subsequent lines until the closing quote is found
            while (++index < lines.Length)
            {
                string nextLine = lines[index];
                multiLineText.Append(nextLine + "\n");

                if (nextLine.EndsWith(quoteChar.ToString()))
                {
                    return multiLineText.ToString().TrimEnd('\n', quoteChar);
                }
            }

            throw new Exception($"{E.QuoteUnclosed}: Unclosed quote starting at line {firstLineIndex + 1}");
        }

        static bool HasKeyValueColon(string line)
        {
            for (int i = 0; i < line.Length; ++i)
            {
                if (line[i] == ':')
                {
                    return true;
                }
                else if (line[i] == '"')
                {
                    return false;
                }
            }

            return false;
        }
    }
}
