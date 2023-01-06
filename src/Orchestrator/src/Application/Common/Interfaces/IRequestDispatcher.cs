namespace Application.Common.Interfaces;

public interface IRequestDispatcher
{
    void SetResponseFor(string requestId, object response);
    Task<object> GetResult(object request);
    Task Send(object request);
}