-- add table for manifests
CREATE TABLE Manifest (
	"name" text PRIMARY KEY,
   	"data" text NOT NULL,
   	"checksum" text NOT NULL,
   	"lastApplied" text NOT NULL
);