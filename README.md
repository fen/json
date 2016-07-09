# JSON .NET source library (POC)

This is a small .NET JSON source library with minimal dependencies with a
similar API to Newtonsoft.Json.Linq. 

A source library is a source code file that you can drop into your project. The
goal is doing the basic JSON operations, and small enought for someone to go in
and add specific stuff.

**NOTE** This is proof of concept code.

```cs
var arr = new JArray {
    new JObject {
        ["id"] = "a",
        ["str"] = null,
        ["date"] = DateTime.UtcNow,
        ["int"] = int.MaxValue,
        ["double"] = double.MaxValue, 
        ["obj"] = new JObject {
            ["id"] = "b"
        }
    }
};

Console.WriteLine(arr);
```

## Installation

Take the [json.cs](json.cs) file and drop it into your project.

## JSON Specification:

```
object
    {}
    { members }
members
    pair
    pair , members
pair
    string : value
array
    []
    [ elements ]
elements
    value 
    value , elements
value
    string
    number
    object
    array
    true
    false
    null

string
    ""
    " chars "
chars
    char
    char chars
char
    any-Unicode-character-
        except-"-or-\-or-
        control-character
    \"
    \\
    \/
    \b
    \f
    \n
    \r
    \t
    \u four-hex-digits
number
    int
    int frac
    int exp
    int frac exp
    int
digit
    digit1-9 digits 
    - digit
    - digit1-9 digits
frac
    . digits
exp
    e digits
digits
    digit
    digit digits
e
    e
    e+
    e-
    E
    E+
    E-
```

In addition to the JSON standard the library tries to by default parse ISO date
formats out of text.
