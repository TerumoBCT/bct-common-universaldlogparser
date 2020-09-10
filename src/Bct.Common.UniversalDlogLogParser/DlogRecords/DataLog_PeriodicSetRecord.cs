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
   /// TODO_CLG: What is the point of this record       
   /// </summary>
   public class DataLog_PeriodicSetRecord : DataLog_BaseRecord
   {
      /// <summary>
      /// 
      /// </summary>
      public DataLog_PeriodicSetRecord() :
          base()
      {
         _recordType = Enums.RecordId.PeriodicSetRecordId;
      }

      /// <summary>
      /// 
      /// </summary>
      public DataLog_TimeStamp _timeStamp { get; set; }

      /// <summary>
      /// 
      /// </summary>
      public UInt16 _setID { get; set; }

      /// <summary>
      /// 
      /// </summary>
      public UInt32 _nodeID { get; set; }

      /// <summary>
      /// Number of Items included in the record
      /// </summary>
      public UInt16 _nameLen { get; set; }

      /// <summary>
      /// 
      /// </summary>
      public String _name { get; set; }

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
         BsonDocument properties = AddCommonProperties(null, logHelper.DeviceSerialNumber, logDbId, logHelper.HeaderRecord._timeStampStart, this._timeStamp, this._recordType, null, null, null, null, this._nodeID, logHelper.NodeNames[this._nodeID]);

         properties.Add(Constants.DB_SetID, this._setID);
         properties.Add(Constants.DB_Name, this._name);
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

         UInt16 nVal16 = 0;
         if (!ReadAndDecompress(reader, ref nVal16))
         {
            return ( false );
         }
         _setID = nVal16;

         UInt32 nVal32 = 0;
         if (!ReadAndDecompress(reader, ref nVal32))
         {
            return ( false );
         }
         _nodeID = nVal32;

         if (!ReadAndDecompress(reader, ref nVal16))
         {
            return ( false );
         }
         _nameLen = nVal16;

         String str = null;
         if (!ReadAndDecompress(reader, ref str, _nameLen))
         {
            return ( false );
         }
         _name = str;

         return ( true );
      }
   }
}
