import { readFile } from "fs/promises";
import { Client } from "pg";

export async function getConnectionString() {
  let result = process.env.CONNECTION_STRING;
  if (!result) {
    return null;
  }

  if (result.startsWith("SECRET:")) {
    const filePath = result.substring("SECRET:".length).trim();
    result = await readFile(filePath, { encoding: "utf8 " });
  }

  return result.trim();
}

export async function databaseFactory(connectionString) {
  const client = new Client({
    connectionString,
  });

  await client.connect();

  return client;
}

export default class ManifestRepository {
  constructor(options) {
    const { db, serializer, deserializer, logger } = options;

    if (!db) {
      throw Error(`Error! Missing required argument "db".`);
    }

    if (!serializer) {
      throw Error(`Error! Missing required argument "serializer".`);
    }

    if (!deserializer) {
      throw Error(`Error! Missing required argument "deserializer".`);
    }

    this.db = db;
    this.serializer = serializer;
    this.deserializer = deserializer;
    this.logger = logger || (() => {});

    this.storeManifest = this.storeManifest.bind(this);
    this.findByChecksum = this.findByChecksum.bind(this);
    this.getAll = this.getAll.bind(this);
    this.getByName = this.getByName.bind(this);

    this.dbRead = this.dbRead.bind(this);

    this.exists = this.exists.bind(this);
  }

  async exists(serviceName) {
    const existingManifest = await this.getByName(serviceName);
    return !!existingManifest;
  }

  async storeManifest(serviceName, manifest, checksum) {
    const serializedManifest = await this.serializer(manifest);

    const alreadyExists = await this.exists(serviceName);
    const query = alreadyExists
      ? {
          text: `UPDATE manifest SET data = $1, checksum = $2, "lastApplied" = NOW() WHERE name = $3`,
          values: [serializedManifest, checksum, serviceName],
        }
      : {
          text: `INSERT INTO manifest (name, data, checksum, "lastApplied") VALUES ($1, $2, $3, NOW())`,
          values: [serviceName, serializedManifest, checksum],
        };

    await this.db.query(query);
  }

  async findByChecksum(checksum) {
    const result = await this.dbRead(
      `SELECT * FROM manifest WHERE checksum = '${checksum}'`
    );

    if (result.length > 0) {
      return await this.deserializer(result[0]["data"]);
    }

    return null;
  }

  async getAll() {
    const list = [];

    const result = await this.dbRead("SELECT * FROM manifest");
    for (let row of result) {
      const manifest = await this.deserializer(row["data"]);
      list.push(manifest);
    }

    return list;
  }

  async dbRead(sql) {
    const result = await this.db.query(sql);
    return result.rows || [];
  }

  async getByName(name) {
    const result = await this.dbRead(
      `SELECT * FROM manifest WHERE name = '${name}'`
    );

    if (result.length === 1) {
      return await this.deserializer(result[0]["data"]);
    }

    return null;
  }
}
