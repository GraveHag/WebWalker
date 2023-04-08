﻿using NLog.Fluent;
using System;
using System.Diagnostics;

namespace CS_Core
{
    /// <summary>
    /// Abstract webCrawler
    /// </summary>
    internal abstract class WebCrawler : IWebCrawler
    {
        protected readonly string _spiderName = NameGenerator.GetName();

        string IWebCrawler.SpiderName => _spiderName;

        protected TimeSpan LiveSpan = new TimeSpan(0, 0, 10);

        protected readonly Stopwatch _stopwatch;

        protected int MaxDepth;

        protected Uri StartedDomain = new Uri("https://www.google.com");

        protected bool IsAlive => LiveSpan.TotalMilliseconds > _stopwatch.Elapsed.TotalMilliseconds;

        protected HttpClient HttpClient => ServiceCatalog.Mediate<IHttpClientFactory>().CreateClient(_spiderName);

        bool IWebCrawler.IsRunning => IsAlive;

        protected async Task<HttpResponseMessage> GetResponse(Uri uri, CancellationToken token)
        {
            try
            {
                HttpRequestMessage request = new HttpRequestMessage(HttpMethod.Get, uri);
                HttpResponseMessage response = await HttpClient.GetAsync(uri, token);

                //todo error handling
                //use another proxy if server refuse
                return response;

            }
            catch (Exception ex)
            {
                LogService.Fatal(ex, $"{GetType().FullName}:[{_spiderName}]", nameof(GetResponse));
                return new HttpResponseMessage() {StatusCode = System.Net.HttpStatusCode.ServiceUnavailable };
            }
        }

        public abstract Task Run(CancellationToken token);

        public abstract Task<CrawlerResponse?> GetUrl(Uri uri, CancellationToken token);

        public WebCrawler(Func<CrawlerConfiguration> configuration) : this()
        {
            LiveSpan = configuration().TimeToLive;
            MaxDepth = configuration().MaxDepth;
            StartedDomain = configuration().StartedDomain;
        }

        protected WebCrawler()
        {
            _stopwatch = new Stopwatch();

            _stopwatch.Start();
        }

        protected void ReadHtmlContent(string htmlContent)
        {
            //todo HtmlAgilityPack
        }
    }
}
