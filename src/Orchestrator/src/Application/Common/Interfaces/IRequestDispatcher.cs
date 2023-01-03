namespace Application.Common.Interfaces;

public interface IRequestDispatcher
{
    void SetResponseFor(string requestId, object response);
    Task<object> Dispatch(object request);
}