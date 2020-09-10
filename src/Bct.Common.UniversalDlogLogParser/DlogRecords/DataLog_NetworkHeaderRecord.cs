using System;
using System.Collections.Generic;
using System.IO;
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
   public class DataLog_NetworkHeaderRecord : DataLog_BaseRecord
   {
      /// <summary>
      /// 
      /// </summary>
      public DataLog_NetworkHeaderRecord() :
          base()
      {
         _recordType = Enums.RecordId.NetworkHeaderRecordId;
      }

      /// <summary>
      /// 
      /// </summary>
      public UInt32 _nodeID { get; set; }

      /// <summary>
      /// 
      /// </summary>
      public DataLog_TimeStampStart _start { get; set; }

      /// <summary>
      /// 
      /// </summary>
      public UInt16 _nodeNameLen { get; set; }

      /// <summary>
      /// 
      /// </summary>
      public String _nodeName { get; set; }

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
         UInt32 nVal = 0;
         if (!ReadAndDecompress(reader, ref nVal))
         {
            return ( false );
         }
         _nodeID = nVal;

         _start = new DataLog_TimeStampStart();
         if (!_start.PopulateFromCompressedStream(reader, helper))
         {
            return ( false );
         }

         UInt16 nVal16 = 0;
         if (!ReadAndDecompress(reader, ref nVal16))
         {
            return ( false );
         }
         _nodeNameLen = nVal16;

         String str = null;
         if (!ReadAndDecompress(reader, ref str, _nodeNameLen))
         {
            return ( false );
         }
         _nodeName = str;

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
         BsonDocument properties = AddCommonProperties(null, logHelper.DeviceSerialNumber, logDbId, ConvertTimestamp(this._start), null, this._recordType, null, null, null, null, this._nodeID, logHelper.NodeNames[this._nodeID]);
         return ( properties );
      }
      #endregion
   }
}
