using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using BCT.Common.Logging.Extensions;
using Bct.Common.LogParsing;
using Bct.Common.LogParsing.DataAccess;
using Bct.Common.LogParsing.Reader;
using Bct.Common.LogParsing.Records;
using Bct.Common.LogParsing.Sinks;
using Bct.Common.LogParsing.Utils;
using MongoDB.Bson;

namespace Bct.Common.LogParsing.Sinks
{
   /// <summary>
   /// 
   /// </summary>
   public sealed class DlogBufferedInputReaderSink : IDlogParsingInputReaderSink
   {
      /// <summary>
      /// 
      /// </summary>
      private const String _mc_plainTextMsg = "CONFIDENTIAL: This file is intended only for the use of Terumo BCT and contains information that "
                           + "is proprietary and confidential. You are hereby notified that any use, dissemination, distribution, "
                           + "or copying of this file is strictly prohibited."
                           + "\nLog file: ";

      /// <summary>
      /// 
      /// </summary>
      private DlogBufferedInputReaderSink()
      {
      }

      private IDlogParsingOutputWriterSink m_Sink;
      private String m_InputPath;
      private String m_InputFileName;
      private String m_SerialNumber;

      /// <summary>
      /// 
      /// </summary>
      /// <param name="inputPath"></param>
      /// <param name="inputFileName"></param>
      /// <param name="sink"></param>
      public DlogBufferedInputReaderSink(
         String inputPath,
         String inputFileName,
         IDlogParsingOutputWriterSink sink)
      {
         m_InputPath = inputPath;
         m_InputFileName = inputFileName;
         m_SerialNumber = inputFileName.Substring(0, inputFileName.IndexOf('_', 0));
         m_Sink = sink;
      }

      #region Read
      /// <summary>
      /// 
      /// </summary>
      private static Byte[] mc_DLOGStartDataCode = new Byte[3] { 0x1A, 0x04, 0x00 };

      /// <summary>
      /// 
      /// </summary>
      private const UInt32 _mc_ByteOrderCode = 0x12345678;

      /// <summary>
      /// 
      /// </summary>
      public String FileName
      {
         get
         {
            return (m_InputFileName);
         }
      }

      /// <summary>
      /// 
      /// </summary>
      public String SerialNumber
      {
         get
         {
            return (m_SerialNumber);
         }
      }

      /// <summary>
      /// 
      /// </summary>
      public String InputPath
      {
         get
         {
            return (m_InputPath);
         }
      }

      /// <summary>
      /// 
      /// </summary>
      /// <returns></returns>
      public Enums.ParsingResult Read()
      {
         BCTLoggerService.GetLogger<DlogInputReaderSink>()
               .WithDebug(String.Format(CultureInfo.InvariantCulture, "Starting parsing of: {0}",
                                        Path.Combine(m_InputPath, m_InputFileName)))
               .Log();

         String inputFileName = Path.Combine(m_InputPath, m_InputFileName);
         if (!File.Exists(inputFileName))
         {
            BCTLoggerService.GetLogger<DlogInputReaderSink>()
                  .WithDebug(String.Format(CultureInfo.InvariantCulture, "File does not exist: {0}",
                                           inputFileName))
                  .Log();
            return (Enums.ParsingResult.SourceFileNotFound);
         }

         String path = Path.GetDirectoryName(inputFileName);
         String plainFileName = Path.GetFileNameWithoutExtension(inputFileName);
         String[] nameParts = plainFileName.Split('_');
         String serialNumber = nameParts[0];

         // We process the file
         DataLogReaderHelper dlogHelper = null;
         Byte[] byRecordType = new Byte[sizeof(Enums.RecordId)];
         Int32 recordCount = 0;
         Guid logId = Guid.Empty;
         Enums.ParsingResult eParsingResult = m_Sink.GetProcessingStatus(serialNumber, plainFileName, out logId);
         switch (eParsingResult)
         {
            // TODO_CLG: What do we do if was partial success
            case Enums.ParsingResult.ParsingSuccess:
            case Enums.ParsingResult.ParsingPartialSuccess:
               BCTLoggerService.GetLogger<DlogInputReaderSink>()
                     .WithDebug(String.Format(CultureInfo.InvariantCulture, "DLOG '{0}' already parsed with result {1}.",
                                              Path.Combine(m_InputPath, m_InputFileName), eParsingResult))
                     .Log();
               return (eParsingResult);

            // TODO_CLG: This is just for this tool
            case Enums.ParsingResult.ParsingInProcess:
            case Enums.ParsingResult.ParsingCorruptedFile:
               BCTLoggerService.GetLogger<DlogInputReaderSink>()
                     .WithDebug(String.Format(CultureInfo.InvariantCulture, "DLOG '{0}' found with status result {1}, removing processing status.",
                                              Path.Combine(m_InputPath, m_InputFileName), eParsingResult))
                     .Log();

               m_Sink?.RemoveProcessingStatusByLogId(logId, serialNumber, plainFileName);
               break;
         }

         // Get the stream of the source file.
         Byte[] readBuffer = new Byte[8192];
         Int32 nRead = 0;

         // Create a Record for this Log
         eParsingResult = Enums.ParsingResult.ParsingInProcess;
         DateTime dtStart = DateTime.Now;
         TimeSpan ts = dtStart - dtStart;


         Guid dlogDbId = Guid.Empty;
         if (null != m_Sink)
         {
            dlogDbId = m_Sink.SetProcessingStatus(Guid.Empty, serialNumber, plainFileName, eParsingResult, recordCount, ts);
         }

         try
         {
            using (FileStream fsInput = File.Open(inputFileName, FileMode.Open, FileAccess.Read, FileShare.Read))
            {
               using (BufferedStream bsInput = new BufferedStream(fsInput))
               {
                  using (StreamReader inFile = new StreamReader(bsInput))
                  {
                     if (ReadUntilHeaderRecord(inFile, ref dlogHelper) >= 0)
                     {
                        if (null == dlogHelper.DeviceSerialNumber)
                        {
                           dlogHelper.DeviceSerialNumber = serialNumber;
                        }

                        ObjectId insertedDataRecordId = ObjectId.Empty;
                        if (null != m_Sink)
                        {
                           insertedDataRecordId = m_Sink.WriteRecord(plainFileName, plainFileName, serialNumber, dlogHelper.HeaderRecord._latestTimeStamp, dlogHelper, dlogHelper.HeaderRecord);
                        }

                        if (ObjectId.Empty != insertedDataRecordId)
                        {
                           ++recordCount;
                        }

                        dlogHelper.DeviceSerialNumber = serialNumber;
                        using (GZipStream decompressStream = new GZipStream(inFile.BaseStream, CompressionMode.Decompress))
                        {
                           do
                           {
                              // Make sure we do not try to read from after the EOF
                              if (inFile.BaseStream.Position >= inFile.BaseStream.Length)
                              {
                                 // Maybe we need a flag that tells us we read the "eof" record then we can set success or partial
                                 eParsingResult = Enums.ParsingResult.ParsingPartialSuccess;
                                 break;
                              }

                              // Handle partial files with a try /catch block
                              try
                              {
                                 nRead = decompressStream.Read(byRecordType, 0, byRecordType.Length);
                              }
                              catch (Exception ex)
                              {
                                 BCTLoggerService.GetLogger<DlogInputReaderSink>()
                                       .WithException(ex, String.Format(CultureInfo.InvariantCulture, "Processing file: {0}",
                                                                inputFileName))
                                       .Log();
                                 eParsingResult = Enums.ParsingResult.ParsingPartialSuccess;
                                 break;
                              }

                              if (nRead > 0)
                              {
                                 byRecordType = DataLogEncryption.DefaultDecryptFunc(byRecordType, byRecordType.Length);
                                 Enums.RecordId recordType = (Enums.RecordId)BitConverter.ToUInt16(byRecordType, 0);
                                 DataLog_BaseRecord currentRecord = ProcessDLogRecord(recordType, dlogHelper, decompressStream);
                                 if (currentRecord != null)
                                 {
                                    if ((Enums.RecordId.PeriodicSetRecordId != currentRecord._recordType) &&
                                        (Enums.RecordId.MissedDataRecordId != currentRecord._recordType))
                                    {
                                       // PeriodiSetRecordID and PeriodiItemRecordId are not recorded since we only care about the PeriodicOutputRecord
                                       if (null != m_Sink)
                                       {
                                          insertedDataRecordId = m_Sink.WriteRecord(plainFileName, plainFileName, serialNumber, dlogHelper.HeaderRecord._latestTimeStamp, dlogHelper, currentRecord);
                                       }
                                       if (ObjectId.Empty != insertedDataRecordId)
                                       {
                                          ++recordCount;
                                       }
                                    }
                                 }
                                 else
                                 {
                                    BCTLoggerService.GetLogger<DlogInputReaderSink>()
                                          .WithError(String.Format(CultureInfo.InvariantCulture, "Error Processing file: {0}",
                                                                   inputFileName))
                                          .Log();
                                    eParsingResult = Enums.ParsingResult.ParsingPartialSuccess;
                                    break;
                                 }
                              }
                           }
                           while (nRead > 0);

                           if (Enums.ParsingResult.ParsingPartialSuccess == eParsingResult)
                           {
                              BCTLoggerService.GetLogger<DlogInputReaderSink>()
                                    .WithDebug(String.Format(CultureInfo.InvariantCulture, "Success parsing file: {0}",
                                                             inputFileName))
                                    .Log();
                              eParsingResult = Enums.ParsingResult.ParsingSuccess;
                           }
                        }
                     }
                     else
                     {
                        BCTLoggerService.GetLogger<DlogInputReaderSink>()
                              .WithInformation(String.Format(CultureInfo.InvariantCulture, "Parsed corrupted file : {0}",
                                                       inputFileName))
                              .Log();
                        eParsingResult = Enums.ParsingResult.ParsingCorruptedFile;
                     }
                  }
               }
            }
         }
         catch (Exception ex2)
         {
            BCTLoggerService.GetLogger<DlogInputReaderSink>()
                  .WithException(ex2, String.Format(CultureInfo.InvariantCulture, "Corrupted file: {0}",
                                           inputFileName))
                  .Log();
            eParsingResult = Enums.ParsingResult.ParsingCorruptedFile;
         }

         ts = DateTime.Now - dtStart;
         if (null != m_Sink)
         {
            m_Sink.WriteFinalRecord(dlogDbId, serialNumber, plainFileName, eParsingResult, recordCount, ts, dlogHelper);
            dlogDbId = m_Sink.SetProcessingStatus(dlogDbId, serialNumber, plainFileName, eParsingResult, recordCount, ts);
         }
         return (eParsingResult);
      }

      /// <summary>
      /// 
      /// </summary>
      /// <param name="recordType"></param>
      /// <param name="dlogHelper"></param>
      /// <param name="decompressStream"></param>
      /// <returns></returns>
      private static DataLog_BaseRecord ProcessDLogRecord(
          Enums.RecordId recordType,
          DataLogReaderHelper dlogHelper,
          GZipStream decompressStream)
      {
         // TODO_IMPROVE: We should have a way to have a list where we get the processor for each message type avoiding the case statement
         DataLog_BaseRecord currentRecord = DataLogRecordFactory.Create(recordType);
         if (currentRecord == null)
         {
            return (null);
         }

         if (!currentRecord.PopulateFromCompressedStream(decompressStream, dlogHelper))
         {
            return (null);
         }

         switch (currentRecord._recordType)
         {
            case Enums.RecordId.LogLevelRecordId:
               {
                  DataLog_LogLevelRecord record = (DataLog_LogLevelRecord)currentRecord;
                  dlogHelper.LogLevels.AddLogLevel(record._nodeId, record._levelID, record._name);
                  break;
               }

            case Enums.RecordId.PeriodicSetRecordId:
               {
                  DataLog_PeriodicSetRecord record = (DataLog_PeriodicSetRecord)currentRecord;
                  dlogHelper.PeriodicSets.AddPeriodicSet(record._setID, record._name);
               }
               break;
            case Enums.RecordId.PeriodicItemRecordId:
               {
                  DataLog_PeriodicItemRecord record = (DataLog_PeriodicItemRecord)currentRecord;
                  dlogHelper.PeriodicItems.AddPeriodicItem(record._keyCode, DataLog_BaseRecord.SanitizeFieldName(record._key), record._description, record._format);
               }
               break;
            case Enums.RecordId.WriteTimeRecordId:
               {
                  DataLog_WriteTimeRecord record = (DataLog_WriteTimeRecord)currentRecord;
                  DateTime dtTime = dlogHelper.HeaderRecord._timeStampStart;
                  dtTime = dtTime.AddMilliseconds(record._timeStamp._nanoseconds / 1000000);
                  dlogHelper.HeaderRecord._latestTimeStamp = dtTime.AddSeconds(record._timeStamp._seconds);
               }
               break;

               // TODO_CLG: What about the rest of the cases
         }

         return (currentRecord);
      }

      /// <summary>
      /// 
      /// </summary>
      /// <param name="inFile"></param>
      /// <param name="oDlogStart"></param>
      /// <returns></returns>
      private static Int64 ReadUntilHeaderRecord(
          StreamReader inFile,
          ref DataLogReaderHelper oDlogStart)
      {
         Byte[] readBuffer = new Byte[4096];
         Byte[] bofBuffer = null;
         Int32 nSearchOffset = 0;
         Int32 nRead = 0;
         do
         {
            nRead = inFile.BaseStream.Read(readBuffer, 0, readBuffer.Length);
            if (nRead > 0)
            {
               if (bofBuffer == null)
               {
                  bofBuffer = new Byte[nRead];
                  Buffer.BlockCopy(readBuffer, 0, bofBuffer, 0, nRead);
               }
               else
               {
                  Byte[] tmpBuffer = new Byte[bofBuffer.Length + nRead];
                  Buffer.BlockCopy(bofBuffer, 0, tmpBuffer, 0, bofBuffer.Length);
                  Buffer.BlockCopy(readBuffer, 0, tmpBuffer, bofBuffer.Length, readBuffer.Length);
                  nSearchOffset = bofBuffer.Length - mc_DLOGStartDataCode.Length;
                  bofBuffer = tmpBuffer;
               }

               Int32 nStartDataOffset = bofBuffer.LocateFirst(mc_DLOGStartDataCode);
               if (nStartDataOffset != 0)
               {
                  nStartDataOffset += mc_DLOGStartDataCode.Length;
                  oDlogStart = new DataLogReaderHelper();

                  Byte[] logInfoBuffer = new Byte[nStartDataOffset];
                  Buffer.BlockCopy(bofBuffer, 0, logInfoBuffer, 0, nStartDataOffset);
                  oDlogStart.Info = ProcessLogInfo(logInfoBuffer);

                  // Get 4 bytes from offset
                  Int32 byteOrderMark = BitConverter.ToInt32(bofBuffer, nStartDataOffset);
                  if (byteOrderMark != _mc_ByteOrderCode)
                  {
                     // TODO_FUTURE: then compare with reverse byte order and see if it matches, if it does. we need to 'read" accordingly
                  }

                  oDlogStart.HeaderRecord = (DataLog_HeaderRecord)DataLogRecordFactory.Create(Enums.RecordId.HeaderRecordId);
                  inFile.BaseStream.Seek(nStartDataOffset + sizeof(Int32), SeekOrigin.Begin);

                  // Do not dispose this BinaryReader object so the inFile object doesnt get disposed as well.
                  BinaryReader reader = new BinaryReader(inFile.BaseStream);
                  oDlogStart.HeaderRecord.PopulateFromReader(reader);

                  return (inFile.BaseStream.Position);
               }
            }
         } while (nRead > 0);

         return -1;
      }

      /// <summary>
      /// 
      /// </summary>
      /// <param name="buffer"></param>
      /// <returns></returns>
      private static RunDataInfo ProcessLogInfo(
          Byte[] buffer)
      {
         RunDataInfo info = new RunDataInfo();
         String message = Encoding.ASCII.GetString(buffer);
         Int32 nPos = message.IndexOf("Log file:");
         if (nPos != -1)
         {
            Int32 nNextLine = message.IndexOf("\n", nPos);
            info.LogFile = message.Substring(nPos, nNextLine - nPos);
            info.DeviceInfo = message.Substring(++nNextLine);
         }

         return (info);
      }
      #endregion

      #region IDisposable Support
      /// <summary>
      /// To detect redundant calls
      /// </summary>
      private Boolean disposedValue = false;

      /// <summary>
      /// Dispose pattern implementation
      /// </summary>
      /// <param name="disposing"></param>
      void Dispose(
         Boolean disposing)
      {
         if (!disposedValue)
         {
            if (disposing)
            {
               if (null != m_Sink)
               {
                  m_Sink.Dispose();
                  m_Sink = null;
               }
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


