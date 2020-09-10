using System;
using System.Collections.Generic;
using System.IO.Compression;
using MongoDB.Bson;
using Bct.Common.LogParsing.Reader;

namespace Bct.Common.LogParsing.Records
{
   /// <summary>
   /// 
   /// </summary>
   public class DataLog_TaskCreateRecord : DataLog_BaseRecord
   {
      /// <summary>
      /// 
      /// </summary>
      public DataLog_TaskCreateRecord() :
          base()
      {
         _recordType = Enums.RecordId.TaskCreateRecordId;
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
      public UInt16 _nameLen { get; set; }

      /// <summary>
      /// 
      /// </summary>
      public String _name { get; set; }

      #region Base class overrides
      /// <summary>
      /// 
      /// </summary>
      /// <param name="reader"></param>
      /// <param name="helper"></param>
      /// <returns></returns>
      public override Boolean PopulateFromCompressedStream(
          GZipStream reader,
          DataLogReaderHelper helper )
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
         _nameLen = val;

         String str = null;
         if (!ReadAndDecompress(reader, ref str, _nameLen))
         {
            return ( false );
         }
         _name = str;

         if (false == helper.TaskNames.ContainsKey(_taskID))
         {
            helper.TaskNames.Add(_taskID, _name);
         }

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
         BsonDocument properties = AddCommonProperties(null, logHelper.DeviceSerialNumber, logDbId, logHelper.HeaderRecord._timeStampStart, this._timeStamp, this._recordType, this._levelID, logHelper.LogLevels.GetLevelString(this._nodeID, this._levelID), this._taskID, logHelper.TaskNames[this._taskID], this._nodeID, logHelper.NodeNames[this._nodeID]);
         return ( properties );
      }
      #endregion
   }
}
