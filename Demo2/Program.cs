using Binance.API.Csharp.Client;
using Binance.API.Csharp.Client.Models.Enums;
using Binance.API.Csharp.Client.Models.Market;
using Binance.API.Csharp.Client.Utils;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using Trady.Analysis.Extension;
using Trady.Core;
using System.Globalization;

namespace Demo2
{
    class Program
    {
        static void Main(string[] args)
        {
            var binanceClient = new BinanceClient(
                new ApiClient(
                    "ITQNFMsjWk7rR9J8MIA3nt2VZSsQumyADZsiIjEjXsvujHbexxVIq1dwMHmCgyab",
                    "4PXwEfnDtmUnwcQaFbPDOuPvy9IZI4IjyEOFsDaJHECr7eF84wAE7HD0zu3lEr8g"
                ), true);

            AlisYap(binanceClient, "WTCUSDT", 20);
            //SatisYap(binanceClient, "MATICUSDT");
            Console.WriteLine("==================== BİTTİ ====================");
            Console.ReadLine();
        }

        public static void AlisYap(BinanceClient binanceClient, string sembol, int emirBuyuklugu)
        {
            //var accountInfo = binanceClient.GetAccountInfo().Result;
            //var kapitalBakiye = accountInfo.Balances.Where(x => x.Asset == "USDT").First().Free;
            //var uyeKomisyonOran = decimal.Parse(((double)accountInfo.TakerCommission / (double)1000).ToString(CultureInfo.InvariantCulture), CultureInfo.InvariantCulture);

            //if (kapitalBakiye > emirBuyuklugu)
            //{
            //    var sembolFiyati = binanceClient.GetAllPrices().Result
            //    .Where(x => x.Symbol.EndsWith(sembol))
            //    .Select(i => new SymbolPrice { Symbol = i.Symbol, Price = i.Price })
            //    .OrderBy(x => x.Symbol).ToList();

            //    var emirBoyutundaSembolAdeti = emirBuyuklugu / sembolFiyati.FirstOrDefault().Price;
            //    var stepSize = binanceClient._tradingRules.Symbols.Where(x => x.SymbolName == sembol).First().Filters.Where(y => y.FilterType == "LOT_SIZE").First().StepSize;
            //    var stepIndex = stepSize.ToString(CultureInfo.InvariantCulture).Split('.')[1].IndexOf('1') + 1;

            //    var komisyonMiktar = emirBoyutundaSembolAdeti * uyeKomisyonOran;
            //    var alinabilirMiktar = emirBoyutundaSembolAdeti - komisyonMiktar;

            //    var alinabilirMiktarString = alinabilirMiktar.ToString(CultureInfo.InvariantCulture).Split('.');
            //    var islemeGirilecekMiktar = Convert.ToDecimal(alinabilirMiktarString[0] + "," + alinabilirMiktarString[1].Substring(0, stepIndex));

            //    var buyMinNotional = binanceClient._tradingRules.Symbols.Where(x => x.SymbolName == sembol).First().Filters.Where(y => y.FilterType == "MIN_NOTIONAL").First().MinNotional;

            //    if (alinabilirMiktar > buyMinNotional)
            //    {
            //        //var buyOrder = binanceClient.PostNewOrder(sembol, islemeGirilecekMiktar, 0, OrderSide.BUY, OrderType.MARKET).Result;


            //        //var getOrder = binanceClient.GetOrder(buyOrder.Symbol, buyOrder.OrderId, buyOrder.ClientOrderId,60000);
            //        //var orderResult = getOrder.Result;
            //        //Console.WriteLine(orderResult.Price);
            //    }
            //    else
            //    {
            //        //Verilen emir boyutuna göre min alım miktarını geçemediği için işleme girilemediği bilgisi dön - Telegram
            //    }
            //}
            //else
            //{
            //    //Bakiye yetersiz olduğu için işleme girilemediği bilgisi dön - Telegram                
            //}

            var getTrades = binanceClient.GetTradeList(sembol);
            var getTradesResult = getTrades.Result;

            var deneme = getTradesResult.Where(x => x.Time == 1596174359986);

            if (deneme.Any())
            {
                Console.WriteLine(deneme.Average(x => x.Price));
            }

            Console.WriteLine(TicaretFiyatiDon(binanceClient, sembol, 1596174359986));

        }

        public static decimal TicaretFiyatiDon(BinanceClient binanceClient, string sembol, long zamanDamgasi)
        {
            var getTrades = binanceClient.GetTradeList(sembol, 20000);
            var getTradesResult = getTrades.Result;

            var ticaretList = getTradesResult.Where(x => x.Time == zamanDamgasi);

            if (ticaretList.Any())
            {
                decimal toplamMaliyet = 0;
                decimal toplamAdet = 0;
                foreach (var ticaret in ticaretList)
                {
                    toplamMaliyet += ticaret.Price * ticaret.Quantity;
                    toplamAdet += ticaret.Quantity;
                }

                if (toplamAdet > 0)
                {
                    return toplamMaliyet / toplamAdet;
                }
            }

            return -1;
        }

        public static void SatisYap(BinanceClient binanceClient, string sembol)
        {
            var accountInfo = binanceClient.GetAccountInfo().Result;
            var sembolBakiye = accountInfo.Balances.Where(x => x.Asset == sembol.Replace("USDT", "")).First().Free;

            var stepSize = binanceClient._tradingRules.Symbols.Where(x => x.SymbolName == sembol).First().Filters.Where(y => y.FilterType == "LOT_SIZE").First().StepSize;
            var stepIndex = stepSize.ToString(CultureInfo.InvariantCulture).Split('.')[1].IndexOf('1') + 1;

            var satilabilirMiktarString = sembolBakiye.ToString(CultureInfo.InvariantCulture).Split('.');
            var islemeGirilecekMiktar = Convert.ToDecimal(satilabilirMiktarString[0] + "," + satilabilirMiktarString[1].Substring(0, stepIndex));

            var sellOrder = binanceClient.PostNewOrder(sembol, islemeGirilecekMiktar, 0, OrderSide.SELL, OrderType.MARKET).Result;
        }

        private static void MostHesapla(SymbolPrice coin, Candlestick[] candlesticks, TimeInterval periyot, int emaUzunluk, double yuzde)
        {
            var startUnixTime = new DateTime(1970, 1, 1, 0, 0, 0, DateTimeKind.Utc);

            List<IslemListe> islemListe = new List<IslemListe>();

            //Console.WriteLine(coin.Symbol + " İÇİN " + periyot.GetDescription() + " PERİYOTTA MOST(" + emaUzunluk + "," + yuzde * 100 + ") BACKTEST");
            List<Candle> tradyCandles = new List<Candle>();
            List<Most> mostList = new List<Most>();

            decimal most = 0;
            decimal topEma = 0;

            decimal coinAdet = 0;
            decimal sermaye = 3000;
            decimal kar = 3000;

            var closes = candlesticks.Select(x => x.Close).ToList();
            var emaList = closes.Ema(emaUzunluk).ToList();

            for (int i = 0; i < emaList.Count; i++)
            {
                var ema = Convert.ToDecimal(emaList[i]);
                var exEma = mostList.Count > 0 ? mostList[i - 1].EmaDegeri : ema;
                var exMost = mostList.Count > 0 ? mostList[i - 1].MostDegeri : most;
                var durum = exEma < most ? "SAT" : "AL";
                var exDurum = mostList.Count > 0 ? mostList[i - 1].MostDurum : durum;

                if (durum == "AL")
                {
                    topEma = ema > topEma ? ema : topEma;
                    most = topEma > exEma ? topEma - Convert.ToDecimal((double)topEma * yuzde) : exMost;
                }
                else
                {
                    topEma = ema < topEma ? ema : topEma;
                    most = topEma < exEma ? topEma + Convert.ToDecimal((double)topEma * yuzde) : exMost;
                }

                mostList.Add(new Most
                {
                    EmaDegeri = ema,
                    MostDegeri = most,
                    MostDurum = durum
                });


                if (exDurum != durum)
                {
                    if (durum == "AL")
                    {
                        coinAdet = sermaye / candlesticks[i].Open;
                    }
                    else
                    {
                        kar += coinAdet * candlesticks[i].Open - sermaye;
                    }

                    islemListe.Add(new IslemListe
                    {
                        Sembol = coin.Symbol,
                        MostParametreleri = "(" + emaUzunluk + "," + yuzde * 100 + ")",
                        Periyot = periyot.GetDescription(),
                        AcilisZamani = candlesticks[i].OpenTime,
                        Adet = coinAdet,
                        Durum = durum,
                        EmaDeger = ema,
                        Fiyat = candlesticks[i].Open,
                        Kar = kar,
                        MostDeger = most,
                        Sermaye = sermaye
                    });

                    //Console.WriteLine(
                    //    "ZAMAN:" + startUnixTime.AddMilliseconds(candlesticks[i].OpenTime).ToLocalTime() +
                    //    " MOST:" + $"{most:N4}" + " EMA:" + $"{ema:N4}" + " DURUM:" + durum + " FİYAT:" +
                    //    $"{candlesticks[i].Open:N4}" + " ADET:" + $"{coinAdet:N4}" + " KAR:" + $"{kar:N4}" +
                    //    " SERMAYE:" + $"{sermaye:N4}");

                    Kaydet("http://www.netdata.com:3000/json/33102eb9?dc_Sembol=" + coin.Symbol + "&dc_Periyot=" + periyot.GetDescription() +
                           "&dc_Most_Parametreleri=(" + emaUzunluk + "," + yuzde * 100 + ")" + "&dc_Bar_Tarihi=" +
                           startUnixTime.AddMilliseconds(candlesticks[i].OpenTime).ToLocalTime() + "&dc_Most_Degeri=" +
                           $"{most:N4}" + "&dc_Ema_Degeri=" + $"{ema:N4}" + "&dc_Durum=" + durum + "&dc_Sermaye=" +
                           $"{sermaye:N4}" + "&dc_Fiyat=" + $"{candlesticks[i].Open:N4}" + "&dc_Adet=" +
                           $"{coinAdet:N4}" + "&dc_Kar=" + $"{kar:N4}");
                }
            }

            var sonucListe = new IslemOzet
            {
                Sembol = coin.Symbol,
                Kar = islemListe.Count > 0 ? kar : 0,
                Sermaye = sermaye,
                MostParametreleri = "(" + emaUzunluk + "," + yuzde * 100 + ")",
                Periyot = periyot.GetDescription(),
                KarOran = islemListe.Count > 0 ? kar / sermaye * 100 : 0,
                BarSayisi = candlesticks.Length,
                IslemSayisi = islemListe.Count
            };

            Console.WriteLine("BİRİM: " + sonucListe.Sembol + " PERİYOT:" + sonucListe.Periyot + " MOST" + sonucListe.MostParametreleri + " SERMAYE:" + sonucListe.Sermaye + " KAR:" + $"{sonucListe.Kar:N4}" + " KAR ORAN:" + $"{sonucListe.KarOran:N2}" + " BAR SAYISI:" + sonucListe.BarSayisi + " İŞLEM SAYISI:" + sonucListe.IslemSayisi);

            Kaydet("http://www.netdata.com:3000/json/7e84157e?dc_Sembol=" + sonucListe.Sembol + "&dc_Periyot=" +
                   sonucListe.Periyot + "&dc_Most_Parametreleri=" + sonucListe.MostParametreleri + "&dc_Sermaye=" +
                   sonucListe.Sermaye + "&dc_Kar=" + $"{sonucListe.Kar:N4}" + "&dc_Kar_Oran=" +
                   $"{sonucListe.KarOran:N2}" + "&dc_Bar_Sayisi=" + sonucListe.BarSayisi + "&dc_Islem_Sayisi=" +
                   sonucListe.IslemSayisi);


            //return new KeyValuePair<IslemOzet, List<IslemListe>>(sonucListe, islemListe);
        }

        private static void Kaydet(string url)
        {
            try
            {
                var request = (HttpWebRequest)WebRequest.Create(url);
                var response = (HttpWebResponse)request.GetResponse();
                var responseString = new StreamReader(response.GetResponseStream()).ReadToEnd();
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
            }
        }

        private static void KayitlariSil(string api)
        {
            try
            {
                string Content = @"<?xml version='1.0' encoding='utf-8'?>
                                <soap:Envelope xmlns:xsi='http://www.w3.org/2001/XMLSchema-instance' xmlns:xsd='http://www.w3.org/2001/XMLSchema' xmlns:soap='http://schemas.xmlsoap.org/soap/envelope/'>
                                  <soap:Body>
                                    <CustomDelete xmlns='http://tempuri.org/'>
                                      <APIKey>{0}</APIKey>
                                      <DeleteConditionsList>
                                        <WhereList>
                                          <Key>ID</Key>
                                          <Operation>NOT EQUAL</Operation>
                                          <Value>0</Value>
                                        </WhereList>
                                      </DeleteConditionsList>
                                    </CustomDelete>
                                  </soap:Body>
                                </soap:Envelope>";

                string url = "http://www.netdata.com/AccPo.asmx";
                string contentType = "text/xml; charset=utf-8";
                string method = "POST";
                string header = "SOAPAction: \"http://tempuri.org/CustomDelete\"";

                HttpWebRequest req = (HttpWebRequest)WebRequest.Create(url);
                req.Method = method;
                req.ContentType = contentType;
                req.Headers.Add(header);

                Stream strRequest = req.GetRequestStream();
                StreamWriter sw = new StreamWriter(strRequest);
                sw.Write(Content, api);
                sw.Close();
                HttpWebResponse resp = (HttpWebResponse)req.GetResponse();
                Stream strResponse = resp.GetResponseStream();
                StreamReader sr = new StreamReader(strResponse, System.Text.Encoding.ASCII);
                sr.Close();
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
            }
        }
    }

    public class IslemOzet
    {
        public string Sembol { get; set; }
        public string Periyot { get; set; }
        public string MostParametreleri { get; set; }
        public decimal Sermaye { get; set; }
        public decimal Kar { get; set; }
        public decimal KarOran { get; set; }
        public int BarSayisi { get; set; }
        public int IslemSayisi { get; set; }
    }

    public class IslemListe
    {
        public string Sembol { get; set; }
        public string Periyot { get; set; }
        public string MostParametreleri { get; set; }
        public long AcilisZamani { get; set; }
        public decimal MostDeger { get; set; }
        public decimal EmaDeger { get; set; }
        public string Durum { get; set; }
        public decimal Sermaye { get; set; }
        public decimal Fiyat { get; set; }
        public decimal Adet { get; set; }
        public decimal Kar { get; set; }
    }

    public class Most
    {
        public decimal EmaDegeri { get; set; }
        public decimal MostDegeri { get; set; }
        public string MostDurum { get; set; }
    }
}
