using System;
using System.IO;
using System.Text;
using System.Collections.Generic;
using System.Diagnostics;

using Json;

namespace ConsoleApplication
{
    public class Program
    {
        public static void Main(string[] args)
        {
            var sw = new Stopwatch();
            sw.Start();
            var o = JObject.Parse(File.OpenRead("output.json"));
            if (o.Failed) {
                Console.WriteLine($"ERROR: {o.ErrorCode}");
            }
            //bool uoi = (bool)o["ActionByObject"]["@unitOfInformation"];
            //Console.WriteLine($"Is ActionByObject an uoi: {uoi}");
            //Console.WriteLine(o["date"].Type);
            sw.Stop();
            Console.WriteLine(sw.Elapsed);
            //Console.WriteLine(o.Value.ToString());
            /*sw.Reset();
            sw.Start();
            if (File.Exists("output.json") == true) {
                File.Delete("output.json");
            }
            using (var f = File.OpenWrite("output.json")) {
                o.Value.Write(f);
                f.Flush();
            }
            sw.Stop();
            Console.WriteLine(sw.Elapsed);*/
            //Console.WriteLine(r["Part"].ToString());
        }
    }

}
