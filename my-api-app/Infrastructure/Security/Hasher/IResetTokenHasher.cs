namespace my_api_app.Infrastructure.Security.Hasher
{
    public interface IResetTokenHasher
    {
        byte[] Hash(string input);
    }
}
