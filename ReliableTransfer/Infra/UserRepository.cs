using ReliableTransfer.Application;
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

	public async Task<User?> Get(int id)
	{
		await using var conn = new NpgsqlConnection(_connectionString);

		const string sql = """
			SELECT *
			FROM users
			WHERE Id = @Id
			""";
		return await conn.QueryFirstOrDefaultAsync<User>(sql, new { Id = id });
	}

	public async Task Add(User user)
	{
		await using var conn = new NpgsqlConnection(_connectionString);

		const string sql = """
			INSERT INTO users (Balance)
			VALUES (@Balance)
			RETURNING Id;
			""";
		user.Id = await conn.ExecuteScalarAsync<int>(sql, user);
	}

	public async Task Save(User user)
	{
		await using var conn = new NpgsqlConnection(_connectionString);

		const string sql = """
			UPDATE users
			SET Balance = @Balance
			WHERE Id = @Id
			""";

		await conn.ExecuteAsync(sql, user);
	}
}
