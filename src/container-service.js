function objectAsArray(obj) {
  const result = [];
  for (let key in obj) {
    result.push({ key: key, value: obj[key] });
  }
  return result;
}

export default class ContainerService {
  constructor(options) {
    const { cmdRunner } = options;
    this.cmdRunner = cmdRunner;

    this.startContainer = this.startContainer.bind(this);
    this.stopContainer = this.stopContainer.bind(this);
    this.renameContainer = this.renameContainer.bind(this);
    this.removeContainer = this.removeContainer.bind(this);
    this.createContainer = this.createContainer.bind(this);
    this.getRunningContainers = this.getRunningContainers.bind(this);
  }

  async startContainer(containerName) {
    await this.cmdRunner(`docker start ${containerName}`);
  }

  async stopContainer(containerName) {
    await this.cmdRunner(`docker stop ${containerName}`);
  }

  async renameContainer(currentContainerName, newContainerName) {
    await this.cmdRunner(
      `docker rename ${currentContainerName} ${newContainerName}`
    );
  }

  async removeContainer(containerName) {
    await this.cmdRunner(`docker container rm ${containerName}`);
  }

  async createContainer(manifest) {
    const args = [
      "docker",
      "create",
      "--restart unless-stopped",
      `--name "${manifest.name}"`,
    ];

    (manifest.ports || []).forEach((portMap) => {
      args.push(`-p ${portMap.host}:${portMap.container}`);
    });

    objectAsArray(manifest.environmentVariables || {}).forEach((mapping) => {
      args.push(`-e ${mapping.key}="${mapping.value}"`);
    });

    args.push(manifest.image);

    const createCommand = args.join(" ");
    await this.cmdRunner(createCommand);
  }

  async getRunningContainers() {
    const result = await this.cmdRunner(
      `docker ps --format '{{.ID}}|{{.Names}}|{{.Image}}|{{.Ports}}'`
    );

    if (result === "") {
      return [];
    }

    return result
      .trim()
      .split("\n")
      .map((line) => {
        const [id, name, image, ports] = line.split("|").map((x) => x.trim());
        const portMappings = (ports || "")
          .trim()
          .split(",")
          .map((map) => map.trim())
          .map((map) =>
            map.match(/.*?:(?<host>\d+)\s*->\s*(?<container>\d+).*?/)
          )
          .filter((match) => match != null && match.groups)
          .map((match) => {
            return {
              host: parseInt(match.groups.host),
              container: parseInt(match.groups.container),
            };
          });

        return { id, name, image, portMappings };
      });
  }
}
