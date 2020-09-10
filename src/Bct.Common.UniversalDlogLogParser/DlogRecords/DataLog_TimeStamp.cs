using Bct.Common.LogParsing.Reader;
using System;
using System.Collections.Generic;
using System.IO.Compression;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Bct.Common.LogParsing.Records
{
   /// <summary>
   /// 
   /// </summary>
   public class DataLog_TimeStamp : DataLog_BaseRecord
   {
      /// <summary>
      /// 
      /// </summary>
      public UInt32 _seconds { get; set; }

      /// <summary>
      /// 
      /// </summary>
      public UInt32 _nanoseconds { get; set; }

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
         Byte[] buffer = new Byte[4];

         UInt32 val = 0;
         if (!ReadAndDecompress(reader, ref val))
         {
            return ( false );
         }
         _seconds = val;

         if (!ReadAndDecompress(reader, ref val))
         {
            return ( false );
         }
         _nanoseconds = val;

         return ( true );
      }
   }
}
