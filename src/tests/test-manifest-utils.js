const mocha = require("mocha");
const { assert } = require("chai");
const { parseManifest } = require("../manifest-utils");

describe("manifest-utils", async function () {
  describe("parseManifest", async function () {
    it("returns expected name", async function () {
      const expected = "foo";
      const { service } = await parseManifest(`
        service:
          name: ${expected}
      `);

      assert.equal(service.name, expected);
    });

    it("returns expected image", async function () {
      const expected = "foo:bar";
      const { service } = await parseManifest(`
        service:
          image: ${expected}
      `);

      assert.equal(service.image, expected);
    });

    it("returns expected ports", async function () {
      const expected = [
        {
          host: 8080,
          container: 80,
        },
      ];
      const { service } = await parseManifest(`
        service:
          ports:
            - "8080:80"
      `);

      assert.deepEqual(service.ports, expected);
    });

    it("returns expected ports when not quoted", async function () {
      const expected = [
        {
          host: 8080,
          container: 80,
        },
      ];
      const { service } = await parseManifest(`
        service:
          ports:
            - 8080:80
      `);

      assert.deepEqual(service.ports, expected);
    });

    it("returns expected environment variable when a single one is defined", async function () {
      const { service } = await parseManifest(`
        service:
          environmentVariables:
            foo: bar
      `);

      assert.deepEqual(service.environmentVariables, { foo: "bar" });
    });

    it("returns expected environment variable when multiple is defined", async function () {
      const { service } = await parseManifest(`
        service:
          environmentVariables:
            foo: bar
            baz: qux
      `);

      assert.deepEqual(service.environmentVariables, {
        foo: "bar",
        baz: "qux",
      });
    });
  });
});
