using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MongoDB.Bson;
using Bct.Common.LogParsing.Reader;
using System.IO.Compression;

namespace Bct.Common.LogParsing.Records
{
   /// <summary>
   /// 
   /// </summary>
   public class DataLog_HeaderRecord : DataLog_BaseRecord
   {
      /// <summary>
      /// 
      /// </summary>
      public DataLog_HeaderRecord() :
          base()
      {
         _recordType = Enums.RecordId.HeaderRecordId;
      }

      /// <summary>
      /// 
      /// </summary>
      public UInt16 _charSize { get; set; }

      /// <summary>
      /// 
      /// </summary>
      public UInt16 _intSize { get; set; }

      /// <summary>
      /// 
      /// </summary>
      public UInt16 _longSize { get; set; }

      /// <summary>
      /// 
      /// </summary>
      public UInt16 _floatSize { get; set; }

      /// <summary>
      /// 
      /// </summary>
      public UInt16 _doubleSize { get; set; }

      /// <summary>
      /// 
      /// </summary>
      public UInt16 _taskIDSize { get; set; }

      /// <summary>
      /// 
      /// </summary>
      public UInt32 _currentTaskID { get; set; }

      /// <summary>
      /// 
      /// </summary>
      public UInt16 _nodeIDSize { get; set; }

      /// <summary>
      /// 
      /// </summary>
      public UInt16 _version { get; set; }

      /// <summary>
      /// 
      /// </summary>
      public UInt16 _platformNameLength { get; set; }

      /// <summary>
      /// 
      /// </summary>
      public String _platformName { get; set; }

      /// <summary>
      /// 
      /// </summary>
      public UInt32 _logNodeId { get; set; }

      /// <summary>
      /// 
      /// </summary>
      public DateTime _latestTimeStamp { get; set; }

      /// <summary>
      /// 
      /// </summary>
      public DateTime _timeStampStart { get; set; }

      /// <summary>
      /// 
      /// </summary>
      public UInt16 _nodeNameLength { get; set; }

      /// <summary>
      /// 
      /// </summary>
      public String _nodeName { get; set; }


      #region Base class overrides
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
         if(logHelper.TaskNames.ContainsKey(this._currentTaskID))
         {
            taskName = logHelper.TaskNames[this._currentTaskID];
         }

         BsonDocument properties = AddCommonProperties(null, logHelper.DeviceSerialNumber, logDbId, logHelper.HeaderRecord._latestTimeStamp, null, Enums.RecordId.HeaderRecordId, null, null, this._currentTaskID, taskName, this._logNodeId, this._nodeName);

         properties.Add(Constants.DB_Version, this._version);
         properties.Add(Constants.DB_PlatformName, this._platformName);

         if (false == logHelper.NodeNames.ContainsKey(_logNodeId))
         {
            logHelper.NodeNames.Add(_logNodeId, _nodeName);
         }

         return (properties);
      }

      /// <summary>
      /// 
      /// </summary>
      /// <param name="reader"></param>
      /// <returns></returns>
      public void PopulateFromReader(
          BinaryReader reader)
      {
         _recordType = (Enums.RecordId)reader.ReadUInt16();
         _charSize = reader.ReadUInt16();
         _intSize = reader.ReadUInt16();
         _longSize = reader.ReadUInt16();
         _floatSize = reader.ReadUInt16();
         _doubleSize = reader.ReadUInt16();
         _taskIDSize = reader.ReadUInt16();
         _currentTaskID = reader.ReadUInt32();
         _nodeIDSize = reader.ReadUInt16();
         _version = reader.ReadUInt16();
         _platformNameLength = reader.ReadUInt16();
         Byte[] ab = reader.ReadBytes(_platformNameLength);
         _platformName = Encoding.ASCII.GetString(ab);
         _logNodeId = reader.ReadUInt32();
         DataLog_TimeStampStart start = new DataLog_TimeStampStart();
         start.PopulateFromReader(reader);
         _timeStampStart = new DateTime(start._year, start._month, start._day, start._hour, start._minute, start._second);
         _nodeNameLength = reader.ReadUInt16();
         ab = reader.ReadBytes(_nodeNameLength);
         _nodeName = Encoding.ASCII.GetString(ab);
      }

      // <summary>
      //     TODO_CLG   : Can this record be in the middle of the file?
      // </summary>
      // <param name="reader"></param>
      //public virtual Boolean PopulateFromCompressedStream(
      //   GZipStream reader)
      //{
      //
      //}
      #endregion
   }
}
