using Bct.Common.LogParsing;
using Bct.Common.LogParsing.Reader;
using Bct.Common.LogParsing.Records;
using MongoDB.Bson;
using MongoDB.Driver;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Bct.Common.LogParsing.DataAccess
{

   /// <summary>
   /// 
   /// </summary>
   public sealed class MongoDbRunDataCollection
   {
      /// <summary>
      /// 
      /// </summary>
      /// <param name="logDbId"></param>
      /// <param name="deviceSerialNumber"></param>
      /// <param name="startTime"></param>
      /// <param name="logHelper"></param>
      /// <param name="entity"></param>
      /// <returns></returns>
      public static ObjectId InsertRecord(
          String logDbId,
          String deviceSerialNumber,
          DateTime startTime,
          DataLogReaderHelper logHelper,
          DataLog_BaseRecord entity)
      {
         ObjectId dbId = ObjectId.Empty;
         BsonDocument documentToInsert = entity.GetPropertiesForStorage(logDbId, logHelper);
         if (( null != documentToInsert ) &&
             ( 0 != documentToInsert.ElementCount ))
         {
            var dbRunData = MongoDBWrapper.RunDataDb.GetNoTimeout();
            if (null == dbRunData)
            {
               // TODO_LOG
               return ( dbId );
            }

            try
            {
               String collectionName = MongoDBWrapper.GetCollectionBySerialNumber(deviceSerialNumber);
               IMongoCollection<BsonDocument> collection = dbRunData.ObjInstance.GetCollection<BsonDocument>(collectionName);
               if (collection != null)
               {
                  collection.InsertOne(documentToInsert);
                  dbId = documentToInsert[Constants.DB_MongoObjectId].AsObjectId;
               }
            }
            catch (Exception)
            {
               // TODO_LOG
            }
            finally
            {
               MongoDBWrapper.RunDataDb.Release(dbRunData);
            }

            return ( dbId );
         }
         return ( dbId );
      }
   }
}
