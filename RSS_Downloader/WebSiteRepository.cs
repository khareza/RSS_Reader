﻿using MongoDB.Driver;

namespace RSS_Downloader
{
    public class WebSiteRepository
    {
        private static IMongoCollection<WebSite> _webSites;

        public WebSiteRepository()
        {
            var client = new MongoClient("mongodb://localhost:27017");
            var server = client.GetDatabase("TestRssDB");
            _webSites = server.GetCollection<WebSite>("WebSites");
        }
    }
}
