using System.Runtime.Serialization;

namespace Yusnaan.Common;

[Serializable]
public class MipMapException : Exception
{
    public MipMapException(string message)
        : base(message)
    {
    }

    public MipMapException(string message, Exception innerException)
        : base(message, innerException)
    {
    }

    public MipMapException(SerializationInfo info, StreamingContext context)
        : base(info, context)
    {
    }
}

[Serializable]
public class ScenarieIncompatibleException : Exception
{
    public ScenarieIncompatibleException(string message)
        : base(message)
    {
    }

    public ScenarieIncompatibleException(string message, Exception innerException)
        : base(message, innerException)
    {
    }

    public ScenarieIncompatibleException(SerializationInfo info, StreamingContext context)
        : base(info, context)
    {
    }
}