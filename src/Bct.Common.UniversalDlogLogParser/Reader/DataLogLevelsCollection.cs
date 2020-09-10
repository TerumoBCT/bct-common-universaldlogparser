using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Bct.Common.LogParsing.Reader
{
   /// <summary>
   /// 
   /// </summary>
   public class DataLogLevelsCollection : Dictionary<UInt32, Dictionary<UInt16, String>>
   {
      /// <summary>
      /// 
      /// </summary>
      /// <param name="nodeID"></param>
      /// <param name="levelID"></param>
      /// <returns></returns>
      public String GetLevelString(
          UInt32 nodeID,
          UInt16 levelID)
      {
         if (false == this.Keys.Contains(nodeID))
         {
            return ( null );
         }

         if (false == this[nodeID].Keys.Contains(levelID))
         {
            return ( null );
         }

         return this[nodeID][levelID];
      }

      /// <summary>
      /// 
      /// </summary>
      /// <param name="nodeID"></param>
      /// <param name="levelID"></param>
      /// <param name="levelName"></param>
      public void AddLogLevel(
          UInt32 nodeID,
          UInt16 levelID,
          String levelName)
      {
         if (false == this.Keys.Contains(nodeID))
         {
            this.Add(nodeID, new Dictionary<UInt16, String>());
         }

         // We do this because we have found a 1W log with duplicated data on it. should never happen but we prevent
         // an exception by checking
         if (false == this[nodeID].Keys.Contains(levelID))
         {
            this[nodeID].Add(levelID, levelName);
         }
      }
   }
}
