namespace ShazamIO.Algorithm;

/// <summary>
/// A circular buffer implementation for audio processing.
/// </summary>
/// <typeparam name="T">The type of elements in the buffer</typeparam>
public class RingBuffer<T>
{
    private readonly T[] _buffer;
    private int _position;

    public int BufferSize { get; }
    public int NumWritten { get; private set; }

    public RingBuffer(int bufferSize, T? defaultValue = default)
    {
        BufferSize = bufferSize;
        _buffer = new T[bufferSize];
        
        if (defaultValue != null && !EqualityComparer<T>.Default.Equals(defaultValue, default))
        {
            for (int i = 0; i < bufferSize; i++)
            {
                if (defaultValue is ICloneable cloneable)
                    _buffer[i] = (T)cloneable.Clone();
                else
                    _buffer[i] = defaultValue;
            }
        }
    }

    public int Position
    {
        get => _position;
        set => _position = value % BufferSize;
    }

    public T this[int index]
    {
        get => _buffer[((index % BufferSize) + BufferSize) % BufferSize];
        set => _buffer[((index % BufferSize) + BufferSize) % BufferSize] = value;
    }

    public void Append(T value)
    {
        _buffer[_position] = value;
        _position = (_position + 1) % BufferSize;
        NumWritten++;
    }

    /// <summary>
    /// Gets a range of elements from the buffer, wrapping around if necessary.
    /// </summary>
    public T[] GetRange(int start, int count)
    {
        var result = new T[count];
        for (int i = 0; i < count; i++)
        {
            result[i] = this[start + i];
        }
        return result;
    }

    /// <summary>
    /// Sets a range of elements in the buffer starting at the specified index.
    /// </summary>
    public void SetRange(int startIndex, T[] values)
    {
        for (int i = 0; i < values.Length; i++)
        {
            this[startIndex + i] = values[i];
        }
    }

    /// <summary>
    /// Gets all elements in order from current position.
    /// </summary>
    public T[] ToArray()
    {
        var result = new T[BufferSize];
        for (int i = 0; i < BufferSize; i++)
        {
            result[i] = _buffer[(_position + i) % BufferSize];
        }
        return result;
    }
}
