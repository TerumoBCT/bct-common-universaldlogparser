using Bct.Common.LogParsing.Reader;
using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Bct.Common.LogParsing.Records
{
   /// <summary>
   /// 
   /// </summary>
   public class DataLog_TimeStampStart : DataLog_BaseRecord
   {
      /// <summary>
      /// 1-31
      /// </summary>
      public Byte _day { get; set; }

      /// <summary>
      /// 1-12
      /// </summary>
      public Byte _month { get; set; }

      /// <summary>
      /// 
      /// </summary>
      public UInt16 _year { get; set; }

      /// <summary>
      /// 
      /// </summary>
      public Byte _hour { get; set; }

      /// <summary>
      /// 
      /// </summary>
      public Byte _minute { get; set; }

      /// <summary>
      /// 
      /// </summary>
      public Byte _second { get; set; }

      /// <summary>
      /// 
      /// </summary>
      /// <param name="reader"></param>
      public void PopulateFromReader(
          BinaryReader reader)
      {
         _day = reader.ReadByte();
         _month = reader.ReadByte();
         _year = reader.ReadUInt16();
         _hour = reader.ReadByte();
         _minute = reader.ReadByte();
         _second = reader.ReadByte();
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
         Byte val = 0;
         if (!ReadAndDecompress(reader, ref val))
         {
            return ( false );
         }
         _day = val;

         if (!ReadAndDecompress(reader, ref val))
         {
            return ( false );
         }
         _month = val;

         UInt16 nVal = 0;
         if (!ReadAndDecompress(reader, ref nVal))
         {
            return ( false );
         }
         _year = nVal;

         if (!ReadAndDecompress(reader, ref val))
         {
            return ( false );
         }
         _hour = val;

         if (!ReadAndDecompress(reader, ref val))
         {
            return ( false );
         }
         _minute = val;

         if (!ReadAndDecompress(reader, ref val))
         {
            return ( false );
         }
         _second = val;

         return ( true );
      }
   }
}
