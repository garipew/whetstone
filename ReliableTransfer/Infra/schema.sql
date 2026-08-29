CREATE TABLE users (
	Id int PRIMARY KEY,
	Balance decimal
);

CREATE TABLE transfers (
	Id int PRIMARY KEY,
	Amount decimal,
	SenderId int FOREIGN KEY,
	ReceiverId int FOREIGN KEY,
	Status string
);
