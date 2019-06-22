using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace ECSSprites.Util
{
    public static class ListPool<T>
    {
        static Stack<List<T>> pool_ = new Stack<List<T>>();

        public static List<T> Get()
        {
            List<T> list;

            if (pool_.Count == 0)
            {
                list = new List<T>();
            }
            else
                list = pool_.Pop();

            list.Clear();

            return list;
        }

        public static void Put(List<T> list)
        {
            list.Clear();
            pool_.Push(list);
        }
    }
}
