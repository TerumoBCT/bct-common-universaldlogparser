using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Text;
using BCT.Common.Logging.Extensions;
using Bct.Common.LogParsing.Reader;
using Bct.Common.LogParsing.Records;
using MongoDB.Bson;
using MongoDB.Bson.IO;
using Newtonsoft.Json;

namespace Bct.Common.LogParsing.Sinks
{
   /// <summary>
   /// 
   /// </summary>
   public sealed class DlogToJsonFileOutputSink : IDlogParsingOutputWriterSink
   {
      /// <summary>
      /// 
      /// </summary>
      private DlogToJsonFileOutputSink()
      {
      }

      private const String mc_tempExtension = ".temp";


      private String m_OutputPath;
      private String m_OutputName;
      private String m_OutputFileName;
      private String m_ResultsOutputFileName;
      private FileStream m_fsOutput;
      private BufferedStream m_bsOutput;
      private StreamWriter m_srOutput;

      [Obsolete]   // TODO_CLG
      private static JsonWriterSettings ms_jsonWritterSettings = new JsonWriterSettings()
      {
         OutputMode = MongoDB.Bson.IO.JsonOutputMode.CanonicalExtendedJson,
         GuidRepresentation = GuidRepresentation.Standard,

      };
      private static JsonSerializerSettings ms_serializerSettings;

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

      /// <summary>
      /// 
      /// </summary>
      public String OutputPath
      {
         get
         {
            return (m_OutputPath);
         }
      }

      /// <summary>
      /// 
      /// </summary>
      /// <param name="outputPath"></param>
      /// <param name="outputName"></param>
      public DlogToJsonFileOutputSink(
         String outputPath,
         String outputName)
      {
         m_OutputPath = outputPath;
         m_OutputName = outputName;
         m_OutputFileName = Path.Combine(outputPath, outputName + ".json");
         m_ResultsOutputFileName = Path.Combine(outputPath, outputName + ".results.json");
         ms_serializerSettings = new JsonSerializerSettings()
         {
            Formatting = Formatting.None,
            NullValueHandling = NullValueHandling.Include,
            DateFormatHandling = DateFormatHandling.IsoDateFormat,
         };
      }

      /// <summary>
      /// 
      /// </summary>
      /// <param name="deviceSerialNumber"></param>
      /// <param name="logName"></param>
      /// <param name="logId"></param>
      /// <returns></returns>
      public Enums.ParsingResult GetProcessingStatus(
          String deviceSerialNumber,
          String logName,
          out Guid logId)
      {
         Enums.ParsingResult retVal = Enums.ParsingResult.Unknown;
         logId = Guid.Empty;

         if (true == File.Exists(m_OutputFileName))
         {
            retVal = Enums.ParsingResult.ParsingSuccess;
            logId = Guid.NewGuid();
         }
         else if (true == File.Exists(String.Concat(m_OutputFileName, mc_tempExtension)))
         {
            retVal = Enums.ParsingResult.ParsingInProcess;
            logId = Guid.NewGuid();
         }

         BCTLoggerService.GetLogger<DlogToJsonFileOutputSink>()
               .WithDebug(String.Format(CultureInfo.InvariantCulture, "Processing status for DLOG '{0}' is '{1}'. LogID assigned '{2}'.",
                                        m_OutputFileName, retVal, logId))
               .Log();

         return (retVal);
      }

      private Enums.ParsingResult getStatusFromResultsFile()
      {
         // TODO_CLG
         throw new NotImplementedException();
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
         Boolean retVal = false;

         String outputFileName = String.Concat(m_OutputFileName, mc_tempExtension);
         if (true == File.Exists(outputFileName))
         {
            try
            {
               File.Delete(outputFileName);
               retVal = true;
            }
            catch (Exception ex)
            {
               BCTLoggerService.GetLogger<DlogToJsonFileOutputSink>()
                     .WithException(ex, String.Format(CultureInfo.InvariantCulture, "Error removing processing status for LogId:'{0}' for Serial#: '{1}', File: '{2}'.",
                                                      logId, serialNumber, outputFileName))
                     .Log();
            }
         }
         else
         {
            retVal = true;
         }

         if (true == File.Exists(m_OutputFileName))
         {
            try
            {
               File.Delete(m_OutputFileName);
               retVal = true;
            }
            catch (Exception ex)
            {
               BCTLoggerService.GetLogger<DlogToJsonFileOutputSink>()
                     .WithException(ex, String.Format(CultureInfo.InvariantCulture, "Error removing processing status for LogId:'{0}' for Serial#: '{1}', File: '{2}'.",
                                                      logId, serialNumber, m_OutputFileName))
                     .Log();
            }
         }
         else
         {
            retVal = true;
         }

         if (true == File.Exists(m_ResultsOutputFileName))
         {
            try
            {
               File.Delete(m_ResultsOutputFileName);
               retVal = true;
            }
            catch (Exception ex)
            {
               BCTLoggerService.GetLogger<DlogToJsonFileOutputSink>()
                     .WithException(ex, String.Format(CultureInfo.InvariantCulture, "Error removing processing status result file for LogId:'{0}' for Serial#: '{1}', File: '{2}'.",
                                                      logId, serialNumber, m_ResultsOutputFileName))
                     .Log();
               retVal = false;
            }
         }

         return (retVal);
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
         Boolean retVal = false;

         String outputFileName = String.Concat(m_OutputFileName, mc_tempExtension);
         if (true == File.Exists(outputFileName))
         {
            try
            {
               File.Delete(outputFileName);
               retVal = true;
            }
            catch (Exception ex)
            {
               BCTLoggerService.GetLogger<DlogToJsonFileOutputSink>()
                     .WithException(ex, String.Format(CultureInfo.InvariantCulture, "Error removing processing status for Serial#: {0} - File: {1}",
                                                      serialNumber, outputFileName))
                     .Log();
            }
         }
         else
         {
            retVal = true;
         }

         if (true == File.Exists(m_OutputFileName))
         {
            try
            {
               File.Delete(m_OutputFileName);
               retVal = true;
            }
            catch (Exception ex)
            {
               BCTLoggerService.GetLogger<DlogToJsonFileOutputSink>()
                     .WithException(ex, String.Format(CultureInfo.InvariantCulture, "Error removing processing status for Serial#: {0} - File: {1}",
                                                      serialNumber, m_OutputFileName))
                     .Log();
            }
         }
         else
         {
            retVal = true;
         }

         if (true == File.Exists(m_ResultsOutputFileName))
         {
            try
            {
               File.Delete(outputFileName);
               retVal = true;
            }
            catch (Exception ex)
            {
               BCTLoggerService.GetLogger<DlogToJsonFileOutputSink>()
                     .WithException(ex, String.Format(CultureInfo.InvariantCulture, "Error removing processing status for Serial#: {0} - File: {1}",
                                                      serialNumber, m_ResultsOutputFileName))
                     .Log();
               retVal = false;
            }
         }
         else
         {
            retVal = true;
         }

         return (retVal);
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
      public Guid SetProcessingStatus(
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

         String outputFileName = String.Concat(m_OutputFileName, mc_tempExtension);
         String processedFileName = m_OutputFileName;

         if (true == bIsUpdate)
         {
            if (true == File.Exists(outputFileName))
            {
               if (true == File.Exists(processedFileName))
               {
                  File.Delete(processedFileName);
               }
               closeWriter();

               File.Move(outputFileName, processedFileName);
            }
         }

         BCTLoggerService.GetLogger<DlogToJsonFileOutputSink>()
               .WithDebug(String.Format(CultureInfo.InvariantCulture, "Setting processing status for DLOG '{0}' to '{1}'.",
                                        m_OutputFileName, eParsingResult))
               .Log();
         return (dbId);
      }

      /// <summary>
      /// 
      /// </summary>
      /// <param name="logDbId"></param>
      /// <param name="plainFileName"></param>
      /// <param name="serialNumber"></param>
      /// <param name="startTime"></param>
      /// <param name="logHelper"></param>
      /// <param name="entity"></param>
      /// <returns></returns>
      public ObjectId WriteRecord(
         String logDbId,
         String plainFileName,
         String serialNumber,
         DateTime startTime,
         DataLogReaderHelper logHelper,
         DataLog_BaseRecord entity)
      {
         StreamWriter writer = getWriter();
         if (null != writer)
         {
            BsonDocument documentToInsert = entity.GetPropertiesForStorage(logDbId, logHelper);
            if (null != documentToInsert)
            {
               // TODO_CLG: Find the most performant code (Lower IO, Lower CPU) that also outputs the right format for everything
               // Other possible options
               // String test = documentToInsert.ToJson<BsonDocument>(ms_jsonWritterSettings);
               // String test = Newtonsoft.Json.JsonConvert.SerializeObject(documentToInsert, Formatting.None, ms_serializerSettings);
               // String test = JsonConvert.SerializeObject(documentToInsert.ToJson, Formatting.None, m_serializerSettings);
               // String test = documentToInsert.ToString();
               String lineToWrite = Newtonsoft.Json.JsonConvert.SerializeObject(BsonTypeMapper.MapToDotNetValue(documentToInsert));
               writer.WriteLine(lineToWrite);
               return ObjectId.GenerateNewId();
            }
         }

         return (ObjectId.Empty);
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
      /// <param name="dlogHelper"></param>
      /// <returns></returns>
      public ObjectId WriteFinalRecord(
          Guid dbId,
          String deviceSerialNumber,
          String logName,
          Enums.ParsingResult eParsingResult,
          Int32 recordCount,
          TimeSpan ts,
          DataLogReaderHelper dlogHelper)
      {
         ObjectId retVal = ObjectId.GenerateNewId();

         StreamWriter jsonFileWriter = getWriter();
         if (null != jsonFileWriter)
         {
            using (TextWriter writer = new StreamWriter(m_ResultsOutputFileName, false, Encoding.ASCII))  // Arbitrary number that seems to perform well.
            {
               // TODO_CLG: This is not the best code. fix it
               writer.WriteLine(String.Format(CultureInfo.InvariantCulture, "{{\"SerialNumber\":\"{0}\",\"LogName\":\"{1}\",\"ParsingResult\":\"{2}\",\"RecordCount\":{3},\"TimeSpan\":\"{4}\",\"LogLevels\":{5},\"PeriodicSets\":{6},\"PeriodicItems\":{7}}}",
                                              deviceSerialNumber, logName, eParsingResult, recordCount, ts,
                                              Newtonsoft.Json.JsonConvert.SerializeObject(dlogHelper.LogLevels),
                                              Newtonsoft.Json.JsonConvert.SerializeObject(dlogHelper.PeriodicSets),
                                              Newtonsoft.Json.JsonConvert.SerializeObject(dlogHelper.PeriodicItems)));
               writer.Flush();
            }
         }

         return (retVal);
      }


      private StreamWriter getWriter()
      {
         lock (this)
         {
            if (null == m_srOutput)
            {
               String outputFileName = String.Concat(m_OutputFileName, mc_tempExtension);

               m_fsOutput = File.Open(outputFileName, FileMode.Create, FileAccess.Write, FileShare.None);

               m_bsOutput = new BufferedStream(m_fsOutput);

               m_srOutput = new StreamWriter(m_bsOutput);

            }
         }

         return (m_srOutput);
      }


      /// <summary>
      /// 
      /// </summary>
      private void closeWriter()
      {
         if (null != m_srOutput)
         {
            m_srOutput.Close();
            m_srOutput.Dispose();
            m_srOutput = null;
         }

         if (null != m_bsOutput)
         {
            m_bsOutput.Close();
            m_bsOutput.Dispose();
            m_bsOutput = null;
         }

         if (null != m_fsOutput)
         {
            m_fsOutput.Close();
            m_fsOutput.Dispose();
            m_fsOutput = null;
         }

      }


      #region IDisposable Support
      private bool disposedValue = false; // To detect redundant calls

      void Dispose(bool disposing)
      {
         if (!disposedValue)
         {
            if (disposing)
            {
               closeWriter();
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
