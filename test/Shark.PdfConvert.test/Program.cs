using NSpec.Domain;
using System;
using System.Linq;
using System.Reflection;

namespace Shark.PdfConvert.test
{
    public class Program
    {
        public static void Main(string[] args)
        {
            var ri = new RunnerInvocation(Assembly.GetEntryAssembly().Location, "", false);
            var results = ri.Run();

            if (results.Failures().Count() > 0)
            {
                Environment.Exit(1);
            }
        }
    }
}
