using System;
using System.Collections.Generic;
using System.Text;
using Bct.Common.LogParsing.Reader;
using Bct.Common.LogParsing.Records;
using MongoDB.Bson;

namespace Bct.Common.LogParsing.Sinks
{
   /// <summary>
   /// 
   /// </summary>
   public interface IDlogParsingOutputWriterSink : IDisposable
   {
      /// <summary>
      /// 
      /// </summary>
      String OutputName { get; }

      /// <summary>
      /// 
      /// </summary>
      String OutputPath { get;  }

                  
      /// <summary>
      /// 
      /// </summary>
      /// <param name="serialNumber"></param>
      /// <param name="plainFileName"></param>
      /// <param name="logId"></param>
      /// <returns></returns>
      Enums.ParsingResult GetProcessingStatus(
         String serialNumber,
         String plainFileName,
         out Guid logId );

      /// <summary>
      /// 
      /// </summary>
      /// <param name="serialNumber"></param>
      /// <param name="plainFileName"></param>
      /// <returns></returns>
      Boolean RemoveProcessingStatus(
         String serialNumber,
         String plainFileName);

      /// <summary>
      /// 
      /// </summary>
      /// <param name="logId"></param>
      /// <param name="serialNumber"></param>
      /// <param name="plainFileName"></param>
      /// <returns></returns>
      Boolean RemoveProcessingStatusByLogId(
         Guid logId,
         String serialNumber,
         String plainFileName);

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
      Guid SetProcessingStatus(
         Guid id,
         String serialNumber,
         String plainFileName,
         Enums.ParsingResult eParsingResult,
         Int32 recordCount,
         TimeSpan ts);

      /// <summary>
      /// 
      /// </summary>
      /// <param name="logDbId"></param>
      /// <param name="plainFileName"></param>
      /// <param name="serialNumber"></param>
      /// <param name="latestTimeStamp"></param>
      /// <param name="dlogHelper"></param>
      /// <param name="headerRecord"></param>
      /// <returns></returns>
      ObjectId WriteRecord(
         String logDbId,
         String plainFileName,
         String serialNumber,
         DateTime latestTimeStamp,
         DataLogReaderHelper dlogHelper,
         DataLog_BaseRecord headerRecord);

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
      ObjectId WriteFinalRecord(
         Guid id,
         String serialNumber,
         String plainFileName,
         Enums.ParsingResult eParsingResult,
         Int32 recordCount,
         TimeSpan ts,
         DataLogReaderHelper dlogHelper);
   }
}
