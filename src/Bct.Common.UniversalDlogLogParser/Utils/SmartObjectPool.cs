using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Bct.Common.LogParsing.Utils
{
   /// <summary>
   /// 
   /// </summary>
   public class SmartObjectPool<T>
   {
      /// <summary>
      /// 
      /// </summary>
      private Dictionary<Int32, SmartObjectPoolEntry<T>> m_ObjectPool = new Dictionary<Int32, SmartObjectPoolEntry<T>>();

      /// <summary>
      /// 
      /// </summary>
      public SmartObjectPool()
      {
      }

      /// <summary>
      /// 
      /// </summary>
      /// <param name="o"></param>
      public void Add(
         T o)
      {
         Monitor.Enter(this.m_ObjectPool);
         try
         {
            Int32 poolId = m_ObjectPool.Keys.Count;
            SmartObjectPoolEntry<T> entry = new SmartObjectPoolEntry<T>()
            {
               ObjInstance = o,
               PoolId = poolId,
            };

            m_ObjectPool.Add(poolId, entry);
         }
         finally
         {
            Monitor.Exit(this.m_ObjectPool);
         }
      }

      /// <summary>
      /// 
      /// </summary>
      /// <returns></returns>
      public SmartObjectPoolEntry<T> GetNoTimeout()
      {
         do
         {
            var entry = m_ObjectPool.FirstOrDefault(item => item.Value.InUse == false);
            if (null != entry.Value)
            {
               if (true == entry.Value.Lock())
               {
                  return ( entry.Value );
               }
            }
            Thread.Sleep(10);
         }
         while (true);
      }

      /// <summary>
      /// 
      /// </summary>
      /// <param name="poolEntry"></param>
      /// <returns></returns>
      public Boolean Release(
         SmartObjectPoolEntry<T> poolEntry)
      {
         if (null == poolEntry)
         {
            return ( false );
         }

         var entry = this.m_ObjectPool.First(item => item.Key == poolEntry.PoolId);
         return ( entry.Value.Unlock() );
      }
   }
}
