using System;
using System.Collections.Generic;
using System.Globalization;
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
   public class DataLog_StreamOutputRecord : DataLog_BaseRecord
   {
      /// <summary>
      /// 
      /// </summary>
      public DataLog_StreamOutputRecord() :
          base()
      {
         _recordType = Enums.RecordId.StreamOutputRecordId;
      }

      /// <summary>
      /// 
      /// </summary>
      public DataLog_TimeStamp _timeStamp { get; set; }

      /// <summary>
      /// 
      /// </summary>
      public UInt16 _levelId { get; set; }

      /// <summary>
      /// 
      /// </summary>
      public UInt32 _taskId { get; set; }

      /// <summary>
      /// 
      /// </summary>
      public UInt32 _nodeId { get; set; }

      /// <summary>
      /// 
      /// </summary>
      public UInt16 _fileNameLen { get; set; }

      /// <summary>
      /// 
      /// </summary>
      public UInt16 _lineNum { get; set; }

      /// <summary>
      /// 
      /// </summary>
      public String _fileName { get; set; }

      /// <summary>
      /// 
      /// </summary>
      public String _message { get; set; }

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
         if (true == logHelper.TaskNames.ContainsKey(this._taskId))
         {
            taskName = logHelper.TaskNames[this._taskId];
         }

         BsonDocument properties = AddCommonProperties(null, logHelper.DeviceSerialNumber, logDbId, logHelper.HeaderRecord._timeStampStart, this._timeStamp, this._recordType, this._levelId, logHelper.LogLevels.GetLevelString(this._nodeId, this._levelId), this._taskId, taskName, this._nodeId, logHelper.NodeNames[this._nodeId]);

         properties.Add(Constants.DB_Message, base.FormatCodeMessage(this._fileName, this._lineNum, this._message));
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

         UInt16 val = 0;
         if (!ReadAndDecompress(reader, ref val))
         {
            return ( false );
         }
         _levelId = val;

         UInt32 nVal32 = 0;
         if (!ReadAndDecompress(reader, ref nVal32))
         {
            return ( false );
         }
         _taskId = nVal32;


         if (!ReadAndDecompress(reader, ref nVal32))
         {
            return ( false );
         }
         _nodeId = nVal32;

         if (!ReadAndDecompress(reader, ref val))
         {
            return ( false );
         }
         _fileNameLen = val;

         if (!ReadAndDecompress(reader, ref val))
         {
            return ( false );
         }
         _lineNum = val;

         String str = null;
         if (!ReadAndDecompress(reader, ref str, this._fileNameLen))
         {
            return ( false );
         }
         _fileName = str;

         if (!GetMessage(reader, out string message))
         {
            return (false);
         }
         _message = message;

         return ( true );
      }

      /// <summary>
      /// 
      /// </summary>
      /// <param name="reader"></param>
      /// <param name="message"></param>
      /// <returns></returns>
      private Boolean GetMessage(
          GZipStream reader,
          out String message)
      {
         message = String.Empty;
         StringBuilder builder = new StringBuilder();


         UInt16 format = 0;
         if (!ReadAndDecompress(reader, ref format))
         {
            return ( false );
         }

         Byte precision = 0x00;
         if (!ReadAndDecompress(reader, ref precision))
         {
            return ( false );
         }

         UInt16 dataLen = 0;
         if (!ReadAndDecompress(reader, ref dataLen))
         {
            return ( false );
         }

         UInt16 readLen = 0;
         Byte type = 0;
         Byte tempByte = 0;
         Int32 tempInt = 0;
         UInt32 tempUInt = 0;
         UInt16 tempShort = 0;
         Single tempFloat = 0;
         Double tempDouble = 0;
         while (readLen < dataLen)
         {
            if (!ReadAndDecompress(reader, ref type))
            {
               return ( false );
            }
            ++readLen;

            switch ((Enums.StreamArgType)type)
            {
               case Enums.StreamArgType.SignedChar:
               case Enums.StreamArgType.UnsignedChar:
                  if (!ReadAndDecompress(reader, ref tempByte))
                  {
                     return ( false );
                  }
                  builder.Append(Encoding.ASCII.GetString(new Byte[1] { tempByte }));
                  ++readLen;
                  break;

               case Enums.StreamArgType.SignedInt:
               case Enums.StreamArgType.SignedLong:
                  if (!ReadAndDecompress(reader, ref tempInt))
                  {
                     return ( false );
                  }
                  builder.Append(tempInt.ToString());
                  readLen += 4;
                  break;

               case Enums.StreamArgType.UnsignedInt:
               case Enums.StreamArgType.UnsignedLong:
                  if (!ReadAndDecompress(reader, ref tempUInt))
                  {
                     return ( false );
                  }
                  builder.Append(tempUInt.ToString());
                  readLen += 4;
                  break;

               case Enums.StreamArgType.String:
                  if (!ReadAndDecompress(reader, ref tempShort))
                  {
                     return ( false );
                  }
                  for (Int32 i = 0; i < tempShort; ++i)
                  {
                     if (!ReadAndDecompress(reader, ref tempByte))
                     {
                        return ( false );
                     }
                     builder.Append(Encoding.ASCII.GetString(new Byte[1] { tempByte }));
                  }
                  readLen += (UInt16)( (UInt16)2 + tempShort );
                  break;

               case Enums.StreamArgType.Float:
                  if (!ReadAndDecompress(reader, ref tempFloat))
                  {
                     return ( false );
                  }
                  builder.Append(tempFloat.ToString());
                  readLen += 4;
                  break;

               case Enums.StreamArgType.Double:
                  if (!ReadAndDecompress(reader, ref tempDouble))
                  {
                     return ( false );
                  }
                  builder.Append(tempDouble.ToString());
                  readLen += 8;
                  break;

               case Enums.StreamArgType.Bool:
                  if (!ReadAndDecompress(reader, ref tempByte))
                  {
                     return ( false );
                  }
                  builder.Append(tempByte == 0 ? "false" : "true");
                  readLen += 1;
                  break;

               case Enums.StreamArgType.Flag:
                  if (!ReadAndDecompress(reader, ref format))
                  {
                     return ( false );
                  }
                  // stream.flags(format);
                  readLen += 2;
                  break;

               case Enums.StreamArgType.Precision:
                  if (!ReadAndDecompress(reader, ref precision))
                  {
                     return ( false );
                  }
                  //stream.precision(precision);
                  readLen += 1;
                  break;
            }
         }

         message = builder.ToString();

         return ( true );
      }
   }
}
