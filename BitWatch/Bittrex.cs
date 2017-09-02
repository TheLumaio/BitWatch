using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Net.Http;
using System.Security.Cryptography;
using System.Text;
using Newtonsoft.Json;

namespace BitWatch
{
	/* Balances */
	public class Coin
	{
		public string Currency { get; set; }
		public double Balance { get; set; }
		public double Available { get; set; }
		public double Pending { get; set; }
		public string CryptoAddress { get; set; }
		public bool Requested { get; set; }
		public object Uuid { get; set; }
	}

	public class Balances
	{
		public bool success { get; set; }
		public string message { get; set; }
		public List<Coin> result { get; set; }
	}

	/* Trades */
	public class Trade
	{
		public int Id { get; set; }
		public string TimeStamp { get; set; }
		public double Quantity { get; set; }
		public double Price { get; set; }
		public double Total { get; set; }
		public string FillType { get; set; }
		public string OrderType { get; set; }
	}

	public class Trades
	{
		public bool success { get; set; }
		public string message { get; set; }
		public List<Trade> result { get; set; }
	}

	/* BTC value */
	public class Value
	{
		public double Bid { get; set; }
		public double Ask { get; set; }
		public double Last { get; set; }
	}

	public class BTCValue
	{
		public bool success { get; set; }
		public string message { get; set; }
		public Value result { get; set; }
	}

	/* Bittrex */
	public class Bittrex
	{
		private static string apisign = "";
		private static string url = "";
		private static long nonce = 0;
		private static string key = "";
		private static string secret = "";
		private static readonly HttpClient client = new HttpClient();

		public Bittrex(string _key, string _secret)
		{
			nonce = System.DateTime.Now.Ticks;
			Console.WriteLine("Nonce/Time: {0}", nonce);

			key = _key;
			secret = _secret;
		}

		private static string HashHmac(string message, string secret)
		{
			Encoding encoding = Encoding.UTF8;
			using (HMACSHA512 hmac = new HMACSHA512(encoding.GetBytes(secret)))
			{
				var msg = encoding.GetBytes(message);
				var hash = hmac.ComputeHash(msg);
				return BitConverter.ToString(hash).ToLower().Replace("-", string.Empty);
			}
		}

		public void GetOpenOrders()
		{
			url = string.Format("https://bittrex.com/api/v1.1/market/getopenorders?apikey={0}&nonce={1}", key, nonce);
			apisign = HashHmac(url, secret);

			var request = new HttpRequestMessage()
			{
				RequestUri = new Uri(url),
				Method = HttpMethod.Get
			};
			request.Headers.Add("apisign", apisign);

			HttpResponseMessage response = client.SendAsync(request).Result;
			string message = response.Content.ReadAsStringAsync().Result;
		}

		public Balances GetBalances()
		{
			url = string.Format("https://bittrex.com/api/v1.1/account/getbalances?apikey={0}&nonce={1}", key, nonce);
			apisign = HashHmac(url, secret);

			var request = new HttpRequestMessage()
			{
				RequestUri = new Uri(url),
				Method = HttpMethod.Get
			};
			request.Headers.Add("apisign", apisign);

			HttpResponseMessage response = client.SendAsync(request).Result;
			string message = response.Content.ReadAsStringAsync().Result;
			Balances balances = JsonConvert.DeserializeObject<Balances>(message);

			return balances;
		}

		public Trades GetTrades(string market)
		{
			url = $"https://bittrex.com/api/v1.1/public/getmarkethistory?market={market}";

			var request = new HttpRequestMessage()
			{
				RequestUri = new Uri(url),
				Method = HttpMethod.Get
			};

			HttpResponseMessage response = client.SendAsync(request).Result;
			string message = response.Content.ReadAsStringAsync().Result;
			Trades trades = JsonConvert.DeserializeObject<Trades>(message);

			return trades;
		}

		public BTCValue GetBtcValue()
		{
			url = $"https://bittrex.com/api/v1.1/public/getticker?market=usdt-btc";
			var request = new HttpRequestMessage()
			{
				RequestUri = new Uri(url),
				Method = HttpMethod.Get
			};

			HttpResponseMessage response = client.SendAsync(request).Result;
			string message = response.Content.ReadAsStringAsync().Result;
			BTCValue value = JsonConvert.DeserializeObject<BTCValue>(message);

			return value;
		}
	}
}