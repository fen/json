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
            //var x = new StringReader("{ \"HelloWorld\" : \"foo\", \"A\" : true, \"B\" : false, \"C\" : null }");
            var sw = new Stopwatch();
            sw.Start();
            var o = JObject.Parse(File.OpenRead("test.json"));
            //bool uoi = (bool)o["ActionByObject"]["@unitOfInformation"];
            //Console.WriteLine($"Is ActionByObject an uoi: {uoi}");
            Console.WriteLine(o["date"].Type);
            sw.Stop();
            Console.WriteLine(sw.Elapsed);
            Console.WriteLine(o.ToString());
            //Console.WriteLine(r["Part"].ToString());
        }
    }

}
