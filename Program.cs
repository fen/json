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
