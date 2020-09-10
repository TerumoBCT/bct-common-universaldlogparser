using System;
using System.Collections.Generic;
using System.IO.Compression;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MongoDB.Bson;
using Bct.Common.LogParsing.Reader;
using System.Globalization;
using BCT.Common.Logging.Extensions;

namespace Bct.Common.LogParsing.Records
{
   /// <summary>
   /// 
   /// </summary>
   public class DataLog_PeriodicItemRecord : DataLog_BaseRecord
   {
      /// <summary>
      /// 
      /// </summary>
      public DataLog_PeriodicItemRecord() :
          base()
      {
         _recordType = Enums.RecordId.PeriodicItemRecordId;
      }

      /// <summary>
      /// 
      /// </summary>
      public DataLog_TimeStamp _timeStamp { get; set; }

      /// <summary>
      /// 
      /// </summary>
      public UInt16 _keyCode { get; set; }

      /// <summary>
      /// 
      /// </summary>
      public UInt32 _nodeID { get; set; }

      /// <summary>
      /// 
      /// </summary>
      public UInt16 _keyLen { get; set; }

      /// <summary>
      /// 
      /// </summary>
      public UInt16 _descLen { get; set; }

      /// <summary>
      /// 
      /// </summary>
      public UInt16 _formatLen { get; set; }

      /// <summary>
      /// 
      /// </summary>
      public String _key { get; set; }

      /// <summary>
      /// 
      /// </summary>
      public String _description { get; set; }

      /// <summary>
      /// 
      /// </summary>
      public String _format { get; set; }

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
         return ( null );
         // TODO_CLG: For now we do not store this record until we decide how the data should be stored (if at all)
         //BsonDocument properties = AddCommonProperties(null, logHelper.DeviceSerialNumber, logDbId, logHelper.HeaderRecord._timeStampStart, this._timeStamp, this._recordType, null, null, null, this._nodeID);
         //properties.Add(Constants.DB_Message,
         //               String.Format(CultureInfo.InvariantCulture, "Periodic Item Key: '{0}' [{1}] (Code: {2})",
         //                             SanitizeFieldName(this._key), 
         //                             this._description,
         //                             this._keyCode));
         //return (properties);
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
            return (false);
         }

         UInt16 nVal16 = 0;
         if (!ReadAndDecompress(reader, ref nVal16))
         {
            return (false);
         }
         _keyCode = nVal16;

         UInt32 nVal32 = 0;
         if (!ReadAndDecompress(reader, ref nVal32))
         {
            return (false);
         }
         _nodeID = nVal32;

         if (!ReadAndDecompress(reader, ref nVal16))
         {
            return (false);
         }
         _keyLen = nVal16;

         if (!ReadAndDecompress(reader, ref nVal16))
         {
            return (false);
         }
         _descLen = nVal16;

         if (!ReadAndDecompress(reader, ref nVal16))
         {
            return (false);
         }
         _formatLen = nVal16;

         String str = null;
         if (!ReadAndDecompress(reader, ref str, _keyLen))
         {
            return (false);
         }
         _key = str;

         if (_descLen != 0)
         {
            if (!ReadAndDecompress(reader, ref str, _descLen))

            {
               return (false);
            }
         }
         else
         {
            BCTLoggerService.GetLogger<DataLog_PeriodicItemRecord>()
                  .WithError(String.Format(CultureInfo.InvariantCulture, "Error at timestamp '{4}.{5}' on record. Description length of 0. keyCode: {0}, keyLen: {1}, formatLen: {2}, key:{3}", _keyCode, _keyLen, _formatLen, _key, _timeStamp._seconds, _timeStamp._nanoseconds))
                  .Log();

            str = String.Empty;
         }

         _description = str;

         if (!ReadAndDecompress(reader, ref str, _formatLen))
         {
            return ( false );
         }
         _format = str;

         return ( true );
      }
   }
}
