using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;
using ApsDefence;

namespace ApsDefenceHarness
{
    class Program
    {
        static void Main(string[] args)
        {
            try
            {
                RDPDefender defender = new RDPDefender(Path.Combine(Helper.ExecutionDirectory, "ApsDefence.conf"));
                WaitForQ();
                defender.Dispose();
            }
            catch (Exception ex)
            {
                Logger.Error(ex.Message);
            }
            finally
            {
                Logger.Close();
            }
        }

        static void WaitForQ()
        {
            Console.WriteLine("Press Q to exit...");
            while (true)
            {
                ConsoleKeyInfo key = Console.ReadKey(true);
                if (key.KeyChar == 'Q' || key.KeyChar == 'q')
                {
                    break;
                }
            }
        }

    }
}
