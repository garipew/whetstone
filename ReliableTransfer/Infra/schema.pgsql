CREATE TABLE users (
	Id int GENERATED ALWAYS AS IDENTITY PRIMARY KEY,
	Balance decimal NOT NULL DEFAULT 0 CHECK (Balance >= 0)
);

CREATE TABLE transfers (
	Id int GENERATED ALWAYS AS IDENTITY PRIMARY KEY,
	Amount decimal NOT NULL CHECK (Amount > 0),
	SenderId int NOT NULL REFERENCES users(Id),
	ReceiverId int NOT NULL REFERENCES users(Id),
	Status varchar(20) NOT NULL DEFAULT 'pending',

	CHECK (SenderId <> ReceiverId)
);
