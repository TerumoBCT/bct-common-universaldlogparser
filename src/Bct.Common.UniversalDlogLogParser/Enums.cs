using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Bct.Common.LogParsing
{
   /// <summary>
   /// 
   /// </summary>
   public sealed class Enums
   {
      /// <summary>
      /// 
      /// </summary>
      public enum PrintArgType
      {
         /// <summary>
         /// 
         /// </summary>
         Invalid = -1,

         /// <summary>
         /// 
         /// </summary>
         Char = 1,

         /// <summary>
         /// 
         /// </summary>
         Int,

         /// <summary>
         /// 
         /// </summary>
         Long,

         /// <summary>
         /// 
         /// </summary>
         Float,

         /// <summary>
         /// 
         /// </summary>
         Double,

         /// <summary>
         /// 
         /// </summary>
         String,

         /// <summary>
         /// 
         /// </summary>
         Done,

         /// <summary>
         /// 
         /// </summary>
         Error
      };

      /// <summary>
      /// 
      /// </summary>
      public enum StreamArgType : byte
      {
         /// <summary>
         /// 
         /// </summary>
         Invalid = 0xFF,

         /// <summary>
         /// 
         /// </summary>
         SignedChar = 1,

         /// <summary>
         /// 
         /// </summary>
         UnsignedChar,

         /// <summary>
         /// 
         /// </summary>
         SignedInt,

         /// <summary>
         /// 
         /// </summary>
         UnsignedInt,

         /// <summary>
         /// 
         /// </summary>
         SignedLong,

         /// <summary>
         /// /
         /// </summary>
         UnsignedLong,

         /// <summary>
         /// 
         /// </summary>
         String,

         /// <summary>
         /// 
         /// </summary>
         Float,

         /// <summary>
         /// 
         /// </summary>
         Double,

         /// <summary>
         /// 
         /// </summary>
         Bool,

         /// <summary>
         /// 
         /// </summary>
         Flag = 100,

         /// <summary>
         ///
         /// </summary>
         Precision
      };

      /// <summary>
      /// 
      /// </summary>
      public enum ParsingResult : ushort
      {
         /// <summary>
         /// 
         /// </summary>
         Unknown = 0xFFFF,

         /// <summary>
         /// The file is ready for a Log Parser to process it
         /// </summary>
         ReadyForProcessing = 0,

         /// <summary>
         /// The file is being parsed. 
         /// </summary>
         ParsingInProcess = 1,

         /// <summary>
         /// 
         /// </summary>
         ParsingSuccess = 2,

         /// <summary>
         /// 
         /// </summary>
         ParsingPartialSuccess = 3,

         /// <summary>
         /// Last try for this file parse was unsuccessfull. 
         /// </summary>
         ParsingCorruptedFile = 4,

         /// <summary>
         /// The file has been processed multiple times (based on server policy), unsuccessfully.
         /// </summary>
         ParsingQuarantined = 5,

         /// <summary>
         /// The file not found
         /// </summary>
         SourceFileNotFound = 5,
      }

      /// <summary>
      /// 
      /// </summary>
      public enum RecordId : ushort
      {
         /// <summary>
         /// 
         /// </summary>
         HeaderRecordId = 0x5500,

         /// <summary>
         /// 
         /// </summary>
         LogLevelRecordId = 0x5501,

         /// <summary>
         /// 
         /// </summary>
         PrintOutputRecordId = 0x5502,

         /// <summary>
         /// 
         /// </summary>
         StreamOutputRecordId = 0x5503,

         /// <summary>
         /// 
         /// </summary>
         PeriodicOutputRecordId = 0x5504,

         /// <summary>
         /// 
         /// </summary>
         PeriodicSetRecordId = 0x5505,

         /// <summary>
         /// 
         /// </summary>
         PeriodicItemRecordId = 0x5506,

         /// <summary>
         /// 
         /// </summary>
         TaskCreateRecordId = 0x5507,

         /// <summary>
         /// 
         /// </summary>
         TaskDeleteRecordId = 0x5508,

         /// <summary>
         /// 
         /// </summary>
         NetworkHeaderRecordId = 0x5509,

         /// <summary>
         /// 
         /// </summary>
         BinaryRecordId = 0x55f0,

         /// <summary>
         /// 
         /// </summary>
         EndOfNetworkOutputRecordId = 0x55fc,

         /// <summary>
         /// 
         /// </summary>
         FileCloseRecordId = 0x55fd,

         /// <summary>
         /// 
         /// </summary>
         WriteTimeRecordId = 0x55fe,

         /// <summary>
         /// 
         /// </summary>
         MissedDataRecordId = 0x55ff
      }

      /// <summary>
      /// 
      /// </summary>
      public static Dictionary<RecordId, String> RecordIdStrings = new Dictionary<RecordId, String>()
      {
         {  RecordId.HeaderRecordId, "Header" },
         {  RecordId.LogLevelRecordId, "LogLevel" },
         {  RecordId.PrintOutputRecordId,"PrintOutput" },
         {  RecordId.StreamOutputRecordId, "Stream" },
         {  RecordId.PeriodicOutputRecordId, "PeriodicOutput" },
         {  RecordId.PeriodicSetRecordId, "PeriodicSet" },
         {  RecordId.PeriodicItemRecordId, "PeriodicItem" },
         {  RecordId.TaskCreateRecordId, "TaskCreate" },
         {  RecordId.TaskDeleteRecordId, "TaskDelete" },
         {  RecordId.NetworkHeaderRecordId, "NetworkHeader" },
         {  RecordId.BinaryRecordId, "Binary" },
         {  RecordId.EndOfNetworkOutputRecordId, "EndOfNetworkOutput" },
         {  RecordId.FileCloseRecordId, "FileClose" },
         {  RecordId.WriteTimeRecordId, "WriteTime" },
         {  RecordId.MissedDataRecordId, "MissedData" },
      };


      /// <summary>
      /// 
      /// </summary>
      public enum DataLog_ErrorType : int
      {
         /// <summary>
         /// must be first entry
         /// </summary>
         NoError = 0,

         /// <summary>
         /// 
         /// </summary>
         BadNetworkClientData,

         /// <summary>
         /// 
         /// </summary>
         MultipleInitialization,

         /// <summary>
         /// 
         /// </summary>
         NetworkConnectionFailed,

         /// <summary>
         /// 
         /// </summary>
         InvalidHandle,

         /// <summary>
         /// 
         /// </summary>
         LevelNotInitialized,

         /// <summary>
         /// 
         /// </summary>
         CriticalBufferMissing,

         /// <summary>
         /// 
         /// </summary>
         LevelConstructorFailed,

         /// <summary>
         /// 
         /// </summary>
         OpenOutputFileFailed,

         /// <summary>
         /// 
         /// </summary>
         LevelRecordWriteFailed,

         /// <summary>
         /// 
         /// </summary>
         PeriodicSetRecordWriteFailed,

         /// <summary>
         /// 
         /// </summary>
         PeriodicItemRecordWriteFailed,

         /// <summary>
         /// 
         /// </summary>
         PrintFormatError,
         /// <summary>
         /// 
         /// </summary>
         InternalWriteError,

         /// <summary>
         /// 
         /// </summary>
         PeriodicWriteError,

         /// <summary>
         /// 
         /// </summary>
         NotLogToFile,

         /// <summary>
         /// 
         /// </summary>
         BufferTooSmall,

         /// <summary>
         /// must be last entry
         /// </summary>
         LastError
      };
   }
}




