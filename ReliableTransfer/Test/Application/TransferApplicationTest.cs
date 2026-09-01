using System;
using System.Data.Common;
using Npgsql;
using Testcontainers.PostgreSql;

using ReliableTransfer.Infra;
using ReliableTransfer.Domain;
using ReliableTransfer.Application;

namespace ReliableTransfer.Test;

public class TransferApplicationTest : IAsyncLifetime
{
	private readonly PostgreSqlContainer _db =
		new PostgreSqlBuilder("postgres:16-alpine")
		.WithDatabase("testdb")
		.WithUsername("test")
		.WithPassword("test")
		.Build();

	private UserRepository users = null!;
	private TransferRepository transfers = null!;
	private TransferApplication sut = null!;

	public async Task InitializeAsync()
	{
		await _db.StartAsync();

		var sql = await File.ReadAllTextAsync(
				Path.Combine(AppContext.BaseDirectory, "../../../../Infra/schema.pgsql"));

		var connString = _db.GetConnectionString();
		await using var conn = new NpgsqlConnection(connString);

		await conn.OpenAsync();

		await using var command = new NpgsqlCommand(sql, conn);
		await command.ExecuteNonQueryAsync();

		users = new UserRepository(connString);
		transfers = new TransferRepository(connString);

		sut = new TransferApplication(users, transfers);
	}

	public async Task DisposeAsync()
	{
		await _db.DisposeAsync();
	}

	private async Task<User> CreateUser(DbConnection con, decimal amount)
	{
		await using var tx = await con.BeginTransactionAsync();
		var user = new User();
		user.Credit(amount);
		await users.Add(tx, user);
		user = (await users.Get(tx, user.Id))!;
		await tx.CommitAsync();
		return user;
	}

	[Fact]
	public async Task ProcessTransfer_ShouldNotReprocessCompleteTransfer()
	{
		var connString = _db.GetConnectionString();
		await using var conn = new NpgsqlConnection(connString);

		await conn.OpenAsync();
		var sender = await CreateUser(conn, 42m);
		var receiver = await CreateUser(conn, 42m);

		await using var other = new NpgsqlConnection(connString);
		await other.OpenAsync();

		var idempotency = Guid.NewGuid();
		var t1 = await sut.ProcessTransfer(conn, idempotency, sender.Id, receiver.Id, 21m);
		var t2 = await sut.ProcessTransfer(other, idempotency, sender.Id, receiver.Id, 21m);

		Assert.Equal(t1.Id, t2.Id);

		using var tx = await other.BeginTransactionAsync();
		sender = (await users.Get(tx, t1.SenderId))!;
		receiver = (await users.Get(tx, t1.ReceiverId))!;
		await tx.CommitAsync();

		Assert.Equal(21m, sender.Balance);
		Assert.Equal(63m, receiver.Balance);
	}

	[Fact]
	public async Task ProcessTransfer_ShouldProcessTransferOnlyOnce()
	{
		var connString = _db.GetConnectionString();
		await using var conn = new NpgsqlConnection(connString);

		await conn.OpenAsync();
		var sender = await CreateUser(conn, 42m);
		var receiver = await CreateUser(conn, 42m);

		await using var other = new NpgsqlConnection(connString);
		await other.OpenAsync();

		var idempotency = Guid.NewGuid();
		var t1 = sut.ProcessTransfer(conn, idempotency, sender.Id, receiver.Id, 21m);
		var t2 = sut.ProcessTransfer(other, idempotency, sender.Id, receiver.Id, 21m);

		var transfers = await Task.WhenAll(t1, t2);

		Assert.Equal(transfers[0].Id, transfers[1].Id);

		using var tx = await other.BeginTransactionAsync();
		sender = (await users.Get(tx, transfers[0].SenderId))!;
		receiver = (await users.Get(tx, transfers[0].ReceiverId))!;
		await tx.CommitAsync();

		Assert.Equal(21m, sender.Balance);
		Assert.Equal(63m, receiver.Balance);
	}
}

