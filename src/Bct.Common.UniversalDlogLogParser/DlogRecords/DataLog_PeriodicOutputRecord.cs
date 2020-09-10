using System;
using System.Collections.Generic;
using System.IO.Compression;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MongoDB.Bson;
using Bct.Common.LogParsing.Reader;
using Bct.Common.LogParsing.Utils;

namespace Bct.Common.LogParsing.Records
{
   /// <summary>
   /// 
   /// </summary>
   public class DataLog_PeriodicOutputRecord : DataLog_BaseRecord
   {
      /// <summary>
      /// 
      /// </summary>
      public DataLog_PeriodicOutputRecord() :
          base()
      {
         _recordType = Enums.RecordId.PeriodicOutputRecordId;
         _items = new List<PeriodicOutputRecordItem>();
      }

      /// <summary>
      /// 
      /// </summary>
      private List<PeriodicOutputRecordItem> _items;

      /// <summary>
      /// 
      /// </summary>
      public DataLog_TimeStamp _timeStamp { get; set; }

      /// <summary>
      /// 
      /// </summary>
      public UInt16 _setID { get; set; }

      /// <summary>
      /// 
      /// </summary>
      public UInt32 _nodeID { get; set; }

      /// <summary>
      /// 
      /// </summary>
      public UInt16 _itemCount { get; set; }

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
         BsonDocument properties = AddCommonProperties(null, logHelper.DeviceSerialNumber, logDbId, logHelper.HeaderRecord._timeStampStart, this._timeStamp, this._recordType, null, null, null, null, this._nodeID, logHelper.NodeNames[this._nodeID]);

         foreach (var item in _items)
         {
            PeriodicItem periodicItem = logHelper.PeriodicItems.GetPeriodicItem(item._keyCode);
            if (periodicItem != null)
            {
               if (false == properties.Contains(periodicItem.Key))
               {
                  if (item._data != null) // We write only data that contains values.
                  {
                     // TODO_CLG: In here instead of a string for value we would like to
                     // save the native object type: Int32, float, string
                     Object fieldValue = PrintfPort.Sprintf(periodicItem.Format, item._data);
                     if (null != fieldValue)
                     {
                        properties.Add(periodicItem.Key, BsonValue.Create(fieldValue));
                     }
                  }
               }
               else
               {
                  // TODO_LOGGING: Data was duplicated in the log (2 columns with the same name)
                  // Skipping it
               }
            }
            else
            {
               // TODO_CLG: this is an error in the DLOG????
               //throw new NotImplementedException(); 
            }
         }
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

         UInt16 nVal16 = 0;
         if (!ReadAndDecompress(reader, ref nVal16))
         {
            return ( false );
         }
         _setID = nVal16;

         UInt32 nVal32 = 0;
         if (!ReadAndDecompress(reader, ref nVal32))
         {
            return ( false );
         }
         _nodeID = nVal32;

         if (!ReadAndDecompress(reader, ref nVal16))
         {
            return ( false );
         }
         _itemCount = nVal16;

         Byte[] data = null;
         for (UInt16 nPos = 0; nPos < _itemCount; ++nPos)
         {
            PeriodicOutputRecordItem item = new PeriodicOutputRecordItem();
            if (!ReadAndDecompress(reader, ref nVal16))
            {
               return ( false );
            }
            item._size = nVal16;


            if (!ReadAndDecompress(reader, ref nVal16))
            {
               return ( false );
            }
            item._keyCode = nVal16;

            if (item._size > 0)
            {
               data = new Byte[item._size];
               if (!ReadAndDecompress(reader, ref data))
               {
                  return ( false );
               }
               item._data = data;
            }

            this._items.Add(item);
         }

         return ( true );
      }
   }

   /// <summary>
   /// 
   /// </summary>
   public class PeriodicOutputRecordItem
   {
      /// <summary>
      /// 
      /// </summary>
      public UInt16 _keyCode { get; set; }

      /// <summary>
      /// 
      /// </summary>
      public UInt16 _size { get; set; }

      /// <summary>
      /// 
      /// </summary>
      public Byte[] _data { get; set; }
   }
}
