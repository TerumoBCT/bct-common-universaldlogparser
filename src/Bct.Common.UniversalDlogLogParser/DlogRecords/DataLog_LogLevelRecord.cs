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
   public class DataLog_LogLevelRecord : DataLog_BaseRecord
   {
      /// <summary>
      /// 
      /// </summary>
      public DataLog_LogLevelRecord() :
          base()
      {
         _recordType = Enums.RecordId.LogLevelRecordId;
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
      public UInt16 _nameLen { get; set; }

      /// <summary>
      /// 
      /// </summary>
      public String _name { get; set; }

      /// <summary>
      /// 
      /// </summary>
      public UInt32 _nodeId { get; set; }


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
         return ( null );
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
         // the record type is not read from compressed stream
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
         _nodeId = nVal;

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

         return ( true );
      }
      #endregion
   }
}
