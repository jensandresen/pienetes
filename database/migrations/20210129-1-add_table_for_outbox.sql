-- add table for outbox
CREATE TABLE outbox (
	"message_id" varchar(255) NOT NULL PRIMARY KEY,
   	"message_type" varchar(255) NOT NULL,
   	"aggregate_id" varchar(255) NOT NULL,
   	"custom_headers" text NULL,
   	"payload" text NOT NULL,
   	"created_at" timestamp NOT NULL,
	"sent_at" timestamp NULL
);