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
   public class DataLog_FileCloseRecord : DataLog_BaseRecord
   {
      /// <summary>
      /// 
      /// </summary>
      public DataLog_FileCloseRecord()
          : base()
      {
         _recordType = Enums.RecordId.FileCloseRecordId;
      }

      /// <summary>
      /// 
      /// </summary>
      public DataLog_TimeStamp _timeStamp { get; set; }

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
         return AddCommonProperties(null, logHelper.DeviceSerialNumber, logDbId, logHelper.HeaderRecord._timeStampStart, this._timeStamp, this._recordType, null, null, null, null, null, null);
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
         return ( true );
      }
      #endregion
   }
}
