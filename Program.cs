using System;
using System.IO;
using System.Text;
using System.Collections.Generic;
using System.Diagnostics;

using Json;

namespace ConsoleApplication {
    public class Program {
        public static void Main(string[] args) {
            var sw = new Stopwatch();
            sw.Start();
            Result<JObject> o;
            using (var s = File.OpenRead("citylots.json")) {
                o = JObject.Parse(s);
            }
            if (o.Failed) {
                Console.WriteLine($"ERROR: {o.ErrorCode}");
                return;
            }
            sw.Stop();
            Console.WriteLine(sw.Elapsed);
        }
    }
}
