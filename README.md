# .NET JSON parser source library

This is a small .NET JSON parser library, distributed as a single C# source
file ([json.cs](json.cs)). It's API is similar to the Newtonsoft.Json.Linq
JObject, JArray API.

This exists because I needed a lightweight library with minimal dependencies
doing the essentials of JSON parsing. This is not a replacement for
Newtonsoft.Json.

This library is built for the CoreCLR with C# 6 syntax (develop on Linux) but
should work on other CLR implementations.

## Example

```cs
using System;
using System.IO;

using Json;

static class Program {
    static void Main() {
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
        Console.WriteLine(arr.ToString(ws: false));

        using (var f = File.OpenWrite("output.json")) {
            arr.Write(f);
        }

        var o = JObject.Parse("{ \"result\" : 42 }");
        if (o.Failed) {
            // ERROR
        }
        using (var f = File.OpenRead("input.json")) {
            o = JObject.Parse(f);
            if (o.Failed) {
                // ERROR
            }
        }
    }
}
```

## Installation

Take the [json.cs](json.cs) file and drop it into your project.

## JSON Specification:

The implementation is following the [JSON specification](http://www.json.org)
with two additions.

1. The library tries to parse ISO date formats (`yyyy-MM-ddTHH:mm:ss.FFFFFFFK`)
   out of JSON strings (if dates are not important to you it is recomented to
   remove this from the json.cs file because it has some impact on
   performance).
2. C like line (`//`) and mulit line (`/*`) comments are supported and handeled as
   white space.

Other additions are out of scope for this implementation and should be handled
in forks.

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

## LICENSE

To the extent possible under law, the author(s) have **dedicated** all copyright
and related and neighboring rights to this software to the public domain
worldwide. This software is distributed without any warranty.

You can copy, modify, distribute and perform the work, even for commercial
purposes, all without asking permission. 

## TODO

* [ ] Implement test scenarios
