using Bct.Common.LogParsing;
using MongoDB.Bson;
using MongoDB.Driver;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Globalization;
using Bct.Common.LogParsing.Records;
using Bct.Common.LogParsing.Utils;

namespace Bct.Common.LogParsing.DataAccess
{
   /// <summary>
   /// 
   /// </summary>
   public static class MongoDBWrapper
   {
      private static Boolean ms_Initialized = false;

      /// <summary>
      /// 
      /// </summary>
      private static SmartObjectPool<IMongoDatabase> ms_RunDataDb = new SmartObjectPool<IMongoDatabase>();

      /// <summary>
      /// 
      /// </summary>
      public static SmartObjectPool<IMongoDatabase> RunDataDb
      {
         get
         {
            return ( ms_RunDataDb );
         }
      }

      /// <summary>
      /// 
      /// </summary>
      private static SmartObjectPool<IMongoDatabase> ms_RunDataStatusDb = new SmartObjectPool<IMongoDatabase>();

      /// <summary>
      /// 
      /// </summary>
      public static SmartObjectPool<IMongoDatabase> RunDataStatusDb
      {
         get
         {
            return ( ms_RunDataStatusDb );
         }
      }

      /// <summary>
      /// Static Constructor
      /// </summary>
      static MongoDBWrapper()
      {
      }

      private static void Initialize()
      {
         if (false == ms_Initialized)
         {
            String sConnString = ConfigurationManager.ConnectionStrings["DLOG_MONGODB"].ConnectionString;
            MongoClient client = new MongoClient(sConnString);

            BsonElement elementToAdd = new BsonElement("LoginTime", DataLog_BaseRecord.DateTimeToDbString(DateTime.UtcNow));
            BsonDocument documentToAdd = new BsonDocument(elementToAdd);


            // TODO_CLG: We should have a way to initialize the Db and collections and indexes before hand somehow, maybe per product "plug-in"
            IMongoDatabase database = client.GetDatabase(Constants.DBName_RunData);
            IMongoCollection<BsonDocument> loginsCollection = database.GetCollection<BsonDocument>("Logins");
            loginsCollection.InsertOne(documentToAdd, new InsertOneOptions() { BypassDocumentValidation = true });
            for (Int32 nPos = 0; nPos < 2 * 7; ++nPos)
            {
               database = client.GetDatabase(Constants.DBName_RunData);
               ms_RunDataDb.Add(database);
            }

            // TODO_CLG: We should have a way to initialize the Db and collections and indexes before hand somehow, maybe per product "plug-in"
            database = client.GetDatabase(Constants.DBName_RunDataStatus);
            loginsCollection = database.GetCollection<BsonDocument>("Logins");
            loginsCollection.InsertOne(documentToAdd, new InsertOneOptions() { BypassDocumentValidation = true });
            for (Int32 nPos = 0; nPos < 2 * 7; ++nPos)
            {
               database = client.GetDatabase(Constants.DBName_RunDataStatus);
               ms_RunDataStatusDb.Add(database);
            }
         }
      }

      /// <summary>
      ///       TODO_CLG: move it outside of mongo so it is generic
      /// </summary>
      /// <param name="serialNumber"></param>
      /// <returns></returns>
      internal static String GetCollectionBySerialNumber(
          String serialNumber)
      {
         return serialNumber.Substring(0, 2);
      }
   }
}
