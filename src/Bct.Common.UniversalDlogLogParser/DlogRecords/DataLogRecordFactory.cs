using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Bct.Common.LogParsing.Records
{
   /// <summary>
   /// 
   /// </summary>
   public sealed class DataLogRecordFactory
   {
      /// <summary>
      /// 
      /// </summary>
      /// <param name="recordType"></param>
      /// <returns></returns>
      public static DataLog_BaseRecord Create(
          Enums.RecordId recordType)
      {
         DataLog_BaseRecord currentRecord = null;

         switch (recordType)
         {
            case Enums.RecordId.HeaderRecordId: // = 0x5500,
               currentRecord = (DataLog_BaseRecord)new DataLog_HeaderRecord();
               break;
            case Enums.RecordId.LogLevelRecordId: //  = 0x5501,
               currentRecord = (DataLog_BaseRecord)new DataLog_LogLevelRecord();
               break;
            case Enums.RecordId.PrintOutputRecordId: // = 0x5502,
               currentRecord = (DataLog_BaseRecord)new DataLog_PrintOutputRecord();
               break;
            case Enums.RecordId.StreamOutputRecordId:// = 0x5503,
               currentRecord = (DataLog_BaseRecord)new DataLog_StreamOutputRecord();
               break;
            case Enums.RecordId.PeriodicOutputRecordId: // = 0x5504,
               currentRecord = (DataLog_BaseRecord)new DataLog_PeriodicOutputRecord();
               break;
            case Enums.RecordId.PeriodicSetRecordId: // = 0x5505,
               currentRecord = (DataLog_BaseRecord)new DataLog_PeriodicSetRecord();
               break;
            case Enums.RecordId.PeriodicItemRecordId: // = 0x5506,
               currentRecord = (DataLog_BaseRecord)new DataLog_PeriodicItemRecord();
               break;
            case Enums.RecordId.TaskCreateRecordId: // = 0x5507,
               currentRecord = (DataLog_BaseRecord)new DataLog_TaskCreateRecord();
               break;
            case Enums.RecordId.TaskDeleteRecordId: // = 0x5508,
               currentRecord = (DataLog_BaseRecord)new DataLog_TaskDeleteRecord();
               break;
            case Enums.RecordId.NetworkHeaderRecordId: // = 0x5509,
               currentRecord = (DataLog_BaseRecord)new DataLog_NetworkHeaderRecord();
               break;
            case Enums.RecordId.BinaryRecordId: // = 0x55f0,
               currentRecord = (DataLog_BaseRecord)new DataLog_BinaryRecord();
               break;
            // TODO_CLG: 
            //case Enums.RecordId.EndOfNetworkOutputRecordID: // = 0x55fc,
            //    currentRecord = (DataLog_BaseRecord)new DataLog_EndOfNetworkOutputRecord();
            //    break;
            case Enums.RecordId.FileCloseRecordId: // = 0x55fd,
               currentRecord = (DataLog_BaseRecord)new DataLog_FileCloseRecord();
               break;
            case Enums.RecordId.WriteTimeRecordId: // = 0x55fe,
               currentRecord = (DataLog_BaseRecord)new DataLog_WriteTimeRecord();
               break;
            case Enums.RecordId.MissedDataRecordId: // = 0x55ff
               currentRecord = (DataLog_BaseRecord)new DataLog_MissedDataRecord();
               break;
            default:
               // TODO_CLG: unhandled record type
               //throw new NotImplementedException();
               return null;
         }

         return ( currentRecord );
      }
   }
}
