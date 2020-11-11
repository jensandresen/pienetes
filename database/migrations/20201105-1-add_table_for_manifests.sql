-- add table for manifests
CREATE TABLE manifest (
	"name" varchar(255) PRIMARY KEY,
   	"data" text NOT NULL,
   	"checksum" varchar(255) NOT NULL,
   	"lastApplied" timestamp NOT NULL
);