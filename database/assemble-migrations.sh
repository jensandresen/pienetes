#!/bin/bash

# Run the Postgres database migrations as well as seeding the database
readonly PSQL="psql -X -q -v ON_ERROR_STOP=1 --pset pager=off"
readonly MIGRATION_TABLE_NAME="_Migrations"
readonly SCHEMA_NAME="public"

FINAL_SCRIPT_FILE_PATH=${PWD}/db_bootstrap.sql
FINAL_SCRIPT=""

if [ -n "${DEBUG+set}" ]; then
    set -x
fi

set -eu

wait_for_database_service() {
    readonly TIMEOUT=1s
    until pg_isready -q -h ${PGHOST} -p ${PGPORT}; do
        echo "postgres is unavailable - waiting ${TIMEOUT}..."
        sleep $TIMEOUT
    done

    echo "postgres is up - preparing..."
}

add_migration_table() {
    echo "Creating '${MIGRATION_TABLE_NAME}' table..."
    FINAL_SCRIPT+="
--
-- BEGIN: Migration table
-- 
DO \$\$
BEGIN
    CREATE TABLE IF NOT EXISTS ${SCHEMA_NAME}.\"${MIGRATION_TABLE_NAME}\"
    (
        \"script_file\"  varchar(255) NOT NULL PRIMARY KEY,
        \"date_applied\" timestamp NOT NULL
    );
END;
\$\$;

DO \$\$
BEGIN
    CREATE UNIQUE INDEX IF NOT EXISTS ${SCHEMA_NAME}_${MIGRATION_TABLE_NAME}_script_file_idx ON ${SCHEMA_NAME}.\"${MIGRATION_TABLE_NAME}\" (script_file);
END;
\$\$;
--
-- END: Migration table
--
\n\n
"
}

assemble_migrations() {
    for f in $(ls $PWD/migrations/*.sql | sort)
    do
        SCRIPT_NAME=$(basename $f)
        echo "Adding script: $SCRIPT_NAME"
        FINAL_SCRIPT+="
--
-- BEGIN: file $SCRIPT_NAME
--
DO \$\$
BEGIN
    IF NOT EXISTS (SELECT 1 FROM ${SCHEMA_NAME}.\"${MIGRATION_TABLE_NAME}\" WHERE script_file = '${SCRIPT_NAME}') THEN

$(cat $f)

        INSERT INTO ${SCHEMA_NAME}.\"${MIGRATION_TABLE_NAME}\" (script_file, date_applied) VALUES ('${SCRIPT_NAME}', NOW());
    END IF;    
END;
\$\$;
--
-- END: file $SCRIPT_NAME
--
\n\n
"
    done
}

wait_for_database_service
add_migration_table
assemble_migrations

printf "$FINAL_SCRIPT" > $FINAL_SCRIPT_FILE_PATH

echo "Running migration script..."
${PSQL} -f ${FINAL_SCRIPT_FILE_PATH}

echo "Done"

