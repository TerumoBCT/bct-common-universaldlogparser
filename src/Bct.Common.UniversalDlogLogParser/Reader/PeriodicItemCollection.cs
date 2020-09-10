using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Bct.Common.LogParsing.Utils;

namespace Bct.Common.LogParsing.Reader
{
   /// <summary>
   /// 
   /// </summary>
   public class PeriodicItemCollection : Dictionary<UInt16, PeriodicItem>
   {
      /// <summary>
      /// 
      /// </summary>
      /// <param name="keyCode"></param>
      /// <param name="key"></param>
      /// <param name="description"></param>
      /// <param name="format"></param>
      public void AddPeriodicItem(
          UInt16 keyCode,
          String key,
          String description,
          String format)
      {
         this.Add(keyCode, new PeriodicItem() { Key = key, Description = description, Format = format });
      }

      /// <summary>
      /// 
      /// </summary>
      /// <param name="keyCode"></param>
      /// <returns></returns>
      public PeriodicItem GetPeriodicItem(
          UInt16 keyCode)
      {
         if (!this.Keys.Contains(keyCode))
         {
            return ( null );
         }

         return ( this[keyCode] );
      }
   }

   /// <summary>
   /// 
   /// </summary> 
   public class PeriodicItem
   {
      /// <summary>
      /// 
      /// </summary>
      public String Key { get; set; }

      /// <summary>
      /// 
      /// </summary>
      public String Description { get; set; }

      /// <summary>
      /// 
      /// </summary>
      public String Format { get; set; }
   }
}