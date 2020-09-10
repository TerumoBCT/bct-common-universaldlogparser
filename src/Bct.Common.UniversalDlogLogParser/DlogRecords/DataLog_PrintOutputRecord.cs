using System;
using System.Collections.Generic;
using System.IO.Compression;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MongoDB.Bson;
using Bct.Common.LogParsing.Reader;

namespace Bct.Common.LogParsing.Records
{
   /// <summary>
   /// 
   /// </summary>
   public class DataLog_PrintOutputRecord : DataLog_BaseRecord
   {
      /// <summary>
      /// 
      /// </summary>
      public DataLog_PrintOutputRecord() :
          base()
      {
         _recordType = Enums.RecordId.PrintOutputRecordId;
      }

      /// <summary>
      /// 
      /// </summary>
      public DataLog_TimeStamp _timeStamp { get; set; }

      /// <summary>
      /// 
      /// </summary>
      public UInt16 _levelID { get; set; }

      /// <summary>
      /// 
      /// </summary>
      public UInt32 _taskID { get; set; }

      /// <summary>
      /// 
      /// </summary>
      public UInt32 _nodeID { get; set; }

      /// <summary>
      /// 
      /// </summary>
      public UInt16 _formatLen { get; set; }

      /// <summary>
      /// 
      /// </summary>
      public UInt16 _fileNameLen { get; set; }

      /// <summary>
      /// 
      /// </summary>
      public UInt16 _lineNum { get; set; }

      /// <summary>
      /// 
      /// </summary>
      public String _format { get; set; }

      /// <summary>
      /// 
      /// </summary>
      public String _fileName { get; set; }

      /// <summary>
      /// 
      /// </summary>
      public String _message { get; set; }

      #region Base class override
      /// <summary>
      /// 
      /// </summary>
      /// <param name="logDbId"></param>
      /// <param name="logHelper"></param>
      /// <returns></returns>
      public override BsonDocument GetPropertiesForStorage(
          String logDbId,
          DataLogReaderHelper logHelper)
      {
         BsonDocument properties = AddCommonProperties(null, logHelper.DeviceSerialNumber, logDbId, logHelper.HeaderRecord._timeStampStart, this._timeStamp, this._recordType, this._levelID, logHelper.LogLevels.GetLevelString(this._nodeID, this._levelID), this._taskID, logHelper.TaskNames[this._taskID], this._nodeID, logHelper.NodeNames[this._nodeID]);

         properties.Add(Constants.DB_Message, base.FormatCodeMessage(this._fileName, this._lineNum, this._message));
         return ( properties );
      }

      /// <summary>
      /// 
      /// </summary>
      /// <param name="reader"></param>
      /// <param name="helper"></param>
      /// <returns></returns>
      public override Boolean PopulateFromCompressedStream(
          GZipStream reader,
          DataLogReaderHelper helper)
      {
         _timeStamp = new DataLog_TimeStamp();
         if (!_timeStamp.PopulateFromCompressedStream(reader, helper))
         {
            return ( false );
         }

         UInt16 val = 0;
         if (!ReadAndDecompress(reader, ref val))
         {
            return ( false );
         }
         _levelID = val;

         UInt32 nVal = 0;
         if (!ReadAndDecompress(reader, ref nVal))
         {
            return ( false );
         }
         _taskID = nVal;

         if (!ReadAndDecompress(reader, ref nVal))
         {
            return ( false );
         }
         _nodeID = nVal;

         if (!ReadAndDecompress(reader, ref val))
         {
            return ( false );
         }
         _formatLen = val;

         if (!ReadAndDecompress(reader, ref val))
         {
            return ( false );
         }
         _fileNameLen = val;

         if (!ReadAndDecompress(reader, ref val))
         {
            return ( false );
         }
         _lineNum = val;

         String str = null;
         if (!ReadAndDecompress(reader, ref str, _formatLen))
         {
            return ( false );
         }
         _format = str;

         str = null;
         if (!ReadAndDecompress(reader, ref str, _fileNameLen))
         {
            return ( false );
         }
         _fileName = str;

         str = String.Empty;
         if (!ReadAndDecompress(reader, _format, ref str))
         {
            return ( false );
         }
         _message = str;

         return ( true );
      }
      #endregion

   }
}
