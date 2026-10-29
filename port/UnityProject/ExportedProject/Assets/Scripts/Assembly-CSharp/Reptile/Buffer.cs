using System.Collections.Generic;

namespace Reptile
{
	public class Buffer<T>
	{
		private int maxBufferSize;

		private List<T> buffer;

		public int BufferSize => buffer.Count;

		public Buffer(int maxBufferSize)
		{
			this.maxBufferSize = maxBufferSize;
			buffer = new List<T>();
		}

		public void Dispose()
		{
			buffer.Clear();
			buffer = null;
		}

		public void AddElement(T element)
		{
			HoldBufferSize();
			AddElementToBuffer(element);
		}

		private void HoldBufferSize()
		{
			if (buffer.Count >= maxBufferSize)
			{
				RemoveFirstElementFromBuffer();
			}
		}

		private void AddElementToBuffer(T element)
		{
			buffer.Add(element);
		}

		private void RemoveFirstElementFromBuffer()
		{
			buffer.RemoveAt(0);
		}

		public T GetElementFromTextBuffer(int index)
		{
			return buffer[index];
		}
	}
}
