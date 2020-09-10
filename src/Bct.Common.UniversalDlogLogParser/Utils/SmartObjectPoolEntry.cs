using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;

namespace Bct.Common.LogParsing.Utils
{
   /// <summary>
   /// 
   /// </summary>
   /// <typeparam name="K"></typeparam>
   public class SmartObjectPoolEntry<K>
   {
      /// <summary>
      /// 
      /// </summary>
      private Object m_Lock = new Object();

      /// <summary>
      /// 
      /// </summary>
      public SmartObjectPoolEntry()
      {
         this.InUse = false;
      }

      /// <summary>
      /// 
      /// </summary>
      /// <param name="instance"></param>
      public SmartObjectPoolEntry(
         K instance) : this()
      {
         this.ObjInstance = instance;
      }

      /// <summary>
      /// 
      /// </summary>
      /// <returns></returns>
      public Boolean Lock()
      {
         Monitor.Enter(this.m_Lock);
         try
         {
            if (this.InUse == true)
            {
               return ( false );
            }

            this.InUse = true;
            return ( true );
         }
         finally
         {
            Monitor.Exit(this.m_Lock);
         }
      }

      /// <summary>
      /// 
      /// </summary>
      /// <returns></returns>
      public Boolean Unlock()
      {
         Monitor.Enter(this.m_Lock);
         try
         {
            this.InUse = false;
            return ( true );
         }
         finally
         {
            Monitor.Exit(this.m_Lock);
         }
      }

      /// <summary>
      /// 
      /// </summary>
      public Boolean InUse { get; private set; } = false;

      /// <summary>
      /// 
      /// </summary>
      public Int32 PoolId { get; internal set; } = 0;

      /// <summary>
      /// 
      /// </summary>
      public K ObjInstance { get; set; } = default;
   }
}
