using System;
using System.IO;
using System.Text;
using System.Collections.Generic;
using System.Diagnostics;

using static Json.JSONImpl;

namespace Json {
    public abstract class JToken {
        public abstract void ToString(StringBuilder sb, int indent, out bool nl);

        public override string ToString() {
            var sb = new StringBuilder();
            bool _;
            ToString(sb, 0, out _); 
            return sb.ToString();
        }

        public virtual JToken this[object key] {
            get { throw new NotImplementedException(); }
            set { throw new NotImplementedException(); }
        }

        protected static string Pad(int indent) {
            if (indent == 0) return string.Empty;
            else if (indent == 1) return "  ";
            else if (indent == 2) return "    ";
            else if (indent == 3) return "      ";
            else if (indent == 4) return "        ";
            else if (indent == 5) return "          ";
            else if (indent == 6) return "            ";
            else if (indent == 7) return "              ";
            else return string.Empty.PadRight(indent * 2);
        }

        public static explicit operator bool(JToken v) => (bool)((JValue)v).Value;
        public static explicit operator JToken(bool v) => new JValue(v);
    }

    public class JObject : JToken {
        List<JPair> _pairs = new List<JPair>();

        public void Add(string key, JToken token) {
            if (Contains(key) == true) {
                return; // @TODO error
            }
            _pairs.Add(new JPair(key, token));
        }

        public bool Contains(string key) {
            foreach (var pair in _pairs) {
                if (pair.Key.Equals(key, StringComparison.Ordinal) == true) {
                    return true;
                }
            }
            return false;
        }


        public JToken this[string key] {
            get {
                foreach (var pair in _pairs) {
                    if (pair.Key.Equals(key, StringComparison.Ordinal) == true) {
                        return pair.Value;
                    }
                }
                return null;
            }
            set {
                foreach (var pair in _pairs) {
                    if (pair.Key.Equals(key, StringComparison.Ordinal) == true) {
                        pair.Value = value;
                        return;
                    }
                }
                Add(key, value);
            }
        }

        public override JToken this[object key] {
            get {
                foreach (var pair in _pairs) {
                    if (pair.Key.Equals((string)key, StringComparison.Ordinal) == true) {
                        return pair.Value;
                    }
                }
                return null;
            }
            set {
                foreach (var pair in _pairs) {
                    if (pair.Key.Equals((string)key, StringComparison.Ordinal) == true) {
                        pair.Value = value;
                        return;
                    }
                }
                Add((string)key, value);
            }
        }

        public override void ToString(StringBuilder sb, int indent, out bool nl) {
            sb.Append("{");
            indent += 1;
            bool writePad = false;
            for (int i = 0; i < _pairs.Count; i++) {
                if (i == 0) {
                    sb.AppendLine();
                    writePad = true;
                }

                sb.Append(Pad(indent));
                bool newLine;
                _pairs[i].ToString(sb, indent, out newLine);
                if (i + 1 < _pairs.Count) {
                    sb.Append(",");
                    sb.AppendLine();
                }
                else {
                    if (newLine) sb.AppendLine();
                }
            }
            indent -= 1;
            if (writePad) sb.Append(Pad(indent));
            sb.Append("}");
            nl = true;
        }

        public static JObject Parse(string json) {
            using (var r = new StringReader(json)) {
                var sb = new StringBuilder();
                return (JObject)Tokenizer(r, sb);
            }
        }

        public static JObject Parse(Stream stream) {
            var r = new StreamReader(stream);
            var sb = new StringBuilder();
            return (JObject)Tokenizer(r, sb);
        }
    }

    public class JArray : JToken {
        List<JToken> _elements = new List<JToken>();

        public void Add(JToken element) {
            _elements.Add(element);
        }

        public int Count => _elements.Count;

        public JToken this[int i] {
            get {
                return _elements[i];
            }
            set {
                _elements[i] = value;
            }
        }

        public override JToken this[object i] {
            get {
                return _elements[(int)i];
            }
            set {
                _elements[(int)i] = value;
            }
        }

        public override void ToString(StringBuilder sb, int indent, out bool nl) {
            sb.Append("[");
            indent += 1;
            bool writePad = false;
            for (int i = 0; i < _elements.Count; i++) {
                if (i == 0) {
                    sb.AppendLine();
                    writePad = true;
                }

                sb.Append(Pad(indent));
                bool newLine;
                _elements[i].ToString(sb, indent, out newLine);
                if (i + 1 < _elements.Count) {
                    sb.Append(",");
                    sb.AppendLine();
                }
                else {
                    if (newLine) sb.AppendLine();
                }
            }
            indent -= 1;
            if (writePad) sb.Append(Pad(indent));
            sb.Append("]");
            nl = true;
        }

        public static JArray Parse(string json) {
            using (var r = new StringReader(json)) {
                var sb = new StringBuilder();
                return (JArray)Tokenizer(r, sb);
            }
        }

        public static JArray Parse(Stream stream) {
            var r = new StreamReader(stream);
            var sb = new StringBuilder();
            return (JArray)Tokenizer(r, sb);
        }
    }

    public class JPair : JToken {
        public JPair(string key, JToken value) {
            Key = key;
            Value = value;
        }

        public string Key;
        public JToken Value;

        public override void ToString(StringBuilder sb, int indent, out bool nl) {
            sb.Append($"\"{Key}\": ");
            Value.ToString(sb, indent, out nl);
        }
    }

    public class JValue : JToken {
        public JValue(string value) {
            Value = value;
        }
        public JValue(bool value) {
            Value = value;
        }
        public JValue(object value) {
            Value = value;
        }

        public object Value;

        public override void ToString(StringBuilder sb, int indent, out bool nl) {
            if (Value == null) {
                sb.Append($"null");
            }
            if (Value is string) {
                sb.Append($"\"{Value}\"");
            }
            else if (Value is int) {
                sb.Append(Value);
            }
            else if (Value is double) {
                sb.Append(Value);
            }
            else if (Value is bool) {
                if ((bool)Value) {
                    sb.Append("true");
                }
                else {
                    sb.Append("false");
                }
            }
            nl = true;
        }

        public static explicit operator double(JValue v) => (double)v.Value;
        public static explicit operator JValue(double v) => new JValue(v);

        public static explicit operator string(JValue v) => (string)v.Value;
        public static explicit operator JValue(string v) => new JValue(v);

        public static explicit operator bool(JValue v) => (bool)v.Value;
        public static explicit operator JValue(bool v) => new JValue(v);
    }

    static class JSONImpl {
        // Caller is responsible for disposing the TextReader.
        // TextReader must support seeking (Peek)
        public static JToken Tokenizer(TextReader reader, StringBuilder sb) {
            WS(reader);

            int c = reader.Peek();
            while (c != -1) {
                switch (c) {
                    case '{': {
                        return TObject(reader, sb); 
                    }
                    case '[': {
                        return TArray(reader, sb);
                    }
                    case '"': {
                        TString(reader, sb);
                        return new JValue(sb.ToString());
                    }
                    case 't': {
                        c = reader.Read();
                        c = reader.Read();
                        if (c != (int)'r') return null; // @TODO ERROR
                        c = reader.Read();
                        if (c != (int)'u') return null; // @TODO ERROR
                        c = reader.Read();
                        if (c != (int)'e') return null; // @TODO ERROR
                        return new JValue(true);
                    }
                    case 'f': {
                        c = reader.Read();
                        c = reader.Read();
                        if (c != (int)'a') return null;
                        c = reader.Read();
                        if (c != (int)'l') return null;
                        c = reader.Read();
                        if (c != (int)'s') return null;
                        c = reader.Read();
                        if (c != (int)'e') return null;
                        return new JValue(false);
                    }
                    case 'n': {
                        c = reader.Read();
                        c = reader.Read();
                        if (c != (int)'u') return null;
                        c = reader.Read();
                        if (c != (int)'l') return null;
                        c = reader.Read();
                        if (c != (int)'l') return null;
                        return new JValue(null);
                    }
                    case '-':
                    case '0': case '1': case '2': case '3': case '4': case '5': case '6': case '7': case '8': case '9': {
                        return new JValue(TNumber(reader, sb));
                    }
                    case '/': {
                        Comment(reader);
                    }
                    break;
                }
                WS(reader);
                c = reader.Read();
            }

            return null;
        }

        public static void WS(TextReader reader) {
            for (;;) {
                int c = reader.Peek();
                if (c == (int)' ' || c == (int)'\n' || c == (int)'\r') {
                    c = reader.Read();
                }
                else if (c == '/') {
                    Comment(reader);
                }
                else {
                    break;
                }
            }

        }

        static void Comment(TextReader reader) {
            reader.Read();
            int c = reader.Peek();
            if (c == (int)'/') {
                reader.Read();
                while (true) {
                    c = reader.Read();
                    if (c == -1) break;
                    if (c == '\r' || c == '\n') {
                        if (c == '\r' && reader.Peek() == '\n') reader.Read();
                        break;
                    }
                }
            }
            else if (c == (int)'*') {
                reader.Read();
                while (true) {
                    c = reader.Read();
                    if (c == -1) break;
                    if (c == '*') {
                        if (reader.Peek() == '/') {
                            reader.Read();
                            break;
                        }
                    }
                }
            }
            else return; // @TODO ERROR
        }

        static object TNumber(TextReader reader, StringBuilder sb) {
            sb.Clear();

            bool isFrac = false;

            int c;
            do {
                c = reader.Read();
                sb.Append((char)c);
                c = reader.Peek();
                if (c == (int)'E' || c == (int)'e' || c == (int)'.') {
                    isFrac = true;
                }
            } while ((c >= (int)'0' && c <= (int)'9') ||
                     c == (int)'e' || c == (int)'E' || c == (int)'-' || c == (int)'+' || c == (int)'.');

            if (isFrac) {
                return double.Parse(sb.ToString());
            }
            else {
                return int.Parse(sb.ToString());
            }
        }

        // members
        //   pair
        //   pair , members
        // pair
        //   string : value
        public static JObject TObject(TextReader reader, StringBuilder sb) {
            var obj = new JObject();

            int c = reader.Read(); // {

            WS(reader);
            c = reader.Peek();
            while (c != -1 && c != (int)'}') {
                WS(reader);
                TString(reader, sb);
                string key = sb.ToString();

                WS(reader);

                c = reader.Read();
                if (c != (int)':') {
                    Console.WriteLine($"ERROR: {(char)c}");
                    return null; // @TODO error
                }

                WS(reader);

                // This should run the Tokenizer
                var value = Tokenizer(reader, sb);

                obj.Add(key, value);

                WS(reader);
                c = reader.Peek();

                if (c == (int)',') {
                    c = reader.Read();
                }
            }

            reader.Read(); // }

            return obj;
        }

        static JArray TArray(TextReader reader, StringBuilder sb) {
            var arr = new JArray();

            int c = reader.Read(); // [

            WS(reader);
            c = reader.Peek();
            while (c != -1 && c != (int)']') {
                WS(reader);
                var element = Tokenizer(reader, sb);
                arr.Add(element);
                WS(reader);
                c = reader.Peek();

                if (c == (int)',') {
                    c = reader.Read();
                }
            }

            reader.Read(); // ]

            return arr;
        }

        public static void TString(TextReader reader, StringBuilder sb) {
            sb.Clear();

            int c = reader.Peek();
            if (c != (int)'"') {
                Console.WriteLine($"ERROR2 '{(char)c}'");
                return;
            }

            reader.Read(); // eat the "
            c = reader.Read();
            while (c != -1 && c != (int)'"') {
                sb.Append((char)c);
                c = reader.Read();
            }
        }

    }
}
