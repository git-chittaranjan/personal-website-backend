namespace my_api_app.Services.Security.Interfaces
{
    public interface IResetTokenHasher
    {
        byte[] Hash(string input);
    }
}
