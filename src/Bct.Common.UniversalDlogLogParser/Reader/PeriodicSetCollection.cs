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
   public class PeriodicSetCollection : Dictionary<UInt16, PeriodicSet>
   {
      /// <summary>
      /// 
      /// </summary>
      /// <param name="setID"></param>
      /// <param name="setName"></param>
      public void AddPeriodicSet(
          UInt16 setID,
          String setName)
      {
         this.Add(setID, new PeriodicSet() 
            { 
               Name = setName 
            });
      }
   }

   /// <summary>
   /// 
   /// </summary> 
   public class PeriodicSet
   {
      /// <summary>
      /// 
      /// </summary>
      public String Name { get; set; }
   }
}
