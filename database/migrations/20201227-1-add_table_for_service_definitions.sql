-- add table for service definition
CREATE TABLE service_definition (
	"id" varchar(255) PRIMARY KEY,
   	"image" varchar(1024) NOT NULL,
   	"checksum" varchar(255) NULL,
   	"ports" text NULL,
   	"secrets" text NULL,
   	"environment_variables" text NULL
);