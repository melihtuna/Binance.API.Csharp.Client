using Binance.API.Csharp.Client.Models.Market;
using Binance.API.Csharp.Client.Models.WebSocket;
using Newtonsoft.Json.Linq;
using System.Collections.Generic;
using System.Linq;

namespace Binance.API.Csharp.Client.Utils
{
    /// <summary>
    /// Class to parse some specific entities.
    /// </summary>
    public class CustomParser
    {
        /// <summary>
        /// Gets the orderbook data and generates an OrderBook object.
        /// </summary>
        /// <param name="orderBookData">Dynamic containing the orderbook data.</param>
        /// <returns></returns>
        public OrderBook GetParsedOrderBook(dynamic orderBookData)
        {
            var result = new OrderBook
            {
                LastUpdateId = orderBookData.lastUpdateId.Value
            };

            var bids = new List<OrderBookOffer>();
            var asks = new List<OrderBookOffer>();

            foreach (JToken item in ((JArray)orderBookData.bids).ToArray())
            {
                bids.Add(new OrderBookOffer() { Price = decimal.Parse(item[0].ToString()), Quantity = decimal.Parse(item[1].ToString()) });
            }

            foreach (JToken item in ((JArray)orderBookData.asks).ToArray())
            {
                asks.Add(new OrderBookOffer() { Price = decimal.Parse(item[0].ToString()), Quantity = decimal.Parse(item[1].ToString()) });
            }

            result.Bids = bids;
            result.Asks = asks;

            return result;
        }

        /// <summary>
        /// Gets the candlestick data and generates an Candlestick object.
        /// </summary>
        /// <param name="candlestickData">Dynamic containing the candlestick data.</param>
        /// <returns></returns>
        public IEnumerable<Candlestick> GetParsedCandlestick(dynamic candlestickData)
        {
            var result = new List<Candlestick>();

            foreach (JToken item in ((JArray)candlestickData).ToArray())
            {
                result.Add(new Candlestick()
                {
                    OpenTime = long.Parse(item[0].ToString()),
                    Open = item[1].Value<decimal>(),
                    High = item[2].Value<decimal>(),
                    Low = item[3].Value<decimal>(),
                    Close = item[4].Value<decimal>(),
                    Volume = item[5].Value<decimal>(),
                    CloseTime = long.Parse(item[6].ToString()),
                    QuoteAssetVolume = item[7].Value<decimal>(),
                    NumberOfTrades = int.Parse(item[8].ToString()),
                    TakerBuyBaseAssetVolume = item[9].Value<decimal>(),
                    TakerBuyQuoteAssetVolume = item[10].Value<decimal>()
                });
            }

            return result;
        }

        public DepthMessage GetParsedDepthMessage(dynamic messageData)
        {
            var result = new DepthMessage
            {
                EventType = messageData.e,
                EventTime = messageData.E,
                Symbol = messageData.s,
                UpdateId = messageData.u
            };

            var bids = new List<OrderBookOffer>();
            var asks = new List<OrderBookOffer>();

            foreach (JToken item in ((JArray)messageData.b).ToArray())
            {
                bids.Add(new OrderBookOffer() { Price = decimal.Parse(item[0].ToString()), Quantity = decimal.Parse(item[1].ToString()) });
            }

            foreach (JToken item in ((JArray)messageData.a).ToArray())
            {
                asks.Add(new OrderBookOffer() { Price = decimal.Parse(item[0].ToString()), Quantity = decimal.Parse(item[1].ToString()) });
            }

            result.Bids = bids;
            result.Asks = asks;

            return result;
        }
    }
}
