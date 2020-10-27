const mocha = require("mocha");
const { assert } = require("chai");
const { parseManifest } = require("../manifest-utils");

describe("manifest-utils", async function () {
  describe("parseManifest", async function () {
    it("returns expected name", async function () {
      const expected = "foo";
      const { name } = await parseManifest(`
        service:
          name: ${expected}
      `);

      assert.equal(name, expected);
    });
    it("returns expected image", async function () {
      const expected = "foo:bar";
      const { image } = await parseManifest(`
        service:
          image: ${expected}
      `);

      assert.equal(image, expected);
    });
    it("returns expected ports", async function () {
      const expected = [
        {
          host: 8080,
          container: 80,
        },
      ];
      const { ports } = await parseManifest(`
        service:
          ports:
            - "8080:80"
      `);

      assert.deepEqual(ports, expected);
    });
    it("returns expected ports when not quoted", async function () {
      const expected = [
        {
          host: 8080,
          container: 80,
        },
      ];
      const { ports } = await parseManifest(`
        service:
          ports:
            - 8080:80
      `);

      assert.deepEqual(ports, expected);
    });
  });
});
