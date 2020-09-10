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
   public class DataLog_TaskDeleteRecord : DataLog_BaseRecord
   {
      /// <summary>
      /// 
      /// </summary>
      public DataLog_TaskDeleteRecord() :
          base()
      {
         _recordType = Enums.RecordId.TaskDeleteRecordId;
      }

      /// <summary>
      /// 
      /// </summary>
      public DataLog_TimeStamp _timeStamp { get; set; }

      /// <summary>
      /// 
      /// </summary>
      public UInt16 _levelId { get; set; }

      /// <summary>
      /// 
      /// </summary>
      public UInt32 _taskId { get; set; }

      /// <summary>
      /// 
      /// </summary>
      public UInt32 _nodeId { get; set; }

      #region Base class overrides
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
         _levelId = val;

         UInt32 nVal = 0;
         if (!ReadAndDecompress(reader, ref nVal))
         {
            return ( false );
         }
         _taskId = nVal;

         if (!ReadAndDecompress(reader, ref nVal))
         {
            return ( false );
         }
         _nodeId = nVal;

         return ( true );
      }

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
         String taskName = String.Empty;
         if (logHelper.TaskNames.ContainsKey(this._taskId))
         {
            taskName = logHelper.TaskNames[this._taskId];
         }

         return AddCommonProperties(null, logHelper.DeviceSerialNumber, logDbId, logHelper.HeaderRecord._timeStampStart, this._timeStamp, this._recordType, this._levelId, logHelper.LogLevels.GetLevelString(this._nodeId, this._levelId), this._taskId, taskName, this._nodeId, logHelper.NodeNames[this._nodeId]);
      }
      #endregion
   }
}
