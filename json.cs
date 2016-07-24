using System;
using System.IO;
using System.Text;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;

using static Json.JSONImpl;

namespace Json {
    public enum JType {
        None,
        Object,
        Array,
        Pair,
        Null,
        String,
        Integer,
        Double,
        Boolean,
        DateTime
    }

    public abstract class JToken {
        JType _type;

        protected JToken(JType type) {
            _type = type;
        }

        public JType Type => _type;

        public string ToString(bool ws) {
            var sb = new StringBuilder();
            using (var writer = new StringWriter(sb)) {
                WriteJson(writer, this, ws); 
                return sb.ToString();
            }
        }

        public override string ToString() {
            var sb = new StringBuilder();
            using (var writer = new StringWriter(sb)) {
                WriteJson(writer, this, true); 
                return sb.ToString();
            }
        }

        public virtual JToken this[object key] {
            get { throw new NotImplementedException(); }
            set { throw new NotImplementedException(); }
        }

        public static explicit operator bool(JToken v) => (bool)((JValue)v).Value;

        public static implicit operator JToken(string v) => new JValue(v);
        public static implicit operator JToken(bool v) => new JValue(v);
        public static implicit operator JToken(int v) => new JValue(v);
        public static implicit operator JToken(double v) => new JValue(v);
        public static implicit operator JToken(DateTime v) => new JValue(v);
    }

    public class JObject : JToken, IEnumerable<JPair> {
        List<JPair> _pairs = new List<JPair>();

        public JObject() : base(JType.Object) {
        }

        public int Count => _pairs.Count;

        public void Add(string key, JToken token) {
            if (ContainsKey(key) == true) {
                throw new ArgumentException("An JPair with the same key already exists in the JObject");
            }
            if (token == null) {
                token = new JValue();
            }
            _pairs.Add(new JPair(key, token));
        }

        public bool TryGetValue(string key, out JToken value) {
            foreach (var pair in _pairs) {
                if (pair.Key.Equals(key, StringComparison.Ordinal) == true) {
                    value = pair.Value;
                    return true;
                }
            }
            value = null;
            return false;
        }

        public bool ContainsKey(string key) {
            foreach (var pair in _pairs) {
                if (pair.Key.Equals(key, StringComparison.Ordinal) == true) {
                    return true;
                }
            }
            return false;
        }

        public bool Remove(string key) {
            for (int i = 0; i < _pairs.Count; i++) {
                if (_pairs[i].Key.Equals(key, StringComparison.Ordinal) == true) {
                    _pairs.RemoveAt(i);
                    return true;
                }
            }
            return false;
        }

        public JPair this[int i] {
            get {
                return _pairs[i];
            }
            set {
                _pairs[i] = value;
            }
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

        public void Write(Stream stream, bool ws = false) {
            var sw = new StreamWriter(stream);
            WriteJson(sw, this, ws);
            sw.Flush();
        }

        public IEnumerator<JPair> GetEnumerator() => _pairs.GetEnumerator();
        IEnumerator IEnumerable.GetEnumerator() => _pairs.GetEnumerator();

        public static Result<JObject> Parse(string json) {
            using (var r = new StringReader(json)) {
                var sb = new StringBuilder();
                var res = ParseJSON(r, sb);
                if (res.Failed) return res.ErrorCode;
                if (res.Value is JObject == false) {
                    return JError.WasNotExpectedResultTypeJObject;
                }
                return (JObject)res.Value;
            }
        }

        public static Result<JObject> Parse(Stream stream) {
            var r = new StreamReader(stream);
            var sb = new StringBuilder();
            var res = ParseJSON(r, sb);
            if (res.Failed) return res.ErrorCode;
            if (res.Value is JObject == false) {
                return JError.WasNotExpectedResultTypeJObject;
            }
            return (JObject)res.Value;
        }
    }

    public class JArray : JToken, IEnumerable<JToken> {
        public JArray() : base(JType.Array) {
        }

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

        public IEnumerator<JToken> GetEnumerator() => _elements.GetEnumerator();
        IEnumerator IEnumerable.GetEnumerator() => _elements.GetEnumerator();

        public void Write(Stream stream, bool ws = false) {
            var sw = new StreamWriter(stream);
            WriteJson(sw, this, ws);
        }

        public static Result<JArray> Parse(string json) {
            using (var r = new StringReader(json)) {
                var sb = new StringBuilder();
                var res = ParseJSON(r, sb);
                if (res.Failed) return res.ErrorCode;
                if (res.Value is JArray == false) {
                    return JError.WasNotExpectedResultTypeJArray;
                }
                return (JArray)res.Value;
            }
        }

        public static Result<JArray> Parse(Stream stream) {
            var r = new StreamReader(stream);
            var sb = new StringBuilder();
            var res = ParseJSON(r, sb);
            if (res.Failed) return res.ErrorCode;
            if (res.Value is JArray == false) {
                return JError.WasNotExpectedResultTypeJArray;
            }
            return (JArray)res.Value;
        }
    }

    public class JPair : JToken {
        public JPair(string key, JToken value) : base(JType.Pair) {
            Key = key;
            Value = value;
        }

        public string Key;
        public JToken Value;
    }

    public class JValue : JToken {
        public JValue() : base(JType.Null) {
            Value = null;
        }
        public JValue(string value) : base(value == null ? JType.Null : JType.String) {
            Value = value;
        }
        public JValue(bool value) : base(JType.Boolean) {
            Value = value;
        }
        public JValue(int value) : base(JType.Integer) {
            Value = value;
        }
        public JValue(double value) : base(JType.Double) {
            Value = value;
        }
        public JValue(DateTime value) : base(JType.DateTime) {
            Value = value;
        }

        public object Value;

        public static explicit operator double(JValue v) => (double)v.Value;
        public static explicit operator JValue(double v) => new JValue(v);

        public static explicit operator string(JValue v) => (string)v.Value;
        public static explicit operator JValue(string v) => new JValue(v);

        public static explicit operator bool(JValue v) => (bool)v.Value;
        public static explicit operator JValue(bool v) => new JValue(v);
    }

    public struct Result {
        public static Result Success = new Result(0);
        int _errorCode;
        public Result(int errorCode) {
            _errorCode = errorCode;
        }

        public int ErrorCode => _errorCode;
        public bool Failed => _errorCode != 0;

        public Result<T> Ok<T>(T result) {
            return new Result<T>(result);
        }

        public void ThrowIfFailed() {
            if (Failed) {
                throw new JException(_errorCode);
            }
        }

        public static implicit operator Result(int e) {
            return new Result(e);
        }
    }

    public struct Result<T> {
        T _value;
        int _errorCode;
        public Result(T value) {
            _value = value;
            _errorCode = 0;
        }
        public Result(int errorCode) {
            _value = default(T);
            _errorCode = errorCode;
        }

        public T Value => _value;

        public int ErrorCode => _errorCode;

        public bool Failed => _errorCode != 0;

        public void ThrowIfFailed() {
            if (Failed) {
                throw new JException(_errorCode);
            }
        }

        public static implicit operator Result<T>(T value) {
            return new Result<T>(value);
        }

        public static implicit operator Result<T>(int e) {
            return new Result<T>(e);
        }
    }

    public class JException : Exception {
        int _errorCode;
        public JException(int errorCode) : base(JError.ToMessage(errorCode)) {
            _errorCode = errorCode;
        }
        public JException(string message) : base(message) {
        }
        public JException(string message, Exception inner) : base(message, inner) {
        }

        public int ErrorCode => _errorCode;
    }

    public static class JError {
        public const int ExpectedBoolTrue = 1;
        public const int ExpectedBoolFalse = 2;
        public const int ExpectedNull= 3;
        public const int MalformedJson = 4;
        public const int MalformedNumber = 5;
        public const int ExpectedJPairSepartor = 6;
        public const int UnexpectedEnd = 7;
        public const int ExpectedStringStartWithDoubleQuote = 8;
        public const int WasNotExpectedResultTypeJArray = 9;
        public const int WasNotExpectedResultTypeJObject = 10;
        public const int InvalidHextCharacter = 11;
        public const int UnknownEscapeCode = 12;

        public static string ToMessage(int errorCode) {
            switch (errorCode) {
                case ExpectedBoolTrue: return "Expected boolean token 'true'.";
                case ExpectedBoolFalse: return "Expected boolean token 'false'.";
                case ExpectedNull: return "Expected token null.";
                case MalformedJson: return "Json is malformed can not parse.";
                case MalformedNumber: return "No a valid Json number.";
                case ExpectedJPairSepartor: return "Missing expected JPair separator.";
                case WasNotExpectedResultTypeJArray: return "Was not of expected type JArray.";
                case WasNotExpectedResultTypeJObject: return "Was not of expected type JObject.";
                case InvalidHextCharacter: return "Invalid hex chracter.";
                case UnknownEscapeCode: return "Unkown escape code.";
                default: throw new NotImplementedException("Unknown error code");
            }
        }
    }

    static class JSONImpl {
        // Caller is responsible for disposing the TextReader.
        // TextReader must support seeking (Peek)
        public static Result<JToken> ParseJSON(TextReader reader, StringBuilder sb) {
            var r = WS(reader);
            if (r.Failed) return r.ErrorCode;

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
                        return TStringOrDate(reader, sb);
                    }
                    case 't': {
                        c = reader.Read();
                        c = reader.Read();
                        if (c != (int)'r') return JError.ExpectedBoolTrue;
                        c = reader.Read();
                        if (c != (int)'u') return JError.ExpectedBoolTrue;
                        c = reader.Read();
                        if (c != (int)'e') return JError.ExpectedBoolTrue;
                        return new JValue(true);
                    }
                    case 'f': {
                        c = reader.Read();
                        c = reader.Read();
                        if (c != (int)'a') return JError.ExpectedBoolFalse;
                        c = reader.Read();
                        if (c != (int)'l') return JError.ExpectedBoolFalse;
                        c = reader.Read();
                        if (c != (int)'s') return JError.ExpectedBoolFalse;
                        c = reader.Read();
                        if (c != (int)'e') return JError.ExpectedBoolFalse;
                        return new JValue(false);
                    }
                    case 'n': {
                        c = reader.Read();
                        c = reader.Read();
                        if (c != (int)'u') return JError.ExpectedNull;
                        c = reader.Read();
                        if (c != (int)'l') return JError.ExpectedNull;
                        c = reader.Read();
                        if (c != (int)'l') return JError.ExpectedNull;
                        return new JValue(null);
                    }
                    case '-':
                    case '0': case '1': case '2': case '3': case '4': case '5': case '6': case '7': case '8': case '9': {
                        return TNumber(reader, sb);
                    }
                    case '/': {
                        Comment(reader);
                    }
                    break;
                    default:
                        return JError.MalformedJson;
                }

                r = WS(reader);
                if (r.Failed) return r.ErrorCode;
                c = reader.Read();
            }

            return JError.MalformedJson;
        }

        // eat all white space and comments
        static Result WS(TextReader reader) {
            for (;;) {
                int c = reader.Peek();
                if (c == (int)' ' || c == (int)'\n' || c == (int)'\r') {
                    c = reader.Read();
                }
                else if (c == '/') {
                    var r = Comment(reader);
                    if (r.Failed == true) {
                        return r.ErrorCode;
                    }
                }
                else {
                    break;
                }
            }
            return Result.Success;
        }

        // read both C line comments and C multi line comments
        static Result Comment(TextReader reader) {
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
            else return JError.MalformedJson;

            if (c == -1) return JError.UnexpectedEnd;

            return Result.Success;
        }

        static Result<JToken> TNumber(TextReader reader, StringBuilder sb) {
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

            if (c == -1) return JError.UnexpectedEnd;

            if (isFrac) {
                double value;
                if (double.TryParse(sb.ToString(), out value) == false) {
                    return JError.MalformedNumber;
                }
                return new JValue(value);
            }
            else {
                int value;
                string s = sb.ToString();
                if (int.TryParse(s, out value) == false) {
                    // we try to fallback on the double
                    double d;
                    if (double.TryParse(s, out d) == true) {
                        return new JValue(d);
                    }
                    else {
                        return JError.MalformedNumber;
                    }
                }
                return new JValue(value);
            }
        }

        public static Result<JToken> TObject(TextReader reader, StringBuilder sb) {
            var obj = new JObject();

            int c = reader.Read(); // {

            var r = WS(reader);
            if (r.Failed) return r.ErrorCode;
            c = reader.Peek();
            while (c != -1 && c != (int)'}') {
                r = WS(reader);
                if (r.Failed) return r.ErrorCode;

                TString(reader, sb);
                if (r.Failed) return r.ErrorCode;

                string key = sb.ToString();

                r = WS(reader);
                if (r.Failed) return r.ErrorCode;

                c = reader.Read();
                if (c != (int)':') {
                    return JError.ExpectedJPairSepartor;
                }

                WS(reader);
                if (r.Failed) return r.ErrorCode;

                var value = ParseJSON(reader, sb);
                if (value.Failed == true) {
                    return value.ErrorCode;
                }

                obj.Add(key, value.Value);

                r = WS(reader);
                if (r.Failed) return r.ErrorCode;

                c = reader.Peek();

                if (c == (int)',') {
                    c = reader.Read();
                }
            }

            if (c == -1) return JError.UnexpectedEnd;

            reader.Read(); // }

            return obj;
        }

        static Result<JToken> TArray(TextReader reader, StringBuilder sb) {
            var arr = new JArray();

            int c = reader.Read(); // [

            var r = WS(reader);
            if (r.Failed) return r.ErrorCode;

            c = reader.Peek();
            while (c != -1 && c != (int)']') {
                WS(reader);
                if (r.Failed) return r.ErrorCode;
                var element = ParseJSON(reader, sb);
                if (element.Failed) return element.ErrorCode;
                arr.Add(element.Value);
                WS(reader);
                if (r.Failed) return r.ErrorCode;
                c = reader.Peek();

                if (c == (int)',') {
                    c = reader.Read();
                }
            }

            if (c == -1) return JError.UnexpectedEnd;

            reader.Read(); // ]

            return arr;
        }

        public static Result<JToken> TStringOrDate(TextReader reader, StringBuilder sb) {
            var r = TString(reader, sb);
            if (r.Failed) return r.ErrorCode;

            var str = sb.ToString();

            DateTime dt;
            if (TryParseDateTime(str, out dt) == true) {
                return new JValue(dt);
            }
            else {
                return new JValue(str);
            }
        }

        public static Result TString(TextReader reader, StringBuilder sb) {
            sb.Clear();

            int c = reader.Peek();
            if (c != (int)'"') {
                return JError.ExpectedStringStartWithDoubleQuote;
            }

            reader.Read(); // eat the "
            c = reader.Read();
            while (c != -1 && c != (int)'"') {
                // espace codes
                if (c == (int)'\\') {
                    int peek = reader.Peek();
                    switch (peek) {
                        case '"':
                            sb.Append('"');
                            reader.Read();
                        break;
                        case '\\':
                            sb.Append('\\');
                            reader.Read();
                        break;
                        case '/':
                            sb.Append('/');
                            reader.Read();
                        break;
                        case 'b':
                            sb.Append('\b');
                            reader.Read();
                        break;
                        case 'f':
                            sb.Append('\f');
                            reader.Read();
                        break;
                        case 'n':
                            sb.Append('\n');
                            reader.Read();
                        break;
                        case 'r':
                            sb.Append('\r');
                            reader.Read();
                        break;
                        case 't':
                            sb.Append('\t');
                            reader.Read();
                        break;
                        case 'u': {
                            reader.Read();
                            int value = 0;
                            for (int i = 0, j = 3; i < 4; i++, j--) {
                                c = reader.Read();
                                if (c == -1) {
                                    break;
                                }
                                int n;
                                if (c <= 57 && c >= 48) {
                                    n = c - 48;
                                }
                                else if (c <= 70 && c >= 65) {
                                    n = c - 55;
                                }
                                else if (c <= 102 && c >= 97) {
                                    n = c - 87;
                                }
                                else {
                                    return JError.InvalidHextCharacter;
                                }
                                value += n << j * 4; 
                            }
                            sb.Append((char)value);
                        }
                        break;
                        default: return JError.UnknownEscapeCode;
                    }
                }
                else {
                    sb.Append((char)c);
                    c = reader.Read();
                }
            }

            if (c == -1) return JError.UnexpectedEnd;

            return Result.Success;
        }

        public const string IsoDateFormat = "yyyy-MM-ddTHH:mm:ss.FFFFFFFK";

        static bool TryParseDateTime(string s, out DateTime dt) {
            if (s.Length > 0) {
                if (s.Length >= 19 && s.Length <= 40 && char.IsDigit(s[0]) && s[10] == 'T') {
                    if (DateTime.TryParseExact(s, IsoDateFormat, CultureInfo.InvariantCulture, DateTimeStyles.RoundtripKind, out dt)) {
                        return true;
                    }
                }
            }

            dt = default(DateTime);
            return false;
        }

        // ----

        public static void WriteJson(TextWriter w, JToken t, bool ws) {
            bool _;
            Write(w, t, ws, 0, out _);
        }

        static void Write(TextWriter w, JToken t, bool ws, int indent, out bool nl) {
            nl = false;
            switch (t.Type) {
                case JType.Object:
                    Write(w, (JObject)t, ws, indent, out nl);
                break;
                case JType.Array:
                    Write(w, (JArray)t, ws, indent, out nl);
                break;
                case JType.Pair:
                    Write(w, (JPair)t, ws, indent, out nl);
                break;
                case JType.Null:
                case JType.String:
                case JType.Integer:
                case JType.Double:
                case JType.Boolean:
                case JType.DateTime:
                    Write(w, (JValue)t, ws, indent, out nl);
                break;
            }
        }

        static void Write(TextWriter w, JObject o, bool ws, int indent, out bool nl) {
            w.Write('{');
            indent += 1;
            bool writePad = false;
            for (int i = 0; i < o.Count; i++) {
                if (i == 0) {
                    if (ws) w.WriteLine();
                    writePad = true;
                }

                if (ws) w.Write(Pad(indent));
                bool newLine;
                Write(w, o[i], ws, indent, out newLine);
                if (i + 1 < o.Count) {
                    w.Write(',');
                    if (ws) w.WriteLine();
                }
                else {
                    if (newLine && ws) w.WriteLine();
                }
            }
            indent -= 1;
            if (writePad && ws) w.Write(Pad(indent));
            w.Write('}');
            nl = true;
        }

        static void Write(TextWriter w, JArray a, bool ws, int indent, out bool nl) {
            w.Write('[');
            indent += 1;
            bool writePad = false;
            for (int i = 0; i < a.Count; i++) {
                if (i == 0) {
                    if (ws) w.WriteLine();
                    writePad = true;
                }

                if (ws) w.Write(Pad(indent));
                bool newLine;
                Write(w, a[i], ws, indent, out newLine);
                if (i + 1 < a.Count) {
                    w.Write(',');
                    if (ws) w.WriteLine();
                }
                else {
                    if (newLine && ws) w.WriteLine();
                }
            }
            indent -= 1;
            if (writePad && ws) w.Write(Pad(indent));
            w.Write(']');
            nl = true;
        }

        static void Write(TextWriter w, JPair p, bool ws, int indent, out bool nl) {
            w.Write($"\"{p.Key}\":");
            if (ws) w.Write(' ');
            Write(w, p.Value, ws, indent, out nl);
        }

        static void Write(TextWriter w, JValue v, bool ws, int indent, out bool nl) {
            if (v.Value == null) {
                w.Write($"null");
            }
            if (v.Value is string) {
                w.Write($"\"{v.Value}\"");
            }
            else if (v.Value is int) {
                w.Write(v.Value);
            }
            else if (v.Value is double) {
                w.Write(((double)v.Value).ToString("G17"));
            }
            else if (v.Value is bool) {
                if ((bool)v.Value) {
                    w.Write("true");
                }
                else {
                    w.Write("false");
                }
            }
            else if (v.Value is DateTime) {
                w.Write($"\"{((DateTime)v.Value).ToUniversalTime().ToString(IsoDateFormat)}\"");
            }

            nl = true;
        }

        static string Pad(int indent) {
            if (indent == 0) return string.Empty;
            else if (indent == 1) return "  ";
            else if (indent == 2) return "    ";
            else if (indent == 3) return "      ";
            else if (indent == 4) return "        ";
            else if (indent == 5) return "          ";
            else if (indent == 6) return "            ";
            else if (indent == 7) return "              ";
            else if (indent == 8) return "                ";
            else if (indent == 9) return "                  ";
            else if (indent == 10) return "                    ";
            else if (indent == 11) return "                      ";
            else if (indent == 12) return "                        ";
            else return string.Empty.PadRight(indent * 2);
        }

    }
}
