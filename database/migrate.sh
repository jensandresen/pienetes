#!/bin/bash

MIGRATION_TABLE_NAME="_Migration"
FINAL_SCRIPT_FILE_PATH=${PWD}/db_bootstrap.sql
DATABASE_FILE_PATH=${DATABASE_FILE:-"$PWD/pienetes-store.db"}

FINAL_SCRIPT="\n"
FINAL_SCRIPT+="-- migration script management table\n"
FINAL_SCRIPT+="BEGIN TRANSACTION;\n"
FINAL_SCRIPT+="CREATE TABLE IF NOT EXISTS $MIGRATION_TABLE_NAME (\n"
FINAL_SCRIPT+="     script_name nvarchar(255) PRIMARY KEY,\n"
FINAL_SCRIPT+="     date_applied datetime NOT NULL\n"
FINAL_SCRIPT+=");\n"
FINAL_SCRIPT+="COMMIT;\n"
FINAL_SCRIPT+="\n"
FINAL_SCRIPT+="\n"

FILES=${PWD}/migrations/*.sql
for f in $FILES
do
    FILENAME=$(basename $f)
    FINAL_SCRIPT+="-- file: $FILENAME\n"
    FINAL_SCRIPT+="BEGIN TRANSACTION;\n"
    FINAL_SCRIPT+="INSERT INTO $MIGRATION_TABLE_NAME (script_name, date_applied) VALUES ('$FILENAME', datetime('now'));\n"
    FINAL_SCRIPT+=$(cat $f)
    FINAL_SCRIPT+="\n"
    FINAL_SCRIPT+="COMMIT;\n"
    FINAL_SCRIPT+="\n"
    FINAL_SCRIPT+="\n"
done

echo "Generating database bootstrap script..."
printf "$FINAL_SCRIPT" > "$FINAL_SCRIPT_FILE_PATH"

echo "Bootstrapping the database..."
sqlite3 "$DATABASE_FILE_PATH" ".read $FINAL_SCRIPT_FILE_PATH"

echo "Removing bootstrap script"
rm -Rf "$FINAL_SCRIPT_FILE_PATH"

echo "Done!"
echo "Database ready: $DATABASE_FILE_PATH"