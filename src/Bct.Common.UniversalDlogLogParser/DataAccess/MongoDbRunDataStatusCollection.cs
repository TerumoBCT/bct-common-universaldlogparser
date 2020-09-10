using System;
using System.Globalization;
using MongoDB.Driver;
using BCT.Common.Logging.Extensions;
using Bct.Common.LogParsing.Reader;
using MongoDB.Bson;
using Bct.Common.LogParsing.Records;

namespace Bct.Common.LogParsing.DataAccess
{
   /// <summary>
   /// 
   /// </summary>
   internal sealed class MongoDbRunDataStatusCollection
   {
      private MongoDbRunDataStatusCollection()
      {

      }

      private IMongoDatabase m_RunDataStatusDb;
      private IMongoDatabase m_RunDataDb;

      /// <summary>
      /// 
      /// </summary>
      /// <param name="runDataStatusDb"></param>
      /// <param name="runDataDb"></param>
      internal MongoDbRunDataStatusCollection(
         IMongoDatabase runDataStatusDb,
         IMongoDatabase runDataDb)
      {
         m_RunDataStatusDb = runDataStatusDb;
         m_RunDataDb = runDataDb;
      }

      /// <summary>
      /// 
      /// </summary>
      /// <param name="deviceSerialNumber"></param>
      /// <param name="logName"></param>
      /// <param name="logId"></param>
      /// <returns></returns>
      internal Enums.ParsingResult GetProcessingStatus(
          String deviceSerialNumber,
          String logName,
          out Guid logId)
      {
         return Query(deviceSerialNumber, logName, out logId);
      }

      /// <summary>
      /// 
      /// </summary>
      /// <param name="dbId"></param>
      /// <param name="deviceSerialNumber"></param>
      /// <param name="logName"></param>
      /// <param name="eParsingResult"></param>
      /// <param name="recordCount"></param>
      /// <param name="ts"></param>
      /// <returns>returns the Db Object Id of the record inserted. Or the passed one if its an update</returns>
      internal Guid SetProcessingStatus(
          Guid dbId,
          String deviceSerialNumber,
          String logName,
          Enums.ParsingResult eParsingResult,
          Int32 recordCount,
          TimeSpan ts)
      {
         Boolean bIsUpdate = false;

         if (true == Guid.Empty.Equals(dbId))
         {
            dbId = Guid.NewGuid();
         }
         else
         {
            bIsUpdate = true;
         }

         DateTime creationUTCTime = DateTime.UtcNow;
         MongoDbRunDataProcessingStatusRecord record = new MongoDbRunDataProcessingStatusRecord()
         {
            LogId = dbId,
            UTCTimeStamp = creationUTCTime,
            SerialNumber = deviceSerialNumber,
            LogName = logName,
            ParsingStatus = eParsingResult,
            ParsingStatusStr = eParsingResult.ToString(),
            RecordCount = recordCount,
            ProcessingTimeMillisecs = Convert.ToInt32(ts.TotalMilliseconds)
         };

         if (!bIsUpdate)
         {
            Insert(deviceSerialNumber, record);
         }
         else
         {
            Update(deviceSerialNumber, record);
         }

         return (dbId);
      }

      /// <summary>
      /// 
      /// </summary>
      /// <param name="logId"></param>
      /// <param name="serialNumber"></param>
      /// <returns></returns>
      internal Boolean RemoveProcessingStatus(
          Guid logId,
          String serialNumber)
      {
         Boolean retVal = false;

         try
         {
            String collectionName = DataLogReaderHelper.GetCollectionTypeBySerialNumber(serialNumber);
            IMongoCollection<MongoDbRunDataProcessingStatusRecord> statusCollection = m_RunDataStatusDb.GetCollection<MongoDbRunDataProcessingStatusRecord>(collectionName);
            if (statusCollection != null)
            {
               //statusCollection.IndexExists(
               var filter = Builders<MongoDbRunDataProcessingStatusRecord>.Filter.Eq(Constants.DB_RecordGuid, logId);
               var deleteResult = statusCollection.DeleteOne(filter);
               if (deleteResult.DeletedCount == 1)
               {
                  if (m_RunDataDb != null)
                  {

                     // TODO_CLG: This i think is incorrect, we might need to do the GetCollection<Dictionary<String,Object>
                     IMongoCollection<MongoDbRunDataProcessingStatusRecord> collection = m_RunDataDb.GetCollection<MongoDbRunDataProcessingStatusRecord>(collectionName);
                     if (collection != null)
                     {
                        deleteResult = collection.DeleteOne(filter);
                        if (1 == deleteResult.DeletedCount)
                        {
                           retVal = true;
                        }
                     }
                  }
               }
            }
         }
         catch (Exception ex)
         {
            BCTLoggerService.GetLogger<MongoDbRunDataStatusCollection>()
               .WithException(ex, String.Format(CultureInfo.InvariantCulture, "Exception for Serial#: {0} - LogId: {1}",
                                                serialNumber, logId))
               .Log();
         }

         return (retVal);
      }

      /// <summary>
      /// 
      /// </summary>
      /// <param name="deviceSerialNumber"></param>
      /// <param name="logName"></param>
      /// <returns></returns>
      internal Boolean RemoveProcessingStatus(
          String deviceSerialNumber,
          String logName)
      {
         Boolean retVal = false;

         try
         {
            String collectionName = DataLogReaderHelper.GetCollectionTypeBySerialNumber(deviceSerialNumber);
            IMongoCollection<MongoDbRunDataProcessingStatusRecord> statusCollection = m_RunDataStatusDb.GetCollection<MongoDbRunDataProcessingStatusRecord>(collectionName);
            if (statusCollection != null)
            {
               //statusCollection.IndexExists(
               var filter = Builders<MongoDbRunDataProcessingStatusRecord>.Filter.Eq(Constants.DB_LogName, logName);
               var deleteResult = statusCollection.DeleteOne(filter);
               if (deleteResult.DeletedCount == 1)
               {
                  if (m_RunDataDb != null)
                  {
                     // TODO_CLG: This i think is incorrect, we might need to do the GetCollection<Dictionary<String,Object>
                     IMongoCollection<MongoDbRunDataProcessingStatusRecord> collection = m_RunDataDb.GetCollection<MongoDbRunDataProcessingStatusRecord>(collectionName);
                     if (collection != null)
                     {
                        deleteResult = collection.DeleteOne(filter);
                        if (1 == deleteResult.DeletedCount)
                        {
                           retVal = true;
                        }
                     }
                  }
               }
            }
         }
         catch (Exception ex)
         {
            BCTLoggerService.GetLogger<MongoDbRunDataStatusCollection>()
               .WithException(ex, String.Format(CultureInfo.InvariantCulture, "Exception for Serial#: {0} - LogName: {1}",
                                                deviceSerialNumber, logName))
               .Log();
         }

         return (retVal);
      }

      /// <summary>
      /// 
      /// </summary>
      /// <param name="deviceSerialNumber"></param>
      /// <param name="record"></param>
      /// <returns></returns>
      internal Boolean Insert(
          String deviceSerialNumber,
          MongoDbRunDataProcessingStatusRecord record)
      {
         Boolean retVal = false;

         if (record != null)
         {
            try
            {
               String collectionName = DataLogReaderHelper.GetCollectionTypeBySerialNumber(deviceSerialNumber);
               IMongoCollection<MongoDbRunDataProcessingStatusRecord> collection = m_RunDataStatusDb.GetCollection<MongoDbRunDataProcessingStatusRecord>(collectionName);
               if (collection != null)
               {
                  collection.InsertOne(record);
                  retVal = true;
               }
            }
            catch (Exception ex)
            {
               BCTLoggerService.GetLogger<MongoDbRunDataStatusCollection>()
                  .WithException(ex, String.Format(CultureInfo.InvariantCulture, "Exception for Serial#: {0} - LogName: {1}",
                                                   deviceSerialNumber, record?.LogName))
                  .Log();
            }
         }

         return (retVal);
      }

      /// <summary>
      /// 
      /// </summary>
      /// <param name="deviceSerialNumber"></param>
      /// <param name="record"></param>
      internal Boolean Update(
          String deviceSerialNumber,
          MongoDbRunDataProcessingStatusRecord record)
      {
         Boolean retVal = false;
         if (record != null)
         {
            try
            {
               String collectionName = DataLogReaderHelper.GetCollectionTypeBySerialNumber(deviceSerialNumber);
               IMongoCollection<MongoDbRunDataProcessingStatusRecord> collection = m_RunDataStatusDb.GetCollection<MongoDbRunDataProcessingStatusRecord>(collectionName);
               if (collection != null)
               {
                  var filter = Builders<MongoDbRunDataProcessingStatusRecord>.Filter.Eq(r => r.LogId, record.LogId);
                  var update = Builders<MongoDbRunDataProcessingStatusRecord>.Update
                     .Set(e => e.ParsingStatus, record.ParsingStatus)
                     .Set(e => e.ParsingStatusStr, record.ParsingStatus.ToString())
                     .Set(e => e.RecordCount, record.RecordCount)
                     .Set(e => e.ProcessingTimeMillisecs, record.ProcessingTimeMillisecs)
                     .CurrentDate(e => e.UTCTimeStamp);

                  MongoDbRunDataProcessingStatusRecord replaced = collection.FindOneAndReplace(
                      filter,
                      record,
                      new FindOneAndReplaceOptions<MongoDbRunDataProcessingStatusRecord, MongoDbRunDataProcessingStatusRecord>()
                      {
                         IsUpsert = true,
                         ReturnDocument = ReturnDocument.After
                      });

                  retVal = (null != replaced);
               }
            }
            catch (Exception ex)
            {
               BCTLoggerService.GetLogger<MongoDbRunDataStatusCollection>()
                  .WithException(ex, String.Format(CultureInfo.InvariantCulture, "Exception for Serial#: {0} - LogName: {1}",
                                                   deviceSerialNumber, record?.LogName))
                  .Log();
            }
         }

         return (retVal);
      }

      /// <summary>
      /// 
      /// </summary>
      /// <param name="deviceSerialNumber"></param>
      /// <param name="logName"></param>
      /// <param name="logId"></param>
      /// <returns></returns>
      internal Enums.ParsingResult Query(
          String deviceSerialNumber,
          String logName,
          out Guid logId)
      {
         logId = Guid.Empty;
         Enums.ParsingResult retVal = Enums.ParsingResult.Unknown;

         try
         {
            String collectionName = DataLogReaderHelper.GetCollectionTypeBySerialNumber(deviceSerialNumber);
            IMongoCollection<MongoDbRunDataProcessingStatusRecord> collection = m_RunDataStatusDb.GetCollection<MongoDbRunDataProcessingStatusRecord>(collectionName);
            if (collection != null)
            {
               var filter = Builders<MongoDbRunDataProcessingStatusRecord>.Filter.Eq(Constants.DB_LogName, logName);
               var findResult = collection.Find(entry => entry.LogName == logName).FirstOrDefault();
               if (findResult != null)
               {
                  retVal = findResult.ParsingStatus;
                  logId = findResult.LogId;
               }

               System.Diagnostics.Debug.WriteLine(String.Format(CultureInfo.InvariantCulture, "Querying Log '{0}', status: '{1}'.",
                  logName, retVal));
            }
         }
         catch (Exception ex)
         {
            BCTLoggerService.GetLogger<MongoDbRunDataStatusCollection>()
               .WithException(ex, String.Format(CultureInfo.InvariantCulture, "Exception for Serial#: {0} - LogName: {1}",
                                                deviceSerialNumber, logName))
               .Log();
         }

         return (retVal);
      }

      /// <summary>
      /// 
      /// </summary>
      /// <param name="logDbId"></param>
      /// <param name="deviceSerialNumber"></param>
      /// <param name="startTime"></param>
      /// <param name="logHelper"></param>
      /// <param name="entity"></param>
      /// <returns></returns>
      internal ObjectId InsertRunDataRecord(
         String logDbId,
         String deviceSerialNumber,
         DateTime startTime,
         DataLogReaderHelper logHelper,
         DataLog_BaseRecord entity)
      {
         ObjectId dbId = ObjectId.Empty;
         BsonDocument documentToInsert = entity.GetPropertiesForStorage(logDbId, logHelper);
         if ((null != documentToInsert) &&
             (0 != documentToInsert.ElementCount))
         {
            try
            {
               String collectionName = DataLogReaderHelper.GetCollectionTypeBySerialNumber(deviceSerialNumber);
               IMongoCollection<BsonDocument> collection = m_RunDataDb.GetCollection<BsonDocument>(collectionName);
               if (collection != null)
               {
                  collection.InsertOne(documentToInsert);
                  dbId = documentToInsert[Constants.DB_MongoObjectId].AsObjectId;
               }
            }
            catch (Exception ex)
            {
               BCTLoggerService.GetLogger<MongoDbRunDataStatusCollection>()
                  .WithException(ex, String.Format(CultureInfo.InvariantCulture, "Exception for LogId: {0} - Serial#: {1}",
                                                   logDbId, deviceSerialNumber))
                  .Log();
            }

            return (dbId);
         }
         return (dbId);
      }
   }
}
