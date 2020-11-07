import mocha from "mocha";
import chai from "chai";
import ManifestRepository, {
  inMemoryDatabaseFactory,
} from "../manifest-repository";

import fs from "fs";
import path from "path";

import chaiAsPromised from "chai-as-promised";
chai.use(chaiAsPromised);

const { assert, expect } = chai;

const dummyCallback = () => {};

const getFiles = (dir) =>
  new Promise((resolve) => {
    fs.readdir(dir, (err, entries) => {
      if (err) {
        resolve([]);
      } else {
        const files = entries
          .map((name) => path.join(dir, name))
          .filter((filePath) => fs.statSync(filePath).isFile());
        resolve(files);
      }
    });
  });

const getMigrationScripts = async () => {
  const files = await getFiles(
    path.resolve(process.cwd(), "..", "database", "migrations")
  );

  return files.map((filePath) =>
    fs.readFileSync(filePath, { encoding: "utf8" })
  );
};

const dbWriter = (db, sql) =>
  new Promise((resolve, reject) => {
    db.run(sql, (err) => {
      if (err) {
        reject(err);
      } else {
        resolve();
      }
    });
  });

const dbReader = (db, sql) =>
  new Promise((resolve, reject) => {
    db.all(sql, (err, rows) => {
      if (err) {
        reject(err);
      } else {
        resolve(rows);
      }
    });
  });

describe("manifest-repository", async function () {
  const migrationScripts = await getMigrationScripts();
  const buildDatabase = async () => {
    const db = await inMemoryDatabaseFactory();
    migrationScripts.forEach((script) => {
      db.exec(script);
    });

    return db;
  };

  const buildSut = (overrides) => {
    const defaults = {
      db: null,
      serializer: (obj) => JSON.stringify(obj, null, 2),
      deserializer: (txt) => JSON.parse(txt),
    };
    return new ManifestRepository({ ...defaults, ...overrides });
  };

  describe("ctor", function () {
    // it("throws error if containerService is undefined", function () {
    //   assert.throws(
    //     () => buildSut({ containerService: undefined }),
    //     /.*?containerService.*?/
    //   );
    // });
    // it("throws error if containerService is null", function () {
    //   assert.throws(
    //     () => buildSut({ containerService: null }),
    //     /.*?containerService.*?/
    //   );
    // });
  });

  describe("getAll", async function () {
    it("returns expected when empty", async function () {
      const db = await buildDatabase();
      const sut = buildSut({ db: db });

      const result = await sut.getAll();

      assert.isEmpty(result);
    });

    it("returns expected when single manifest is available", async function () {
      const db = await buildDatabase();
      await dbWriter(
        db,
        `insert into Manifest (name, data, checksum, lastApplied) values('foo', '{ "bar": "baz" }', 'qux', 'dummy-date')`
      );

      const sut = buildSut({ db: db });
      const result = await sut.getAll();

      assert.deepEqual(result, [
        {
          bar: "baz",
        },
      ]);
    });
  });

  describe("storeManifest", async function () {
    it("inserts expected in db", async function () {
      const db = await buildDatabase();
      const sut = buildSut({ db: db });

      await sut.storeManifest("foo", { foo: "bar" }, "foo-checksum");

      const result = await dbReader(db, "select * from Manifest");

      assert.equal(result[0].name, "foo");
      assert.equal(result[0].data, sut.serializer({ foo: "bar" }));
      assert.equal(result[0].checksum, "foo-checksum");
      assert.isNotNull(result[0].lastApplied);
    });

    it("updated expected record in db", async function () {
      const db = await buildDatabase();
      await dbWriter(
        db,
        `insert into Manifest (name, data, checksum, lastApplied) values('foo', '{ "bar": "baz" }', 'qux', 'dummy-date')`
      );

      const sut = buildSut({ db: db });
      await sut.storeManifest("foo", { foo1: "bar1" }, "foo1-checksum");

      const result = await dbReader(
        db,
        "select * from Manifest where name = 'foo'"
      );

      assert.equal(result[0].name, "foo");
      assert.equal(result[0].data, sut.serializer({ foo1: "bar1" }));
      assert.equal(result[0].checksum, "foo1-checksum");
      assert.isNotNull(result[0].lastApplied);
    });
  });

  describe("findByChecksum", async function () {
    it("returns expected when empty", async function () {
      const db = await buildDatabase();
      const sut = buildSut({ db: db });

      const result = await sut.findByChecksum();

      assert.isNull(result);
    });

    it("returns expected when single manifest is available", async function () {
      const db = await buildDatabase();
      await dbWriter(
        db,
        `insert into Manifest (name, data, checksum, lastApplied) values('foo', '{ "bar": "baz" }', 'qux', 'dummy-date')`
      );

      const sut = buildSut({ db: db });
      const result = await sut.findByChecksum("qux");

      assert.deepEqual(result, {
        bar: "baz",
      });
    });

    it("returns expected when single manifest is available but has different checksum", async function () {
      const db = await buildDatabase();
      await dbWriter(
        db,
        `insert into Manifest (name, data, checksum, lastApplied) values('foo', '{ "bar": "baz" }', 'qux', 'dummy-date')`
      );

      const sut = buildSut({ db: db });
      const result = await sut.findByChecksum("non-existing-checksum");

      assert.isNull(result);
    });
  });

  describe("getByName", async function () {
    it("returns expected when empty", async function () {
      const db = await buildDatabase();
      const sut = buildSut({ db: db });

      const result = await sut.getByName();

      assert.isNull(result);
    });

    it("returns expected when single manifest is available", async function () {
      const db = await buildDatabase();
      await dbWriter(
        db,
        `insert into Manifest (name, data, checksum, lastApplied) values('foo', '{ "bar": "baz" }', 'qux', 'dummy-date')`
      );

      const sut = buildSut({ db: db });
      const result = await sut.getByName("foo");

      assert.deepEqual(result, {
        bar: "baz",
      });
    });

    it("returns expected when single manifest is available but has different name", async function () {
      const db = await buildDatabase();
      await dbWriter(
        db,
        `insert into Manifest (name, data, checksum, lastApplied) values('foo', '{ "bar": "baz" }', 'qux', 'dummy-date')`
      );

      const sut = buildSut({ db: db });
      const result = await sut.getByName("non-existing-name");

      assert.isNull(result);
    });
  });
});
