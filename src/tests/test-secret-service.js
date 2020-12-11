import mocha from "mocha";
import { assert } from "chai";
import path from "path";

import SecretService from "../secret-service";

const buildSut = (overrides) => {
  const defaults = {
    secretRepository: {
      readSecret: () => Promise.resolve(),
    },
    secretFileWriter: () => Promise.resolve(),
    localSecretsDirPath: "foo-local",
    hostSecretsDirPath: "foo-host",
  };
  return new SecretService({ ...defaults, ...overrides });
};

const buildServiceDefinition = (overrides) => {
  const defaults = {
    name: "foo-service-name",
    image: "foo-service-image",
  };

  return { ...defaults, ...overrides };
};

describe("secret-service", async function () {
  describe("prepare secrets", async function () {
    describe("return value", async function () {
      it("returns expected when no secrets is specified", async function () {
        const sut = buildSut();
        const result = await sut.prepareSecrets(
          buildServiceDefinition({ secrets: undefined })
        );

        assert.isEmpty(result);
      });

      it("returns expected when single secret is specified", async function () {
        const sut = buildSut({
          localSecretsDirPath: "foo-local",
          hostSecretsDirPath: "foo-host",
        });

        const result = await sut.prepareSecrets(
          buildServiceDefinition({
            name: "foo-service",
            secrets: [
              {
                name: "foo-secret-name",
                mountPath: "foo-secret-path",
              },
            ],
          })
        );

        assert.deepEqual(result, [
          {
            hostFile: path.join("foo-host", "foo-service", "foo-secret-name"),
            containerFile: "foo-secret-path",
          },
        ]);
      });

      it("returns expected when multiple is specified", async function () {
        const sut = buildSut({
          localSecretsDirPath: "foo-local",
          hostSecretsDirPath: "foo-host",
        });

        const result = await sut.prepareSecrets(
          buildServiceDefinition({
            name: "foo-service",
            secrets: [
              {
                name: "foo-secret-name",
                mountPath: "foo-secret-path",
              },
              {
                name: "bar-secret-name",
                mountPath: "bar-secret-path",
              },
            ],
          })
        );

        assert.deepEqual(result, [
          {
            hostFile: path.join("foo-host", "foo-service", "foo-secret-name"),
            containerFile: "foo-secret-path",
          },
          {
            hostFile: path.join("foo-host", "foo-service", "bar-secret-name"),
            containerFile: "bar-secret-path",
          },
        ]);
      });

      it("returns expected when secret without mountpath is specified", async function () {
        const sut = buildSut({
          localSecretsDirPath: "foo-local",
          hostSecretsDirPath: "foo-host",
        });

        const result = await sut.prepareSecrets(
          buildServiceDefinition({
            name: "foo-service",
            secrets: [
              {
                name: "foo-secret-name",
                mountPath: "",
              },
            ],
          })
        );

        assert.deepEqual(result, [
          {
            hostFile: path.join("foo-host", "foo-service", "foo-secret-name"),
            containerFile: path.join("/", "run", "secrets", "foo-secret-name"),
          },
        ]);
      });
    });

    describe("writes secret to file", async function () {
      it("retrieves expected secret", async function () {
        let retrievedSecretName = undefined;

        const sut = buildSut({
          secretRepository: {
            readSecret: (name) => {
              retrievedSecretName = name;
              return Promise.resolve("dummy");
            },
          },
        });

        await sut.prepareSecrets(
          buildServiceDefinition({
            secrets: [
              {
                name: "foo",
                mountPath: "dummy",
              },
            ],
          })
        );

        assert.equal(retrievedSecretName, "foo");
      });

      it("writes expected secret", async function () {
        let writtenValue = undefined;

        const sut = buildSut({
          secretRepository: {
            readSecret: () => Promise.resolve("foo bar"),
          },
          secretFileWriter: (_, value) => {
            writtenValue = value;
            return Promise.resolve();
          },
        });

        await sut.prepareSecrets(
          buildServiceDefinition({
            secrets: [
              {
                name: "dummy",
                mountPath: "dummy",
              },
            ],
          })
        );

        assert.equal(writtenValue, "foo bar");
      });

      it("writes secret to expected host file", async function () {
        let fileLocation = undefined;

        const sut = buildSut({
          localSecretsDirPath: "local-secrets-dir",
          secretFileWriter: (value, _) => {
            fileLocation = value;
            return Promise.resolve();
          },
        });

        await sut.prepareSecrets(
          buildServiceDefinition({
            name: "foo-service-name",
            secrets: [
              {
                name: "foo-secret",
                mountPath: "dummy",
              },
            ],
          })
        );

        assert.equal(
          fileLocation,
          path.join("local-secrets-dir", "foo-service-name", "foo-secret")
        );
      });
    });
  });
});
