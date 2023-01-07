using System;
using System.Collections.Generic;

namespace GameArki.PathFinding.Generic
{

    public class Heap<T>
    {


        int _count;
        public int Count => _count;

        public T[] _items;
        Comparer<T> _comparer;
        int _capacity;

        public Heap(Comparer<T> comparer, int capacity)
        {
            this._comparer = comparer;
            this._capacity = capacity;
            this._count = 0;
            this._items = new T[capacity];
        }

        public void Push(T value)
        {
            if (_count == _capacity)
            {
                throw new Exception("Heap is full");
            }

            _items[_count] = value;
            _count++;

            HeapifyUp();
        }

        void HeapifyUp()
        {
            int index = _count - 1;
            var parentIndex = GetParentIndex(index);
            while (index > 0 && NeedSwap(parentIndex, index))
            {
                Swap(parentIndex, index);
                index = parentIndex;
                parentIndex = GetParentIndex(index);
            }
        }

        public T Pop()
        {
            if (_count == 0)
            {
                throw new Exception("Heap is empty");
            }

            var min = _items[0];
            _items[0] = _items[_count - 1];
            _count--;
            HeapifyDown();
            return min;
        }

        void HeapifyDown()
        {
            int index = 0;
            while (HasLeftChild(index))
            {

                int smallerChildIndex = GetComparedChild(index);

                if (!NeedSwap(index, smallerChildIndex))
                {
                    break;
                }

                Swap(index, smallerChildIndex);
                index = smallerChildIndex;
            }
        }

        int GetParentIndex(int index)
        {
            return (index - 1) / 2;
        }

        void Swap(int index1, int index2)
        {
            var temp = _items[index1];
            _items[index1] = _items[index2];
            _items[index2] = temp;
        }

        int GetLeftChild(int index)
        {
            return index * 2 + 1;
        }

        int GetRightChild(int index)
        {
            return index * 2 + 2;
        }

        int GetComparedChild(int index)
        {
            int leftChildIndex = GetLeftChild(index);
            int rightChildIndex = GetRightChild(index);
            if (NeedSwap(rightChildIndex, leftChildIndex)) return leftChildIndex;
            return rightChildIndex;
        }

        bool HasLeftChild(int index)
        {
            return GetLeftChild(index) < _count;
        }

        bool HasRightChild(int index)
        {
            return GetRightChild(index) < _count;
        }

        bool NeedSwap(int parentIndex, int childIndex)
        {
            return _comparer.Compare(_items[parentIndex], _items[childIndex]) >= 0;
        }

    }

}