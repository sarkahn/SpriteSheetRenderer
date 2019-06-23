
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace DOTSSpriteRenderer.Utils
{
    public class MultiListDictionary<KEY, VALUE> : IEnumerable<(KEY, List<VALUE>)>
    {
        Dictionary<KEY, List<VALUE>> dict_ = new Dictionary<KEY, List<VALUE>>();

        public Dictionary<KEY, List<VALUE>>.KeyCollection Keys => dict_.Keys;

        public System.Action<List<VALUE>> onRemove_;

        public List<VALUE> this[KEY k] => dict_[k];
        
        public void Add(KEY key, VALUE value)
        {
            GetOrCreateValueList(key).Add(value);
        }

        public void Remove(KEY key, VALUE val)
        {
            List<VALUE> list;
            if (!dict_.TryGetValue(key, out list))
                return;
            list.Remove(val);
            if (list.Count == 0)
            {
                onRemove_?.Invoke(list);
                dict_.Remove(key);
                ListPool<VALUE>.Put(list);
            }
        }

        public void Remove(KEY key)
        {
            List<VALUE> list;
            if (!dict_.TryGetValue(key, out list))
                return;

            onRemove_?.Invoke(list);
            dict_.Remove(key);
            ListPool<VALUE>.Put(list);
        }

        public List<VALUE> GetOrCreateValueList(KEY key)
        {
            List<VALUE> list;
            if (!dict_.TryGetValue(key, out list))
                dict_[key] = list = ListPool<VALUE>.Get();

            return list;
        }

        public bool Contains(KEY key, VALUE value)
        {
            List<VALUE> list;
            if (!dict_.TryGetValue(key, out list))
                return false;
            return list.Contains(value);
        }

        public bool TryGetValues(KEY key, out List<VALUE> list)
        {
            return dict_.TryGetValue(key, out list);
        }

        public void Clear()
        {
            var keys = dict_.Keys;
            List<KEY> keyList = new List<KEY>(keys);
            foreach (var key in keyList)
                Remove(key);
        }

        public int KeyCount => dict_.Count;

        #region ENUMERABLE

        public Enumerator GetEnumerator()
        {
            return new Enumerator(dict_.GetEnumerator());
        }

        IEnumerator<(KEY, List<VALUE>)> IEnumerable<(KEY, List<VALUE>)>.GetEnumerator()
        {
            return GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }

        public struct Enumerator : IEnumerator<(KEY, List<VALUE>)>
        {
            Dictionary<KEY, List<VALUE>>.Enumerator sourceEnumerator_;


            public (KEY, List<VALUE>) Current =>
                (sourceEnumerator_.Current.Key, sourceEnumerator_.Current.Value);

            object IEnumerator.Current => Current;

            public Enumerator(Dictionary<KEY, List<VALUE>>.Enumerator enumerator)
            {
                sourceEnumerator_ = enumerator;
            }

            public void Dispose() { }

            public bool MoveNext() => sourceEnumerator_.MoveNext();

            public void Reset() { }
        }

        #endregion
    }
}