using System.Data.Common;
using ReliableTransfer.Domain;
using Npgsql;
using Dapper;

namespace ReliableTransfer.Infra;

public class UserRepository : IUserRepository
{
	private readonly string _connectionString;

	public UserRepository(string conString)
	{
		_connectionString = conString;
	}

	public async Task<User?> Get(DbTransaction tx, int id)
	{
		var db = tx.Connection;
		const string sql = """
			SELECT *
			FROM users
			WHERE Id = @Id
			""";
		return await db.QueryFirstOrDefaultAsync<User>(sql, new { Id = id }, tx);
	}

	public async Task Add(DbTransaction tx, User user)
	{
		var db = tx.Connection;
		const string sql = """
			INSERT INTO users (Balance)
			VALUES (@Balance)
			RETURNING Id;
			""";
		user.Id = await db.ExecuteScalarAsync<int>(sql, user, tx);
	}

	public async Task Save(DbTransaction tx, User user)
	{
		var db = tx.Connection;
		const string sql = """
			UPDATE users
			SET Balance = @Balance
			WHERE Id = @Id
			""";

		await db.ExecuteAsync(sql, user, tx);
	}
}
