using System.Collections.Generic;

namespace GameArki.PathFinding.Generic
{

    public class PriorityQueue<T>
    {

        List<T> _data;
        IComparer<T> _comparer;

        public int Count => _data.Count;

        public PriorityQueue() : this(Comparer<T>.Default)
        {
        }

        public PriorityQueue(IComparer<T> comparer)
        {
            _data = new List<T>();
            _comparer = comparer;
        }

        public void Enqueue(T item)
        {
            int childIndex = _data.Count;
            _data.Add(item);
            while (childIndex > 0)
            {
                int parentIndex = (childIndex - 1) / 2;
                if (_comparer.Compare(_data[childIndex], _data[parentIndex]) >= 0)
                {
                    break;
                }

                T temp = _data[childIndex];
                _data[childIndex] = _data[parentIndex];
                _data[parentIndex] = temp;
                childIndex = parentIndex;
            }
        }

        public T Dequeue()
        {
            T frontItem = _data[0];
            _data[0] = _data[_data.Count - 1];
            _data.RemoveAt(_data.Count - 1);

            int parentIndex = 0;
            while (true)
            {
                int childIndex = parentIndex * 2 + 1;
                if (childIndex > _data.Count - 1)
                {
                    break;
                }

                int rightChildIndex = childIndex + 1;
                if (rightChildIndex <= _data.Count - 1 && _comparer.Compare(_data[rightChildIndex], _data[childIndex]) < 0)
                {
                    childIndex = rightChildIndex;
                }

                if (_comparer.Compare(_data[parentIndex], _data[childIndex]) <= 0)
                {
                    break;
                }

                T temp = _data[parentIndex];
                _data[parentIndex] = _data[childIndex];
                _data[childIndex] = temp;
                parentIndex = childIndex;
            }

            return frontItem;
        }

        public T Peek() => _data[0];

        public void Clear() => _data.Clear();

    }

}