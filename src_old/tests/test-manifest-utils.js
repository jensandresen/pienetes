import mocha from "mocha";
import { assert } from "chai";
import { manifestDeserializer } from "../manifest-utils";

describe("manifest-utils", async function () {
  describe("manifestDeserializer", async function () {
    it("returns expected name", async function () {
      const expected = "foo";
      const { service } = await manifestDeserializer(`
        service:
          name: ${expected}
      `);

      assert.equal(service.name, expected);
    });

    it("returns expected image", async function () {
      const expected = "foo:bar";
      const { service } = await manifestDeserializer(`
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
      const { service } = await manifestDeserializer(`
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
      const { service } = await manifestDeserializer(`
        service:
          ports:
            - 8080:80
      `);

      assert.deepEqual(service.ports, expected);
    });

    it("returns expected environment variable when a single one is defined", async function () {
      const { service } = await manifestDeserializer(`
        service:
          environmentVariables:
            foo: bar
      `);

      assert.deepEqual(service.environmentVariables, { foo: "bar" });
    });

    it("returns expected environment variable when multiple is defined", async function () {
      const { service } = await manifestDeserializer(`
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

    describe("secrets", async function () {
      it("returns expected secrets when none is defined", async function () {
        const expected = undefined;

        const { service } = await manifestDeserializer(`
        service:
          name: dummy
      `);

        assert.deepEqual(service.secrets, expected);
      });

      it("returns expected secrets when single is defined", async function () {
        const expected = [
          {
            name: "foo",
            mountPath: undefined,
          },
        ];

        const { service } = await manifestDeserializer(`
        service:
          secrets:
            - foo
      `);

        assert.deepEqual(service.secrets, expected);
      });

      it("returns expected secrets when single is defined with double quotes", async function () {
        const expected = [
          {
            name: "foo",
            mountPath: undefined,
          },
        ];

        const { service } = await manifestDeserializer(`
        service:
          secrets:
            - "foo"
      `);

        assert.deepEqual(service.secrets, expected);
      });

      it("returns expected secrets when single is defined with single quotes", async function () {
        const expected = [
          {
            name: "foo",
            mountPath: undefined,
          },
        ];

        const { service } = await manifestDeserializer(`
        service:
          secrets:
            - 'foo'
      `);

        assert.deepEqual(service.secrets, expected);
      });

      it("returns expected secrets when multiple is defined", async function () {
        const expected = [
          {
            name: "foo",
            mountPath: undefined,
          },
          {
            name: "bar",
            mountPath: undefined,
          },
        ];

        const { service } = await manifestDeserializer(`
        service:
          secrets:
            - foo
            - bar
      `);

        assert.deepEqual(service.secrets, expected);
      });

      it("returns expected secrets when single is defined with mount path", async function () {
        const expected = [
          {
            name: "foo",
            mountPath: "bar",
          },
        ];

        const { service } = await manifestDeserializer(`
        service:
          secrets:
            - foo:bar
      `);

        assert.deepEqual(service.secrets, expected);
      });

      it("returns expected secrets when single is defined in quotes and with mount path", async function () {
        const expected = [
          {
            name: "foo",
            mountPath: "bar",
          },
        ];

        const { service } = await manifestDeserializer(`
        service:
          secrets:
            - "foo:bar"
      `);

        assert.deepEqual(service.secrets, expected);
      });

      it("returns expected secrets when multiple is defined with mount path", async function () {
        const expected = [
          {
            name: "foo",
            mountPath: "bar",
          },
          {
            name: "baz",
            mountPath: "qux",
          },
        ];

        const { service } = await manifestDeserializer(`
        service:
          secrets:
            - foo:bar
            - baz:qux
      `);

        assert.deepEqual(service.secrets, expected);
      });
    });
  });
});
