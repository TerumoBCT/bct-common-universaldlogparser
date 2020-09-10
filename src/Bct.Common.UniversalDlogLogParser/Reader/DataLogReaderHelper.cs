using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Bct.Common.LogParsing.Records;

namespace Bct.Common.LogParsing.Reader
{
   /// <summary>
   /// 
   /// </summary>
   public class DataLogReaderHelper
   {

      /// <summary>
      /// 
      /// </summary>
      public DataLogReaderHelper()
      {
         this.LogLevels = new DataLogLevelsCollection();
         this.PeriodicSets = new PeriodicSetCollection();
         this.PeriodicItems = new PeriodicItemCollection();
         this.TaskNames = new Dictionary<UInt32, String>();
         this.NodeNames = new Dictionary<UInt32, String>();
      }

      /// <summary>
      /// 
      /// </summary>
      public RunDataInfo Info { get; set; }

      /// <summary>
      /// 
      /// </summary>
      public DataLog_HeaderRecord HeaderRecord { get; set; }

      /// <summary>
      /// [NodeID], { [LevelID], [LEvelName] }
      /// </summary>
      public DataLogLevelsCollection LogLevels { get; set; }

      /// <summary>
      /// 
      /// </summary>
      public PeriodicSetCollection PeriodicSets { get; set; }

      /// <summary>
      /// 
      /// </summary>
      public PeriodicItemCollection PeriodicItems { get; set; }

      /// <summary>
      /// 
      /// </summary>
      public Dictionary<UInt32, String> TaskNames { get; set;  }

      /// <summary>
      /// 
      /// </summary>
      public Dictionary<UInt32, String> NodeNames { get; set;  }

      /// <summary>
      /// 
      /// </summary>
      public String DeviceSerialNumber { get; set; }

      /// <summary>
      /// 
      /// </summary>
      /// <param name="serialNumber"></param>
      /// <returns></returns>
      public static String GetCollectionTypeBySerialNumber(
          String serialNumber)
      {
         return serialNumber?.Substring(0, 2);
      }
   }
}
