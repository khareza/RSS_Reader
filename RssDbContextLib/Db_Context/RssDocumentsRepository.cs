﻿using MongoDB.Bson;
using MongoDB.Driver;
using RssModelsLib.Models;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace RssDbContextLib.Db_Context
{
    public class RssDocumentsRepository : IRssDocumentsRepository
    {
        public IMongoCollection<RssDocumentSingle> _rssDocumentCollection;
        private IMongoCollection<SubscriberEmail> _subscribers;

        private MongoClient _mongoClient;
        private IMongoDatabase _server;
        public string collectionRSS = "Stored_RSS";
        public string collectionMailingList = "Mailing_List";


        public RssDocumentsRepository()
        {
            string path;
            string user;
            string password;
            string database;
            string connectionstring;

            path = @"C:\credentials.txt";
            user = "Admin";
            password = File.ReadLines(path).First();
            database = "rss_downloader_web_application";
            connectionstring = $"mongodb://{user}:{password}@ds062818.mlab.com:62818/{database}";

            InitializeContext(connectionstring, database);
        }


        public void InitializeContext(string connectionstring, string database)
        {
            _mongoClient = new MongoClient(connectionstring);
            _server = _mongoClient.GetDatabase(database);
            _rssDocumentCollection = _server.GetCollection<RssDocumentSingle>(collectionRSS);
        }

        public void SaveOneRssDocumentToDatabase(RssDocumentSingle rssDocument)
        {
            _rssDocumentCollection.InsertOne(rssDocument);
        }

        public void SaveManyRssDocumentsToDatabase(List<RssDocumentSingle> rssDocuments)
        {
            _rssDocumentCollection.InsertMany(rssDocuments);
        }

        public bool CheckIfDatabaseIsEmpty()
        {
            if (_rssDocumentCollection.AsQueryable().Count() == 0)
            {
                return true;
            }
            return false;
        }

        public List<RssDocumentSingle> GetAllDocuments()
        {
            return _rssDocumentCollection.AsQueryable().ToList();
        }

        public void AddNewContent(List<RssDocumentSingle> documentsWithNewContent)
        {
            var allDocuments = _rssDocumentCollection.AsQueryable().ToList();
            foreach (var document in documentsWithNewContent)
            {
                var filter = $"{document.Id}";
                RssDocumentSingle singleDocument = null;
                foreach (var item in document.RssDocumentContent)
                {
                    singleDocument = _rssDocumentCollection.AsQueryable().Where(x => x.Id == document.Id)?.FirstOrDefault();
                    singleDocument.LastFetched = DateTime.UtcNow.AddHours(1);
                    singleDocument.RssDocumentContent.Add(item);
                }
                if (singleDocument != null)
                {
                    _rssDocumentCollection.ReplaceOne(x => x.Id == document.Id, singleDocument);
                }

            }
        }

        public IMongoCollection<SubscriberEmail> GetSubscribersList()
        {
            _subscribers = _server.GetCollection<SubscriberEmail>(collectionMailingList);
            return _subscribers;
        }


        public IMongoCollection<RssDocumentSingle> GetAllRssDocumentsFromDatabase()
        {
            return _rssDocumentCollection;
        }

        public RssDocumentSingle GetDocumentByIdFromDatabase(string id)
        {
            return _rssDocumentCollection.Find(x => x.Id == new ObjectId(id)).Single();
        }

        public void InsertToMailingList(string emailAddress, ICollection<string> subscriberList)
        {
            _subscribers = GetSubscribersList();
            SubscriberEmail newNewsletterMember = new SubscriberEmail()
            {
                EmailAddress = emailAddress,
                SubscriberList = subscriberList.ToList()
            };

            _subscribers.InsertOne(newNewsletterMember);
        }

        public void DeleteFromMailingList(string emailAddres)
        {
            _subscribers = GetSubscribersList();
            _subscribers.DeleteMany(x => x.EmailAddress == emailAddres);
        }
    }
}