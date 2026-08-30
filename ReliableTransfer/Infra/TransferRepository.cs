using ReliableTransfer.Application;
using ReliableTransfer.Domain;
using Npgsql;
using Dapper;

namespace ReliableTransfer.Infra;

public class TransferRepository : ITransferRepository
{
	private readonly string _connectionString;

	public TransferRepository(string conString)
	{
		_connectionString = conString;
	}

	public Transfer? GetByIdempotency(Guid idempotency)
	{
		using var conn = new NpgsqlConnection(_connectionString);
		const string sql = """
			SELECT Id, SenderId, ReceiverId, Amount
			FROM transfers
			WHERE IdempotencyKey = @Idempotency
			""";
		return conn.QueryFirstOrDefault<Transfer>(sql, new { Idempotency = idempotency });
	}

	public void Add(Guid idempotency, Transfer t)
	{
		using var conn = new NpgsqlConnection(_connectionString);

		const string sql = """
			INSERT INTO transfers (IdempotencyKey, Amount, SenderId, ReceiverId, Status)
			VALUES (@IdempotencyKey, @Amount, @SenderId, @ReceiverId, @Status)
			RETURNING Id
			""";
		t.Id = conn.ExecuteScalar<int>(sql, new {
				IdempotencyKey = idempotency,
				Amount = t.Amount,
				SenderId = t.SenderId,
				ReceiverId = t.ReceiverId,
				Status = t.Status
				});
	}

	public void Save(Transfer t)
	{
		using var conn = new NpgsqlConnection(_connectionString);

		const string sql = """
			UPDATE transfers
			SET Amount = @Amount,
			SenderId = @SenderId,
			ReceiverId = @ReceiverId,
			Status = @Status
			WHERE Id = @Id
			""";
		var rowsAffected = conn.Execute(sql, t);
	}
}
