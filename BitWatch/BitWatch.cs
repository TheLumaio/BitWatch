using System;
using System.ComponentModel;
using System.Text;
using System.Threading;

namespace BitWatch
{
	public class BitWatch
	{
		private static Bittrex bit;
		private static string key = "7a8f51b3a2fa44afb8993c6dd0b50d08"; // dont hack plz
		private static string secret = "184cc8ccaba04a0f8c73a055d0353ee2"; // i dont want my bitz stolen thx
		private static double btcvalue = 0;

		public static void Main(string[] args)
		{
			Console.SetWindowSize(100, 25);

			bit = new Bittrex(key, secret);

			bit.GetOpenOrders();
			var balances = bit.GetBalances();

			Console.CursorVisible = false;

			Console.Clear();
			Console.WriteLine("COIN : History                :         Balance :       BTC Value :  USD Value :    USD P/C");
			Console.WriteLine("-------------------------------------------------------------------------------------------");
			Console.SetCursorPosition(0, balances.result.Count + 4);
			Console.WriteLine("Press <Q> to stop.");

			BackgroundWorker worker = new BackgroundWorker();
			worker.DoWork += delegate
			{
				while (!Console.KeyAvailable)
				{
					btcvalue = bit.GetBtcValue().result.Last;
					balances = bit.GetBalances();
					PrintBalances(balances);
					Thread.Sleep(5000);
				}
			};
			worker.RunWorkerAsync();

			while (Console.ReadKey(true).Key != ConsoleKey.Q) ;
			Console.Clear();
			Console.ForegroundColor = ConsoleColor.Gray;
			Console.CursorVisible = true;
		}

		private static void PrintBalances(Balances balances)
		{
			Console.SetCursorPosition(0, 2);
			double totalbits = 0;
			foreach (var b in balances.result)
			{
				if (b.Currency == "BTC")
				{
					totalbits += b.Available;
					var busdval = Math.Round(btcvalue * b.Available, 2);
					Console.WriteLine("BTC  : ");
					Console.ForegroundColor = ConsoleColor.DarkGray;
					Console.Write("----------------------");
					Console.ForegroundColor = ConsoleColor.Gray;
					Console.WriteLine(" : {0,15:F10} :             N/A : ${2,9:F2} : ${3,9:F2}", b.Available, btcvalue * b.Available,
						busdval, btcvalue);
					continue;
				}
				;

				if (b.Balance == 0)
					continue;

				Console.Write($"{b.Currency,-5}: ");
				var trades = bit.GetTrades($"BTC-{b.Currency}");
				totalbits += trades.result[0].Price * b.Balance;

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
				var tradebal = trades.result[0].Price * b.Balance;
				var usdval = Math.Round(btcvalue * tradebal, 2);
				var usdpc = Math.Round(btcvalue * trades.result[0].Price, 2);
				Console.WriteLine(" : {0,15:F10} : {1,15:F10} : ${2,9:F2} : ${3,9:F2}", b.Available, tradebal, usdval, usdpc);
			}
			Console.WriteLine("\nTotal BTC value: {0}", totalbits);
			Console.WriteLine("Total USD value: ${0:F2}", Math.Round(btcvalue * totalbits, 2));
		}
	}
}