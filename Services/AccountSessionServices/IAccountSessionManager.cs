namespace Services.AccountSessionServices
{
    public interface IAccountSessionManager
    {
        void CreateSession(string username);
        void DeleteSessionCookie();
        (string, string) ReadSessionCookie();
    }
}