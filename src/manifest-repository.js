import sqlite, { OPEN_READWRITE } from "sqlite3";

const sq = sqlite.verbose();

export function inMemoryDatabaseFactory() {
  return new Promise((resolve, reject) => {
    const db = new sq.Database(":memory:");
    resolve(db);
  });
}

export function fileBasedDatabaseFactory(filePath) {
  return new Promise((resolve, reject) => {
    const db = new sq.Database(filePath, OPEN_READWRITE);
    resolve(db);
  });
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
    this.dbWrite = this.dbWrite.bind(this);
  }

  async storeManifest(serviceName, manifest, checksum) {
    const existingManifest = await this.getByName(serviceName);

    const sql = !!existingManifest
      ? `UPDATE Manifest SET data = $data, checksum = $checksum, lastApplied = $lastApplied WHERE name = '${serviceName}'`
      : "INSERT INTO Manifest (name, data, checksum, lastApplied) VALUES ($name, $data, $checksum, $lastApplied)";

    const params = !!existingManifest
      ? {
          $data: await this.serializer(manifest),
          $checksum: checksum,
          $lastApplied: new Date().toUTCString(),
        }
      : {
          $name: serviceName,
          $data: await this.serializer(manifest),
          $checksum: checksum,
          $lastApplied: new Date().toUTCString(),
        };

    const self = this;
    return new Promise((resolve, reject) => {
      self.db.run(sql, params, (err) => {
        if (err) {
          reject(err);
        } else {
          self.logger(`Success! Stored manifest for "${serviceName}".`);
          resolve();
        }
      });
    });
  }

  async findByChecksum(checksum) {
    const result = await this.dbRead(
      `SELECT * FROM Manifest WHERE checksum = '${checksum}'`
    );

    if (result.length > 0) {
      return await this.deserializer(result[0]["data"]);
    }

    return null;
  }

  async getAll() {
    const list = [];

    const result = await this.dbRead("SELECT * FROM Manifest");
    for (let row of result) {
      const manifest = await this.deserializer(row["data"]);
      list.push(manifest);
    }

    return list;
  }

  dbRead(sql) {
    const self = this;
    return new Promise((resolve, reject) => {
      self.db.all(sql, (err, rows) => {
        if (err) {
          reject(err);
        } else {
          resolve(rows || []);
        }
      });
    });
  }

  dbWrite(sql, params) {
    const self = this;
    return new Promise((resolve, reject) => {
      self.db.run(sql, params, (err) => {
        if (err) {
          reject(err);
        } else {
          resolve();
        }
      });
    });
  }

  async getByName(name) {
    const result = await this.dbRead(
      `SELECT * FROM Manifest WHERE name = '${name}'`
    );

    if (result.length === 1) {
      return await this.deserializer(result[0]["data"]);
    }

    return null;
  }
}
