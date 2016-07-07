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
            var o = JObject.Parse(File.OpenRead("model.json"));
            bool uoi = (bool)o["ActionByObject"]["@unitOfInformation"];
            Console.WriteLine($"Is ActionByObject an uoi: {uoi}");
            sw.Stop();
            Console.WriteLine(sw.Elapsed);
            //Console.WriteLine(r["Part"].ToString());
        }
    }

}
