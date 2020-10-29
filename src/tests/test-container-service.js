import mocha from "mocha";
import { assert } from "chai";
import ContainerService from "../container-service";

const dummyCallback = () => {};
const buildSut = (overrides) => {
  const defaults = {
    cmdRunner: dummyCallback,
  };
  return new ContainerService({ ...defaults, ...overrides });
};

describe("container-service", async function () {
  let actualCommand = "";
  const sut = buildSut({
    cmdRunner: (cmd) => (actualCommand = cmd),
  });

  describe("simple commands", async function () {
    it("runs expected start command", async function () {
      await sut.startContainer("foo");
      assert.equal(actualCommand, "docker start foo");
    });

    it("runs expected stop command", async function () {
      await sut.stopContainer("foo");
      assert.equal(actualCommand, "docker stop foo");
    });

    it("runs expected rename command", async function () {
      await sut.renameContainer("foo", "bar");
      assert.equal(actualCommand, "docker rename foo bar");
    });

    it("runs expected remove command", async function () {
      await sut.removeContainer("foo");
      assert.equal(actualCommand, "docker container rm foo");
    });

    it("runs expected pull command", async function () {
      await sut.pullContainer("foo");
      assert.equal(actualCommand, "docker pull foo");
    });
  });

  describe("create container", async function () {
    describe("on the simplest service definition", async function () {
      await sut.createContainer({
        name: "foo",
        image: "bar",
      });

      it("starts with expected command", function () {
        assert.isTrue(actualCommand.startsWith("docker create "));
      });

      it("ends with expected image name", function () {
        assert.isTrue(actualCommand.endsWith(" bar"));
      });

      it("sets expected name of container", function () {
        assert.include(actualCommand, '--name "foo"');
      });

      it("sets expected restart policy", function () {
        assert.include(actualCommand, "--restart unless-stopped");
      });
    });

    describe("on simple service definition with ports", async function () {
      it("sets expected port mapping for single port set", async function () {
        await sut.createContainer({
          name: "foo",
          image: "bar",
          ports: [{ host: 1, container: 2 }],
        });

        assert.include(actualCommand, "-p 1:2");
      });

      it("sets expected port mapping for multiple port sets", async function () {
        await sut.createContainer({
          name: "foo",
          image: "bar",
          ports: [
            { host: 1, container: 2 },
            { host: 3, container: 4 },
          ],
        });

        assert.include(actualCommand, "-p 1:2");
        assert.include(actualCommand, "-p 3:4");
      });
    });

    describe("on simple service definition with environment variables", async function () {
      it("sets expected environment variable when single is defined", async function () {
        await sut.createContainer({
          name: "dummy1",
          image: "dummy2",
          environmentVariables: {
            foo: "bar",
          },
        });

        assert.include(actualCommand, '-e foo="bar"');
      });

      it("sets expected environment variables when multiple is defined", async function () {
        await sut.createContainer({
          name: "dummy1",
          image: "dummy2",
          environmentVariables: {
            foo: "bar",
            baz: "qux",
          },
        });

        assert.include(actualCommand, '-e foo="bar"');
        assert.include(actualCommand, '-e baz="qux"');
      });
    });
  });

  describe("getRunningContainers", async function () {
    const buildSutWithOutput = (output) =>
      buildSut({
        cmdRunner: (cmd) => Promise.resolve(output),
      });

    it("returns expected when no containers are running", async function () {
      const sut = buildSutWithOutput("");
      const result = await sut.getRunningContainers();
      assert.isEmpty(result);
    });

    it("returns expected count when single container is running", async function () {
      const sut = buildSutWithOutput("foo");
      const result = await sut.getRunningContainers();
      assert.lengthOf(result, 1);
    });

    it("returns expected id when single containers is running", async function () {
      const sut = buildSutWithOutput("foo");
      const result = await sut.getRunningContainers();
      assert.equal(result[0].id, "foo");
    });

    it("returns expected name when single containers is running", async function () {
      const sut = buildSutWithOutput("foo|bar");
      const result = await sut.getRunningContainers();
      assert.equal(result[0].name, "bar");
    });

    it("returns expected image when single containers is running", async function () {
      const sut = buildSutWithOutput("foo|bar|baz");
      const result = await sut.getRunningContainers();
      assert.equal(result[0].image, "baz");
    });

    it("returns expected count when multiple containers are running", async function () {
      const sut = buildSutWithOutput("foo\nbar\nbaz\nqux");
      const result = await sut.getRunningContainers();
      assert.lengthOf(result, 4);
    });

    it("returns expected when multiple containers are running", async function () {
      const sut = buildSutWithOutput(
        "foo1|foo2|foo3\nbar1|bar2|bar3\nbaz1|baz2|baz3\nqux1|qux2|qux3"
      );

      const result = await sut.getRunningContainers();

      assert.deepEqual(result, [
        { id: "foo1", name: "foo2", image: "foo3", portMappings: [] },
        { id: "bar1", name: "bar2", image: "bar3", portMappings: [] },
        { id: "baz1", name: "baz2", image: "baz3", portMappings: [] },
        { id: "qux1", name: "qux2", image: "qux3", portMappings: [] },
      ]);
    });

    it("it handles trailing empty lines in output from runner", async function () {
      const sut = buildSutWithOutput("foo\nbar\nbaz\nqux\n");
      const result = await sut.getRunningContainers();
      assert.lengthOf(result, 4);
    });

    it("returns expected port mapping when single container is running with a single mapping", async function () {
      const sut = buildSutWithOutput("dummy|dummy|dummy|host-ip:8080->80/tcp");
      const result = await sut.getRunningContainers();

      assert.deepEqual(result[0].portMappings, [
        {
          host: 8080,
          container: 80,
        },
      ]);
    });

    it("returns expected port mapping when single container is running with multiple mappings", async function () {
      const sut = buildSutWithOutput(
        "dummy|dummy|dummy|host-ip:8080->80/tcp, host-ip:9090->90/tcp, "
      );
      const result = await sut.getRunningContainers();

      assert.deepEqual(result[0].portMappings, [
        {
          host: 8080,
          container: 80,
        },
        {
          host: 9090,
          container: 90,
        },
      ]);
    });
  });
});
