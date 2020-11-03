import mocha from "mocha";
import chai from "chai";
import ManifestService from "../manifest-service";

import chaiAsPromised from "chai-as-promised";
chai.use(chaiAsPromised);

const { assert, expect } = chai;

describe("manifest-service", async function () {
  const dummyCallback = () => {};
  const buildContainerService = (overrides) => {
    const defaults = {
      createContainer: dummyCallback,
      startContainer: dummyCallback,
      stopContainer: dummyCallback,
      renameContainer: dummyCallback,
      removeContainer: dummyCallback,
      pullContainer: dummyCallback,
      getRunningContainers: () => [],
    };
    return { ...defaults, ...overrides };
  };
  const buildSut = (overrides) => {
    const defaults = { containerService: buildContainerService() };
    return new ManifestService({ ...defaults, ...overrides });
  };

  const buildManifest = (overrides) => {
    const defaults = {
      name: "foo-name",
      image: "foo-image:latest",
      ports: [{ host: 1, container: 2 }],
    };
    return {
      version: 1,
      service: { ...defaults, ...overrides },
    };
  };

  describe("ctor", function () {
    it("throws error if containerService is undefined", function () {
      assert.throws(
        () => buildSut({ containerService: undefined }),
        /.*?containerService.*?/
      );
    });

    it("throws error if containerService is null", function () {
      assert.throws(
        () => buildSut({ containerService: null }),
        /.*?containerService.*?/
      );
    });
  });

  describe("applyManifest", async function () {
    describe("handle invalid manifest", async function () {
      it("throws error when manifest is undefined", async function () {
        const sut = buildSut();
        await expect(sut.applyManifest(undefined)).to.be.rejectedWith(
          /.*?Invalid manifest.*?/gi
        );
      });

      it("throws error when manifest is null", async function () {
        const sut = buildSut();
        await expect(sut.applyManifest(null)).to.be.rejectedWith(
          /.*?Invalid manifest.*?/gi
        );
      });

      it("throws error when manifest is missing required name", async function () {
        const sut = buildSut();
        const stubManifest = buildManifest({ name: null });
        await expect(sut.applyManifest(stubManifest)).to.be.rejectedWith(
          /.*?Invalid manifest.*?[Mm]issing required name.*?/gi
        );
      });

      it("throws error when manifest is missing required image", async function () {
        const sut = buildSut();
        const stubManifest = buildManifest({ image: null });
        await expect(sut.applyManifest(stubManifest)).to.be.rejectedWith(
          /.*?Invalid manifest.*?[Mm]issing required image.*?/gi
        );
      });
    });

    describe("creates a container", async function () {
      it("runs expected create", async function () {
        let wasCalled = false;
        const spy = buildContainerService({
          createContainer: () => (wasCalled = true),
        });

        const sut = buildSut({
          containerService: spy,
        });

        await sut.applyManifest(buildManifest());
        assert.isTrue(wasCalled);
      });

      it("throws error if port is already in use", async function () {
        const stubRunningContainers = [
          {
            portMappings: [
              {
                host: 8080,
              },
            ],
          },
        ];

        const spy = buildContainerService({
          getRunningContainers: () => stubRunningContainers,
        });

        const sut = buildSut({
          containerService: spy,
        });

        await expect(
          sut.applyManifest(buildManifest({ ports: [{ host: 8080 }] }))
        ).to.be.rejectedWith(/.*?[Pp]ort.*?already in use.*?/gi);
      });
    });

    describe("pulls container image", async function () {
      it("runs expected pull command", async function () {
        let wasCalled = false;
        const spy = buildContainerService({
          pullContainer: () => (wasCalled = true),
        });

        const sut = buildSut({
          containerService: spy,
        });

        await sut.applyManifest(buildManifest());
        assert.isTrue(wasCalled);
      });
    });

    describe("starts a container", async function () {
      it("runs expected start command", async function () {
        let wasCalled = false;
        const spy = buildContainerService({
          startContainer: () => (wasCalled = true),
        });

        const sut = buildSut({
          containerService: spy,
        });

        await sut.applyManifest(buildManifest());
        assert.isTrue(wasCalled);
      });
    });

    describe("when a container is already running", async function () {
      describe("pulls container image", async function () {
        it("runs expected pull command", async function () {
          let wasCalled = false;
          const spy = buildContainerService({
            pullContainer: () => (wasCalled = true),
          });

          const sut = buildSut({
            containerService: spy,
          });

          await sut.applyManifest(buildManifest());
          assert.isTrue(wasCalled);
        });
      });

      it("renames the running container", async function () {
        const stubRunningContainer = { name: "foo" };

        let wasCalled = false;
        const spy = buildContainerService({
          renameContainer: () => (wasCalled = true),
          getRunningContainers: () => [stubRunningContainer],
        });

        const sut = buildSut({
          containerService: spy,
        });

        await sut.applyManifest(
          buildManifest({ name: stubRunningContainer.name })
        );

        assert.isTrue(wasCalled);
      });

      it("creates a new the container", async function () {
        const stubRunningContainer = { name: "foo" };

        let wasCalled = false;
        const spy = buildContainerService({
          createContainer: () => (wasCalled = true),
          getRunningContainers: () => [stubRunningContainer],
        });

        const sut = buildSut({
          containerService: spy,
        });

        await sut.applyManifest(
          buildManifest({ name: stubRunningContainer.name })
        );

        assert.isTrue(wasCalled);
      });

      it("stops the old container", async function () {
        const stubRunningContainer = { name: "foo" };

        let wasCalled = false;
        const spy = buildContainerService({
          stopContainer: () => (wasCalled = true),
          getRunningContainers: () => [stubRunningContainer],
        });

        const sut = buildSut({
          containerService: spy,
        });

        await sut.applyManifest(
          buildManifest({ name: stubRunningContainer.name })
        );

        assert.isTrue(wasCalled);
      });

      it("removes the old container", async function () {
        const stubRunningContainer = { name: "foo" };

        let wasCalled = false;
        const spy = buildContainerService({
          removeContainer: () => (wasCalled = true),
          getRunningContainers: () => [stubRunningContainer],
        });

        const sut = buildSut({
          containerService: spy,
        });

        await sut.applyManifest(
          buildManifest({ name: stubRunningContainer.name })
        );

        assert.isTrue(wasCalled);
      });
    });
  });
});
