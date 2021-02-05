-- add table for queued manifests
CREATE TABLE queued_manifest (
	"id" varchar(255) NOT NULL PRIMARY KEY,
   	"queued_timestamp" timestamp NOT NULL,
   	"is_processed" boolean NOT NULL,
   	"content_type" varchar(1024) NOT NULL,
   	"content" text NOT NULL
);