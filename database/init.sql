-- initialize migration table
CREATE TABLE IF NOT EXISTS _Migration (
	script_name nvarchar(255) PRIMARY KEY,
   	date_applied datetime NOT NULL
);