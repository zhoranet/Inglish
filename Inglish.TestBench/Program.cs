using System;
using Inglish.Test;
using Newtonsoft.Json;

namespace Inglish.TestBench
{
    internal class Program
    {
        private static void Main(string[] args)
        {
            try
            {
                var parser = new Parser(new Thesaurus());
                Console.WriteLine("Type sentence and press ENTER:");
                var cmd = parser.DoCommand(Console.ReadLine());
                Console.WriteLine(JsonConvert.SerializeObject(cmd));
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
            }

            Console.Read();
        }
    }
}