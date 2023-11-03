namespace Application.Common.Interfaces;

public interface IDeadLetterArchiver
{
    Task Archive(object message, IEnumerable<KeyValuePair<string, byte[]>> headers);
}
