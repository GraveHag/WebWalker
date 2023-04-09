﻿using AngleSharp.Dom;
using AngleSharp.Html.Dom;

namespace CS_Core
{
    /// <summary>
    /// CyberspaceSpider
    /// type of web crawler
    /// </summary>
    internal sealed class CyberspaceSpider : WebCrawler
    {
        public CyberspaceSpider(Func<CrawlerConfiguration> configuration) : base(configuration)
        {
        }

        public CyberspaceSpider() { }

        public override async Task<CrawlerResponse?> GetUrl(Uri uri, CancellationToken token)
        {
            if (!IsAlive) return default;

            HttpResponseMessage response = await GetResponse(uri, token);

            LogService.Info($"{GetType().Name}:[{_spiderName}]", nameof(GetUrl), $"{uri} [{response.StatusCode}]");

            if (response is null) return new CrawlerResponse { CurrentDomain = uri.ToString(), StatusCode = 500};
            if (!response.IsSuccessStatusCode) return new CrawlerResponse { CurrentDomain = uri.ToString(), StatusCode = (int)response.StatusCode };

            string htmlContent = await response.Content.ReadAsStringAsync(token);

            CrawlerResponse crawlerResponse = new CrawlerResponse { CurrentDomain = uri.ToString() };

            //Read content -> transfer to document object
            IDocument document = await ReadHtmlContent(htmlContent);

            //Retrieve all "<a href=....><a/>" dom elements, then select href contents to list
            IHtmlCollection<IElement> linkElements = document.Links;
            IList<Uri> links = linkElements.Select(el => ((IHtmlAnchorElement)el).Href).ToUriList();


            //Filter new distinct domains
            crawlerResponse.NextDomains = links.Select(link => link.Host).Distinct().ToUriList();

            return crawlerResponse;

        }

        public override async Task Run(CancellationToken token)
        {
            //todo another possibilities 
            while (IsAlive)
            {
                await GetUrl(StartedDomain, token);
            }

        }
    }
}
