using System;
using System.Collections.Generic;
using Bct.Common.LogParsing.DataAccess;
using Bct.Common.LogParsing.Models;
using Bct.Common.LogParsing.Reader;
using Bct.Common.LogParsing.Records;
using Elasticsearch.Net;
using MongoDB.Bson;
using Nest;

namespace Bct.Common.LogParsing.Sinks
{
   /// <summary>
   /// 
   /// </summary>
   public class ElasticSearchOutputSink : IDlogParsingOutputWriterSink
   {
      /// <summary>
      /// TODO_CLG: https://www.elastic.co/guide/en/elasticsearch/client/net-api/current/connection-pooling.html
      /// </summary>
      private StaticConnectionPool m_ConnectionPool;

      private ConnectionSettings m_RunDataIndexConnectionSettings;
      private ElasticClient m_RunDataIndexElasticClient;
      private const String _mc_RunDataIndexName = "run_data";

      private ConnectionSettings m_RunDataStatusIndexConnectionSettings;
      private ElasticClient m_RunDataStatusIndexElasticClient;
      private const String _mc_RunDataStatusIndexName = "run_data_status";

      //private ElasticSearchWrapper m_ElasticRunDataIndex;

      private ElasticSearchOutputSink()
      {
      }

      /// <summary>
      /// 
      /// </summary>
      /// <param name="elasticSearchConnectionString"></param>
      /// <param name="outputName"></param>
      public ElasticSearchOutputSink(
         Uri elasticSearchConnectionString,
         String outputName)
      {
         m_elasticSearchConnectionString = elasticSearchConnectionString;
         m_OutputName = outputName;

         m_ConnectionPool = new StaticConnectionPool(new Uri[] { m_elasticSearchConnectionString });

         // RunData
         m_RunDataIndexConnectionSettings = new ConnectionSettings(m_ConnectionPool);
         m_RunDataIndexElasticClient = new ElasticClient(m_RunDataIndexConnectionSettings);
         m_RunDataIndexConnectionSettings.DefaultIndex(_mc_RunDataIndexName);

         // RunDataStatus
         m_RunDataStatusIndexConnectionSettings = new ConnectionSettings(m_ConnectionPool);
         m_RunDataStatusIndexElasticClient = new ElasticClient(m_RunDataStatusIndexConnectionSettings);
         m_RunDataStatusIndexConnectionSettings.DefaultIndex(_mc_RunDataStatusIndexName);
      }

      /// <summary>
      /// 
      /// </summary>
      private Uri m_elasticSearchConnectionString;

      /// <summary>
      /// 
      /// </summary>
      public String OutputPath
      {
         get
         {
            return (m_elasticSearchConnectionString.ToString());
         }
      }

      /// <summary>
      /// 
      /// </summary>
      private String m_OutputName;

      /// <summary>
      /// 
      /// </summary>
      public String OutputName
      {
         get
         {
            return (m_OutputName);
         }
      }

      #region IParsingOutputWriterSink
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
         logId = Guid.Empty;
         Enums.ParsingResult parsingResult = Enums.ParsingResult.Unknown;

         try
         {
            ISearchResponse<ELKRunDataProcessingStatusRecord> searchResponse = getStatus(serialNumber, plainFileName);
            if (null != searchResponse)
            {
               if (searchResponse.Documents?.Count != 0)
               {
                  IEnumerator<ELKRunDataProcessingStatusRecord> enumerator = searchResponse.Documents.GetEnumerator();
                  if (null != enumerator)
                  {
                     logId = enumerator.Current.LogId;
                     // TODO_CESAR parsingResult = (Enums.ParsingResult)enumerator.Current.ParsingStatus;
                  }
               }
            }
         }
         catch (Exception)
         {
            // TODO_CLG: LOG
         }

         return (parsingResult);
      }

      /// <summary>
      /// 
      /// </summary>
      /// <param name="serialNumber"></param>
      /// <param name="plainFileName"></param>
      /// <returns></returns>
      public Boolean RemoveProcessingStatus(
         String serialNumber,
         String plainFileName)
      {
         try
         {
            var searchId = getStatus(serialNumber, plainFileName);

            var searchResponse = m_RunDataStatusIndexElasticClient.DeleteByQuery<ELKRunDataProcessingStatusRecord>(q => q.Query(rq => rq.Term(f => f.SerialNumber, plainFileName)));

            return (true);
         }
         catch (Exception)
         {
            // TODO_CLG: LOG

            return (false);
         }
      }

      /// <summary>
      /// 
      /// </summary>
      /// <param name="logId"></param>
      /// <param name="serialNumber"></param>
      /// <param name="plainFileName"></param>
      /// <returns></returns>
      public Boolean RemoveProcessingStatusByLogId(
         Guid logId, 
         String serialNumber, 
         String plainFileName)
      {
         try
         {
            var searchId = getStatus(serialNumber, plainFileName);

            var searchResponse = m_RunDataStatusIndexElasticClient.DeleteByQuery<ELKRunDataProcessingStatusRecord>(q => q.Query(rq => rq.Term(f => f.LogId, logId)));

            return (true);
         }
         catch (Exception)
         {
            // TODO_CLG: LOG

            return (false);
         }

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
         Guid returnId = Guid.Empty;

         try
         {
            if (Guid.Empty == id)
            {
               var searchResponse = m_RunDataStatusIndexElasticClient.Search<ELKRunDataProcessingStatusRecord>(q => q.Query(rq => rq.Term(f => f.SerialNumber, plainFileName)));
               if (false == searchResponse.IsValid)
               {
                  // TODO_CLG: We failed to find the index or connect. 
               }

               if (0 == searchResponse.Documents.Count)
               {
                  //var updatehResponse = m_RunDataStatusIndexElasticClient.Index<ELKRunDataProcessingStatusRecord>(i =>
                  //   i.Index
                  //u.Id(plainFileName)
                  // .Params(p => p.Add("LogName", plainFileName))
                  // .RetryOnConflict(3)
                  // .Refresh());
               }
            }
            else
            {
               //var updatehResponse = m_RunDataStatusIndexElasticClient.Update<ELKRunDataProcessingStatusRecord>(u =>
               //u.Id(plainFileName)
               // .Params(p => p.Add("LogName", plainFileName))
               // .RetryOnConflict(3)
               // .Refresh());
            }
         }
         catch (Exception)
         {
            // TODO_CLG: LOG
         }

         return (returnId);
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
         ObjectId objId = ObjectId.GenerateNewId();

         try
         {
            BsonDocument record = dlogRecord.GetPropertiesForStorage(logDbId, dlogHelper);
            if( null == record)
            {
               return (ObjectId.Empty);
            }
            String tableType = MongoDBWrapper.GetCollectionBySerialNumber(serialNumber);

            


            objId = ObjectId.GenerateNewId();
            Id searchId = new Id(objId.ToString());

            //IIndexResponse indexResponse = m_RunDataIndexElasticClient.Index<BsonDocument>(
            //   record, s =>
            //      s.Index(_mc_RunDataIndexName)
            //       .Type(tableType)
            //       .Id(searchId)
            //       .Refresh(Elasticsearch.Net.Refresh.WaitFor));


            var indexResponse = m_RunDataIndexElasticClient.IndexDocument<string>(record.Elements.ToJson());
            //record, s =>
            //   s.Index(_mc_RunDataIndexName)
            //    //.Type(tableType)
            //    .Id(searchId)
            //    .Refresh(Elasticsearch.Net.Refresh.WaitFor));
            if (null != indexResponse)
            {

            }
         }
         catch (Exception)
         {
            // TODO_CLG: LOG
         }

         return (objId);
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
         throw new NotImplementedException();
      }
      #endregion

      #region Private Internal methods for handling Elastic Search NEST API
      /// <summary>
      /// 
      /// </summary>
      /// <param name="serialNumber"></param>
      /// <param name="plainFileName"></param>
      /// <returns></returns>
      private ISearchResponse<ELKRunDataProcessingStatusRecord> getStatus(
         String serialNumber,
         String plainFileName)
      {
         var response = m_RunDataStatusIndexElasticClient.Search<ELKRunDataProcessingStatusRecord>(s =>
            s.Query(q =>
               q.Match(m =>
                  m.Field(f => f.LogName)
                  .Query(plainFileName))));
         if (response?.IsValid == false && response?.ServerError?.Status == 404)
         {
            var request = new IndexExistsRequest(_mc_RunDataStatusIndexName);
            var result = m_RunDataStatusIndexElasticClient.Indices.Exists(request);
            if (false == result.IsValid)
            {
               // TODO_CLG: This is the case where we cant connect to ElasticSearch
            }

            if (false == result.Exists)
            {
               var createResponse = m_RunDataStatusIndexElasticClient.Indices.Create(_mc_RunDataStatusIndexName);
               if (false == createResponse.IsValid)
               {
                  // TODO_CLG: Case that fails creating an index
               }

            }
         }

         //var response = m_RunDataStatusIndexElasticClient.Search<ELKRunDataProcessingStatusRecord>(s =>
         //              s.From(0)
         //               .SearchType(SearchType.QueryThenFetch)
         //               .Index(_mc_RunDataStatusIndexName)
         //               .Source(sr => sr
         //                 .Includes(fi => fi.Field("ParsingStatus")))
         //               .Query(q =>
         //                 q.Match(m =>
         //                   m.Field(f => f.SerialNumber)
         //                    .Query(serialNumber)) &&
         //                 q.Match(m =>
         //                    m.Field(f => f.LogName)
         //                     .Query(plainFileName))));

         return (null);
      }


      /// <summary>
      /// 
      /// </summary>
      /// <typeparam name="T"></typeparam>
      /// <param name="client"></param>
      /// <param name="data"></param>
      /// <param name="indexName"></param>
      /// <returns></returns>
      private Boolean IndexData<T>(
         ElasticClient client,
         T data,
         String indexName = null)
         where T : class, new()
      {
         if (client == null)
         {
            throw new ArgumentNullException("data");
         }
         var result = client.Index<T>(data, c => c.Index(indexName));
         return result.IsValid;
      }

      //private Boolean AddIndexes(
      //   ELKRunDataProcessingStatusRecord model)
      //{
      //   IIndexResponse indexResponse = m_RunDataIndexElasticClient.Index<ELKRunDataProcessingStatusRecord>(model, null);
      //   if (null == indexResponse)
      //   {
      //      return ( false );
      //   }

      //   return ( true );
      //}
      #endregion


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
