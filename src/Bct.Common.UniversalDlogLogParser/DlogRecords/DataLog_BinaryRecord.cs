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
   public class DataLog_BinaryRecord : DataLog_BaseRecord
   {
      /// <summary>
      /// 
      /// </summary>
      public DataLog_BinaryRecord() :
          base()
      {
         _recordType = Enums.RecordId.BinaryRecordId;
      }

      /// <summary>
      /// 
      /// </summary>
      public UInt32 _size { get; set; }

      /// <summary>
      /// 
      /// </summary>
      public UInt16 _type { get; set; }

      /// <summary>
      /// 
      /// </summary>
      public UInt16 _subType { get; set; }

      /// <summary>
      /// 
      /// </summary>
      public Byte[] _data { get; set; }

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
         BsonDocument properties = AddCommonProperties(null, logHelper.DeviceSerialNumber, logDbId, logHelper.HeaderRecord._latestTimeStamp, null, this._recordType, null, null, null, null, null, null);

         // TODO_CLG: should we figure out the previous record and use that timestamp??
         // properties.Add(Constants.DB_TimeStamp, CalculateTimeStamp(startTime, this._timeStamp));
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
         UInt32 nVal32 = 0;

         if (!ReadAndDecompress(reader, ref nVal32))
         {
            return ( false );
         }
         _size = nVal32;

         UInt16 nVal16 = 0;
         if (!ReadAndDecompress(reader, ref nVal16))
         {
            return ( false );
         }
         _type = nVal16;

         if (!ReadAndDecompress(reader, ref nVal16))
         {
            return ( false );
         }
         _subType = nVal16;

         if (_size > 4)
         {
            Byte[] data = new Byte[_size - 4];
            if (!ReadAndDecompress(reader, ref data))
            {
               return ( false );
            }
            _data = data;
         }

         return ( true );
      }
      #endregion
   }
}
