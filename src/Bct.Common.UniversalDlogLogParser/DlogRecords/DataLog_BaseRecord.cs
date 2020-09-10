using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Bct.Common.LogParsing.Reader;
using Bct.Common.LogParsing.Utils;
using BCT.Common.Logging.Extensions;
using Bct.Common.LogParsing.Sinks;

namespace Bct.Common.LogParsing.Records
{
   /// <summary>
   /// 
   /// </summary>
   [BsonIgnoreExtraElements]
   public abstract class DataLog_BaseRecord
   {
      /// <summary>
      /// 
      /// </summary>
      [BsonIgnore]
      public Nullable<Enums.RecordId> _recordType { get; set; }

      /// <summary>
      /// 
      /// </summary>
      /// <param name="logDbId"></param>
      /// <param name="logHelper"></param>
      /// <returns></returns>
      public virtual BsonDocument GetPropertiesForStorage(
          String logDbId,
          DataLogReaderHelper logHelper)
      {
         throw new NotImplementedException();
      }

      /// <summary>
      /// 
      /// </summary>
      /// <param name="reader"></param>
      /// <param name="helper"></param>
      /// <returns></returns>
      public virtual Boolean PopulateFromCompressedStream(
          GZipStream reader,
          DataLogReaderHelper helper )
      {
         throw new NotImplementedException();
      }

      /// <summary>
      /// 
      /// </summary>
      private static DataLog_TimeStamp ms_NoDelta = new DataLog_TimeStamp();

      /// <summary>
      /// 
      /// </summary>
      /// <param name="inputProperties"></param>
      /// <param name="serialNumber"></param>
      /// <param name="logDbId"></param>
      /// <param name="logStartTime"></param>
      /// <param name="delta"></param>
      /// <param name="recordTypeId"></param>
      /// <param name="logLevelId"></param>
      /// <param name="logLevel"></param>
      /// <param name="taskId"></param>
      /// <param name="taskName"></param>
      /// <param name="nodeId"></param>
      /// <param name="nodeName"></param>
      /// <returns></returns>
      protected BsonDocument AddCommonProperties(
          BsonDocument inputProperties,
          String serialNumber,
          String logDbId,
          DateTime logStartTime,
          DataLog_TimeStamp delta,
          Nullable<Enums.RecordId> recordTypeId,
          Nullable<UInt16> logLevelId,
          String logLevel,
          Nullable<UInt32> taskId,
          String taskName,
          Nullable<UInt32> nodeId,
          String nodeName)
      {
         if (null == inputProperties)
         {
            inputProperties = new BsonDocument();
         }

         inputProperties.Add(Constants.DB_SerialNumber, serialNumber);
         inputProperties.Add(Constants.DB_LogName, logDbId);
         if (delta != null)
         {
            inputProperties.Add(Constants.DB_LocalTimeStamp, CalculateTimeStampAsDate(logStartTime, delta));
            inputProperties.Add(Constants.DB_TimeDelta, String.Format("{0}.{1}", delta._seconds, delta._nanoseconds));
         }
         else
         {
            inputProperties.Add(Constants.DB_LocalTimeStamp, CalculateTimeStampAsDate(logStartTime, ms_NoDelta));
         }

         if (recordTypeId != null)
         {
            inputProperties.Add(Constants.DB_RecordType, Enums.RecordIdStrings[recordTypeId.Value]);
            inputProperties.Add(Constants.DB_RecordTypeId, recordTypeId);
         }

         // TODO_CLG: Do we need  to add the LogLevel strings for each record, we do it at the end as a new entry with the list 
         if (null != logLevel)
         {
            inputProperties.Add(Constants.DB_LogLevel, logLevel);
         }

         if (null != logLevelId)
         {
            inputProperties.Add(Constants.DB_LogLevelId, logLevelId);
         }

         if (null != taskId)
         {
            inputProperties.Add(Constants.DB_TaskId, taskId);
         }

         if (false == String.IsNullOrWhiteSpace(taskName))
         {
            inputProperties.Add(Constants.DB_TaskName, taskName);
         }

         // TODO_CLG: Do we need the setId?
         if (null != nodeId)
         {
            inputProperties.Add(Constants.DB_NodeId, nodeId);
         }

         if (false == String.IsNullOrWhiteSpace(nodeName))
         {
            inputProperties.Add(Constants.DB_NodeName, nodeName);
         }
         
         return (inputProperties);
      }

      /// <summary>
      /// 
      /// </summary>
      /// <param name="start"></param>
      /// <param name="delta"></param>
      /// <returns></returns>
      public static String CalculateTimeStamp(
          DataLog_TimeStampStart start,
          DataLog_TimeStamp delta)
      {
         DateTime dtTime = CalculateTimeStampAsDate(start, delta);
         return (DateTimeToDbString(dtTime));
      }

      /// <summary>
      /// 
      /// </summary>
      /// <param name="start"></param>
      /// <param name="delta"></param>
      /// <returns></returns>
      public static String CalculateTimeStamp(
          DateTime start,
          DataLog_TimeStamp delta)
      {
         DateTime dtTime = start.AddMilliseconds(delta._nanoseconds / 1000000);
         dtTime = dtTime.AddSeconds(delta._seconds);
         return (DateTimeToDbString(dtTime));
      }

      /// <summary>
      /// 
      /// </summary>
      /// <param name="start"></param>
      /// <param name="delta"></param>
      /// <returns></returns>
      public static DateTime CalculateTimeStampAsDate(
          DateTime start,
          DataLog_TimeStamp delta)
      {
         DateTime dtTime = start.AddMilliseconds(delta._nanoseconds / 1000000);
         dtTime = dtTime.AddSeconds(delta._seconds);
         return (dtTime);
      }

      /// <summary>
      /// 
      /// </summary>
      /// <param name="start"></param>
      /// <param name="delta"></param>
      /// <returns></returns>
      public static DateTime CalculateTimeStampAsDate(
          DataLog_TimeStampStart start,
          DataLog_TimeStamp delta)
      {
         DateTime dtTime = new DateTime(start._year, start._month, start._day, start._hour, start._minute, start._second);
         if (delta != null)
         {
            dtTime = dtTime.AddMilliseconds(delta._nanoseconds / 1000000);
            dtTime = dtTime.AddSeconds(delta._seconds);
         }

         return (dtTime);
      }

      /// <summary>
      /// 
      /// </summary>
      /// <param name="value"></param>
      /// <returns></returns>
      public static DateTime ConvertTimestamp(
          DataLog_TimeStampStart value)
      {
         return new DateTime(value._year, value._month, value._day, value._hour, value._minute, value._second);
      }



      /// <summary>
      /// 
      /// </summary>
      /// <param name="input"></param>
      /// <returns></returns>
      public static String DateTimeToDbString(
          DateTime input)
      {
         String val = String.Format(CultureInfo.InvariantCulture, "{0:D4}-{1:D2}-{2:D2}T{3:D2}:{4:D2}:{5:D2}.{6:D3}",
                                    input.Year,
                                    input.Month,
                                    input.Day,
                                    input.Hour,
                                    input.Minute,
                                    input.Second,
                                    input.Millisecond);
         return (val);
      }

      private const Int32 mc_UInt16SizeToRead = sizeof(UInt16);

      /// <summary>
      /// 
      /// </summary>
      /// <param name="reader"></param>
      /// <param name="val"></param>
      /// <returns></returns>
      public static Boolean ReadAndDecompress(
          GZipStream reader,
          ref UInt16 val)
      {

         Byte[] buffer = new Byte[mc_UInt16SizeToRead];
         Int32 nRead = reader.Read(buffer, 0, mc_UInt16SizeToRead);
         if (nRead == 0)
         {
            return (false);
         }

         buffer = DataLogEncryption.DefaultDecryptFunc(buffer, nRead);
         val = BitConverter.ToUInt16(buffer, 0);

         return (true);
      }

      /// <summary>
      /// 
      /// </summary>
      /// <param name="reader"></param>
      /// <param name="val"></param>
      /// <returns></returns>
      public static Boolean ReadAndDecompress(
          GZipStream reader,
          ref Single val)
      {
         Byte[] buffer = new Byte[sizeof(UInt32)];
         Int32 nRead = reader.Read(buffer, 0, buffer.Length);
         if (nRead == 0)
         {
            return (false);
         }

         buffer = DataLogEncryption.DefaultDecryptFunc(buffer, nRead);
         val = BitConverter.ToSingle(buffer, 0);

         return (true);
      }

      /// <summary>
      /// 
      /// </summary>
      /// <param name="reader"></param>
      /// <param name="val"></param>
      /// <returns></returns>
      public static Boolean ReadAndDecompress(
          GZipStream reader,
          ref Double val)
      {
         Byte[] buffer = new Byte[sizeof(Double)];
         Int32 nRead = reader.Read(buffer, 0, buffer.Length);
         if (nRead == 0)
         {
            return (false);
         }

         buffer = DataLogEncryption.DefaultDecryptFunc(buffer, nRead);
         val = BitConverter.ToDouble(buffer, 0);

         return (true);
      }

      /// <summary>
      /// 
      /// </summary>
      /// <param name="reader"></param>
      /// <param name="val"></param>
      /// <returns></returns>
      public static Boolean ReadAndDecompress(
          GZipStream reader,
          ref UInt32 val)
      {
         Byte[] buffer = new Byte[sizeof(UInt32)];
         Int32 nRead = reader.Read(buffer, 0, buffer.Length);
         if (nRead == 0)
         {
            return (false);
         }

         buffer = DataLogEncryption.DefaultDecryptFunc(buffer, nRead);
         val = BitConverter.ToUInt32(buffer, 0);

         return (true);
      }

      /// <summary>
      /// 
      /// </summary>
      /// <param name="reader"></param>
      /// <param name="val"></param>
      /// <returns></returns>
      public static Boolean ReadAndDecompress(
          GZipStream reader,
          ref Int32 val)
      {
         Byte[] buffer = new Byte[sizeof(UInt32)];
         Int32 nRead = reader.Read(buffer, 0, buffer.Length);
         if (nRead == 0)
         {
            return (false);
         }

         buffer = DataLogEncryption.DefaultDecryptFunc(buffer, nRead);
         val = BitConverter.ToInt32(buffer, 0);

         return (true);
      }

      /// <summary>
      /// 
      /// </summary>
      /// <param name="reader"></param>
      /// <param name="val"></param>
      /// <returns></returns>
      public static Boolean ReadAndDecompress(
          GZipStream reader,
          ref Byte val)
      {
         Byte[] buffer = new Byte[1];
         Int32 nRead = reader.Read(buffer, 0, buffer.Length);
         if (nRead == 0)
         {
            return (false);
         }

         buffer = DataLogEncryption.DefaultDecryptFunc(buffer, nRead);
         val = buffer[0];
         return (true);
      }

      /// <summary>
      /// 
      /// </summary>
      /// <param name="reader"></param>
      /// <param name="val"></param>
      /// <returns></returns>
      public static Boolean ReadAndDecompress(
          GZipStream reader,
          ref Byte[] val)
      {
         try
         {
            Int32 nRead = reader.Read(val, 0, val.Length);
            if (nRead != 0)
            {
               val = DataLogEncryption.DefaultDecryptFunc(val, nRead);
               return (true);
            }
         }
         catch (Exception ex)
         {
            BCTLoggerService.GetLogger<DlogInputReaderSink>()
                  .WithError(String.Format(CultureInfo.InvariantCulture, "Exception '{0}' parsingreading from compressed stream",
                                           ex.Message))
                  .Log();
         }

         return (false);
      }


      /// <summary>
      /// 
      /// </summary>
      /// <param name="reader"></param>
      /// <param name="str"></param>
      /// <param name="strLen"></param>
      /// <returns></returns>
      public static Boolean ReadAndDecompress(
          GZipStream reader,
          ref String str,
          UInt16 strLen)
      {
         Byte[] buffer = new Byte[strLen];
         Int32 nRead = reader.Read(buffer, 0, buffer.Length);
         if (nRead == 0)
         {
            return (false);
         }

         buffer = DataLogEncryption.DefaultDecryptFunc(buffer, nRead);
         str = Encoding.ASCII.GetString(buffer);
         return (true);
      }

      /// <summary>
      /// 
      /// </summary>
      /// <param name="reader"></param>
      /// <param name="format"></param>
      /// <param name="str"></param>
      /// <returns></returns>
      public static Boolean ReadAndDecompress(
          GZipStream reader,
          String format,
          ref String str)
      {
         str = String.Empty;
         if (!ReadFormattedString(reader, format, out str))
         {
            return (false);
         }

         return (true);
      }

      /// <summary>
      /// TODO_CLG: Change this for "sprintf"
      /// </summary>
      /// <param name="reader"></param>
      /// <param name="format"></param>
      /// <param name="data"></param>
      /// <returns></returns>
      private static Boolean ReadFormattedString(
          GZipStream reader,
          String format,
          out String data)
      {
         data = String.Empty;
         String originalText = format;
         Enums.PrintArgType argType = Enums.PrintArgType.Invalid;
         Int32 nStartTokenPosition = -1;
         Int32 nStartIndex = 0;

         do
         {
            nStartTokenPosition = format.IndexOf('%', nStartIndex);
            if (nStartTokenPosition >= 0)
            {
               Char currentChar = format[nStartTokenPosition];
               Int32 nEndTokenPosition = nStartTokenPosition;
               Boolean bHasToken = false;
               Boolean bShouldFormat = true;
               Boolean longModifierPresent = false;
               do
               {
                  Char nextChar = format[++nEndTokenPosition];
                  switch (nextChar)
                  {
                     case '%':
                        if (currentChar == '%')
                        {
                           bHasToken = true;
                           bShouldFormat = false;
                           nStartIndex = nEndTokenPosition + 1; // Combination of %% corresponds to single % output. Skip it
                        }
                        else
                        {
                           // The format string contains an error.
                           argType = Enums.PrintArgType.Error;
                           bShouldFormat = false;
                           bHasToken = true;
                           break;
                        }
                        break;

                     case '*':
                        break;

                     case 'l':
                        longModifierPresent = true;
                        break;

                     case 'c':
                        argType = Enums.PrintArgType.Char;
                        bHasToken = true;
                        break;

                     case 'd':
                     case 'u':
                     case 'x':
                     case 'X':
                        argType = (longModifierPresent) ? Enums.PrintArgType.Long : Enums.PrintArgType.Int;
                        bHasToken = true;
                        break;

                     case 'f':
                     case 'g':
                        argType = (longModifierPresent) ? Enums.PrintArgType.Double : Enums.PrintArgType.Float;
                        bHasToken = true;
                        break;

                     case 's':
                        argType = Enums.PrintArgType.String;
                        bHasToken = true;
                        break;
                  }
               } while (bHasToken == false);

               if (bShouldFormat)
               {
                  ++nEndTokenPosition;
                  data += format.Substring(nStartIndex, nStartTokenPosition);
                  String token = format.Substring(nStartTokenPosition, nEndTokenPosition - nStartTokenPosition);
                  format = format.Substring(nEndTokenPosition);

                  // here we fetch the data from the stream and "format it", append it to data
                  String formattedToken = String.Empty;
                  String formatString = GetFormatStringFromToken(token);
                  Byte tempByte = 0;
                  Int32 tempInt32 = 0;
                  Single tempSingle = 0;
                  Double tempDouble = 0;
                  String tempString = String.Empty;
                  UInt16 tempUInt16 = 0;
                  switch (argType)
                  {
                     case Enums.PrintArgType.Char:
                        if (!ReadAndDecompress(reader, ref tempByte))
                        {
                           return (false);
                        }
                        formattedToken = String.Format(CultureInfo.InvariantCulture, formatString, tempByte);
                        break;

                     case Enums.PrintArgType.Int:
                     case Enums.PrintArgType.Long:
                        if (!ReadAndDecompress(reader, ref tempInt32))
                        {
                           return (false);
                        }
                        formattedToken = String.Format(CultureInfo.InvariantCulture, formatString, tempInt32);
                        break;

                     case Enums.PrintArgType.Float:
                        if (!ReadAndDecompress(reader, ref tempSingle))
                        {
                           return (false);
                        }
                        formattedToken = String.Format(CultureInfo.InvariantCulture, formatString, tempSingle);
                        break;

                     case Enums.PrintArgType.Double:
                        if (!ReadAndDecompress(reader, ref tempDouble))
                        {
                           return (false);
                        }
                        formattedToken = String.Format(CultureInfo.InvariantCulture, formatString, tempDouble);
                        break;

                     case Enums.PrintArgType.String:
                        if (!ReadAndDecompress(reader, ref tempUInt16))
                        {
                           return (false);
                        }
                        if (!ReadAndDecompress(reader, ref tempString, tempUInt16))
                        {
                           return (false);
                        }

                        formattedToken = String.Format(CultureInfo.InvariantCulture, formatString, tempString);
                        break;
                     default:
                        // TODO_CLG:
                        throw new NotImplementedException();
                  }
                  data += formattedToken;
               }
            }
         } while (nStartTokenPosition != -1);

         return (true);
      }

      /// <summary>
      /// TODO_CLG this does not take into account the token , must do a full printf implementation in c#
      /// </summary>
      /// <param name="token"></param>
      /// <returns></returns>
      private static String GetFormatStringFromToken(
          String token)
      {
         return "{0}";
      }

      /// <summary>
      /// 
      /// </summary>
      /// <param name="fieldName"></param>
      /// <returns></returns>
      public static String SanitizeFieldName(
          String fieldName)
      {
         if (true == String.IsNullOrEmpty(fieldName))
         {
            return (fieldName);
         }

         return fieldName.Replace(".", ";");
      }

      /// <summary>
      /// 
      /// </summary>
      /// <param name="fileName"></param>
      /// <param name="lineNum"></param>
      /// <param name="message"></param>
      /// <returns></returns>
      public String FormatCodeMessage(
          String fileName,
          UInt16 lineNum,
          String message)
      {
         return String.Format(CultureInfo.InvariantCulture, "{0}:{1} [{2}]", fileName, lineNum, message);
      }
   }
}
