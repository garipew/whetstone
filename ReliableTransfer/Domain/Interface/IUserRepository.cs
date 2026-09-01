using System.Data.Common;

namespace ReliableTransfer.Domain;

public interface IUserRepository
{
	public Task<User?> Get(DbTransaction tx, int id);
	public Task Add(DbTransaction tx, User user);
	public Task Save(DbTransaction tx, User user);
}
