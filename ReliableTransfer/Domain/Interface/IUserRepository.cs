namespace ReliableTransfer.Domain;

public interface IUserRepository
{
	public Task<User?> Get(int id);
	public Task Add(User user);
	public Task Save(User user);
}
