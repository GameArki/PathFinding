using System.Collections.Generic;

namespace GameArki.PathFinding.Generic {

    public class ObjectPool<T> where T : new() {

        readonly Queue<T> objects;
        readonly int maxSize;

        public ObjectPool(int maxSize) {
            objects = new Queue<T>(maxSize);
            this.maxSize = maxSize;
        }

        public bool TryDequeue(out T obj) {
            obj = Dequeue();
            return obj != null;
        }

        public T Dequeue() {
            if (objects.Count == 0) {
                return new T();
            }

            return objects.Dequeue();
        }

        public bool TryEnqueue(T obj) {
            if (objects.Count >= maxSize) return false;
            objects.Enqueue(obj);
            return true;
        }

        public void Enqueue(T obj) {
            if (objects.Count >= maxSize) return;
            objects.Enqueue(obj);
        }

    }

}
