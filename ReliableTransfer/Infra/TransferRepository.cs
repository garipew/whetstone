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

	public async Task<Transfer?> GetByIdempotency(Guid idempotency)
	{
		await using var conn = new NpgsqlConnection(_connectionString);
		const string sql = """
			SELECT Id, SenderId, ReceiverId, Amount
			FROM transfers
			WHERE IdempotencyKey = @Idempotency
			""";
		return await conn.QueryFirstOrDefaultAsync<Transfer>(sql, new { Idempotency = idempotency });
	}

	public async Task Add(Guid idempotency, Transfer t)
	{
		await using var conn = new NpgsqlConnection(_connectionString);

		const string sql = """
			INSERT INTO transfers (IdempotencyKey, Amount, SenderId, ReceiverId, Status)
			VALUES (@IdempotencyKey, @Amount, @SenderId, @ReceiverId, @Status)
			RETURNING Id
			""";
		t.Id = await conn.ExecuteScalarAsync<int>(sql, new {
				IdempotencyKey = idempotency,
				Amount = t.Amount,
				SenderId = t.SenderId,
				ReceiverId = t.ReceiverId,
				Status = t.Status
				});
	}

	public async Task Save(Transfer t)
	{
		await using var conn = new NpgsqlConnection(_connectionString);

		const string sql = """
			UPDATE transfers
			SET Amount = @Amount,
			SenderId = @SenderId,
			ReceiverId = @ReceiverId,
			Status = @Status
			WHERE Id = @Id
			""";

		await conn.ExecuteAsync(sql, t);
	}
}
