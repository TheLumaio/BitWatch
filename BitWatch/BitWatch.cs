
using System;
using System.ComponentModel;
using System.Text;
using System.Threading;

namespace BitWatch
{
    public class BitWatch
    {

        private static Bittrex bit;
        private static string key = "7a8f51b3a2fa44afb8993c6dd0b50d08";
        private static string secret = "184cc8ccaba04a0f8c73a055d0353ee2";
        
        public static void Main(string[] args)
        {
            bit = new Bittrex(key, secret);
            
            bit.GetOpenOrders();
            var balances = bit.GetBalances();

            Console.CursorVisible = false;
            
            Console.Clear();
            Console.WriteLine("BTC  : History                :         Balance");
            Console.WriteLine("-----------------------------------------------");
            Console.SetCursorPosition(0, balances.result.Count+3);
            Console.WriteLine("Press <Q> to stop.");
            
            BackgroundWorker worker = new BackgroundWorker();
            worker.DoWork += delegate
            {
                while (!Console.KeyAvailable)
                {
                    balances = bit.GetBalances();
                    PrintBalances(balances);
                    Thread.Sleep(5000);
                }
            };
            worker.RunWorkerAsync();

            Console.ReadKey();
            Console.Clear();
            Console.ForegroundColor = ConsoleColor.Gray;
            Console.CursorVisible = true;
        }

        private static void PrintBalances(Balances balances)
        {
            Console.SetCursorPosition(0, 2);
            foreach (var b in balances.result)
            {
                if (b.Currency == "BTC") continue;
                Console.Write($"{b.Currency, -5}: ");
                var trades = bit.GetTrades($"BTC-{b.Currency}");

                var last = 0.0;
                for (var i = 0; i < 22; i++)
                {
                    last = trades.result[i + 1].Price;
                    var t = trades.result[i];
                    if (t.Price > last)
                    {
                        Console.ForegroundColor = ConsoleColor.Green;
                        Console.Write("↑");
                    }
                    else if (t.Price < last)
                    {
                        Console.ForegroundColor = ConsoleColor.Red;
                        Console.Write("↓");
                    }
                    else
                    {
                        Console.ForegroundColor = ConsoleColor.DarkGray;
                        Console.Write("-");
                    }
                    Console.ForegroundColor = ConsoleColor.Gray;
                }
                Console.WriteLine(" : {0,15:F10}", b.Available);
            }
        }
    }
}
