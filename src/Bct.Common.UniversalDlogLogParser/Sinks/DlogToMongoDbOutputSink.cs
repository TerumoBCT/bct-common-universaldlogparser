using System;
using Bct.Common.LogParsing.DataAccess;
using Bct.Common.LogParsing.Reader;
using Bct.Common.LogParsing.Records;
using MongoDB.Bson;
using MongoDB.Driver;

namespace Bct.Common.LogParsing.Sinks
{
   /// <summary>
   /// 
   /// </summary>
   public class DlogToMongoDbOutputSink : IDlogParsingOutputWriterSink
   {
      private MongoDbRunDataStatusCollection m_MongoDb;

      private DlogToMongoDbOutputSink()
      {
      }

      /// <summary>
      /// 
      /// </summary>
      /// <param name="mongoConnectionString"></param>
      /// <param name="outputName"></param>
      public DlogToMongoDbOutputSink(
         String mongoConnectionString,
         String outputName)
      {
         m_mongoConnectionString = mongoConnectionString;
         m_OutputName = outputName;

         MongoClient client = new MongoClient(m_mongoConnectionString);

         // TODO_CLG: We should have a way to initialize the Db and collections and indexes before hand somehow, maybe per product "plug-in"
         IMongoDatabase runDataStatus = client.GetDatabase(Constants.DBName_RunDataStatus);
         IMongoDatabase runData = client.GetDatabase(Constants.DBName_RunData);

         m_MongoDb = new MongoDbRunDataStatusCollection(runDataStatus, runData);
      }

      /// <summary>
      /// 
      /// </summary>
      private String m_mongoConnectionString;

      /// <summary>
      /// 
      /// </summary>
      public String OutputPath
      {
         get
         {
            return ( m_mongoConnectionString );
         }
      }

      /// <summary>
      /// 
      /// </summary>
      private readonly String m_OutputName;

      /// <summary>
      /// 
      /// </summary>
      public String OutputName
      {
         get
         {
            return ( m_OutputName );
         }
      }

      /// <summary>
      /// 
      /// </summary>
      /// <param name="serialNumber"></param>
      /// <param name="plainFileName"></param>
      /// <param name="logId"></param>
      /// <returns></returns>
      public Enums.ParsingResult GetProcessingStatus(
         String serialNumber,
         String plainFileName,
         out Guid logId )
      {
         return m_MongoDb.GetProcessingStatus(serialNumber, plainFileName, out logId );
      }

      /// <summary>
      /// 
      /// </summary>
      /// <param name="serialNumber"></param>
      /// <param name="logName"></param>
      /// <returns></returns>
      public Boolean RemoveProcessingStatus(
         String serialNumber,
         String logName)
      {
         // TODO_CLG: Also should remove the data (not just the status record
         return m_MongoDb.RemoveProcessingStatus(serialNumber, logName);
      }

      /// <summary>
      /// 
      /// </summary>
      /// <param name="logId"></param>
      /// <param name="serialNumber"></param>
      /// <param name="logName"></param>
      /// <returns></returns>
      public Boolean RemoveProcessingStatusByLogId(
         Guid logId,
         String serialNumber,
         String logName)
      {
         // TODO_CLG: Also should remove the data (not just the status record
         return m_MongoDb.RemoveProcessingStatus(logId, serialNumber);
      }

      /// <summary>
      /// 
      /// </summary>
      /// <param name="id"></param>
      /// <param name="serialNumber"></param>
      /// <param name="plainFileName"></param>
      /// <param name="eParsingResult"></param>
      /// <param name="recordCount"></param>
      /// <param name="ts"></param>
      /// <returns></returns>
      public Guid SetProcessingStatus(
         Guid id,
         String serialNumber,
         String plainFileName,
         Enums.ParsingResult eParsingResult,
         Int32 recordCount,
         TimeSpan ts)
      {
         return m_MongoDb.SetProcessingStatus(id, serialNumber, plainFileName, eParsingResult, recordCount, ts);
      }

      /// <summary>
      /// 
      /// </summary>
      /// <param name="logDbId"></param>
      /// <param name="plainFileName"></param>
      /// <param name="serialNumber"></param>
      /// <param name="latestTimeStamp"></param>
      /// <param name="dlogHelper"></param>
      /// <param name="dlogRecord"></param>
      /// <returns></returns>
      public ObjectId WriteRecord(
         String logDbId,
         String plainFileName,
         String serialNumber,
         DateTime latestTimeStamp,
         DataLogReaderHelper dlogHelper,
         DataLog_BaseRecord dlogRecord)
      {
         return m_MongoDb.InsertRunDataRecord(logDbId, serialNumber, latestTimeStamp, dlogHelper, dlogRecord);
      }

      /// <summary>
      /// TODO_CLG: Implement
      /// </summary>
      /// <param name="id"></param>
      /// <param name="serialNumber"></param>
      /// <param name="plainFileName"></param>
      /// <param name="eParsingResult"></param>
      /// <param name="recordCount"></param>
      /// <param name="ts"></param>
      /// <param name="dlogHelper"></param>
      /// <returns></returns>
      public ObjectId WriteFinalRecord(
         Guid id,
         String serialNumber,
         String plainFileName,
         Enums.ParsingResult eParsingResult,
         Int32 recordCount,
         TimeSpan ts,
         DataLogReaderHelper dlogHelper)
      {
         return ObjectId.GenerateNewId();
      }

      #region IDisposable Support
      /// <summary>
      /// 
      /// </summary>
      private Boolean disposedValue = false; // To detect redundant calls

      /// <summary>
      /// 
      /// </summary>
      /// <param name="disposing"></param>
      void Dispose(Boolean disposing)
      {
         if (!disposedValue)
         {
            if (disposing)
            {
               //closeWriter();
            }

            disposedValue = true;
         }
      }

      /// <summary>
      /// This code added to correctly implement the disposable pattern.
      /// </summary>
      public void Dispose()
      {
         // Do not change this code. Put cleanup code in Dispose(bool disposing) above.
         Dispose(true);
      }
      #endregion
   }
}
