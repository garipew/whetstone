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

	public async Task<List<User>> GetAll()
	{
		using var conn = new NpgsqlConnection(_connectionString);

		const string sql = """
			SELECT *
			FROM users
			""";
		return conn.Query<User>(sql).ToList();
	}

	public async Task<User> Get(int id)
	{
		using var conn = new NpgsqlConnection(_connectionString);

		const string sql = """
			SELECT *
			FROM users
			WHERE Id = @Id
			""";
		return conn.QuerySingle<User>(sql, new { Id = id });
	}

	public async Task<int> Add(User user)
	{
		using var conn = new NpgsqlConnection(_connectionString);

		const string sql = """
			INSERT INTO users (Balance)
			VALUES (@Balance)
			RETURNING Id;
			""";
		return await conn.ExecuteScalarAsync<int>(sql, user);
	}

	public async Task Save(User user)
	{
		using var conn = new NpgsqlConnection(_connectionString);

		const string sql = """
			UPDATE users
			SET Balance = @Balance
			WHERE Id = @Id
			""";

		conn.Execute(sql, user);
	}
}
