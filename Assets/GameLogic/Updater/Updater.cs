using System;

namespace Assets.GameLogic.Updater
{
    public class Updater<T> : IUpdater<T>
        where T : IUpdatable
    {
        private struct BufferData
        {
            public int size;
            public T[] buffer;
        }

        private BufferData bufferData;

        public Updater(int initialBufferSize = 1)
        {
            bufferData.size = 0;
            bufferData.buffer = new T[initialBufferSize <= 0 ? 1 : initialBufferSize];
        }

        public void Update()
        {
            for (int i = 0; i < bufferData.size; ++i)
            {
                bufferData.buffer[i].OnUpdate();
            }
        }

        public void Register(T updatable)
        {
            var buffer = bufferData.buffer;

            if (bufferData.size >= buffer.Length)
            {
                Array.Resize(ref buffer, buffer.Length << 1);
                bufferData.buffer = buffer;
            }

            buffer[bufferData.size++] = updatable;
        }

        public void Deregister(T updatable)
        {
            var buffer = bufferData.buffer;
            int index = Array.IndexOf(buffer, updatable, 0, bufferData.size);

            if (index >= 0)
            {
                Array.Copy(buffer, index + 1, buffer, index, bufferData.size - index - 1);
                buffer[--bufferData.size] = default(T);
            }
        }
    }

}