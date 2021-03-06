function objectAsArray(obj) {
  const result = [];
  for (let key in obj) {
    result.push({ key: key, value: obj[key] });
  }
  return result;
}

export default class ContainerService {
  constructor(options) {
    const { cmdRunner, secretService } = options;

    if (!cmdRunner) {
      throw Error('Error! Missing required argument "cmdRunner".');
    }

    if (!secretService) {
      throw Error('Error! Missing required argument "secretService".');
    }

    this.cmdRunner = cmdRunner;
    this.secretService = secretService;

    this.startContainer = this.startContainer.bind(this);
    this.stopContainer = this.stopContainer.bind(this);
    this.renameContainer = this.renameContainer.bind(this);
    this.removeContainer = this.removeContainer.bind(this);
    this.createContainer = this.createContainer.bind(this);
    this.pullContainer = this.pullContainer.bind(this);
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

  async pullContainer(containerImage) {
    await this.cmdRunner(`docker pull ${containerImage}`);
  }

  async createContainer(manifest) {
    const args = [
      "docker",
      "create",
      "--restart unless-stopped",
      "--network pienetes-net",
      `--name "${manifest.name}"`,
    ];

    // port mappings
    (manifest.ports || []).forEach((portMap) => {
      args.push(`-p ${portMap.host}:${portMap.container}`);
    });

    // environment variables
    objectAsArray(manifest.environmentVariables || {}).forEach((mapping) => {
      args.push(`-e ${mapping.key}="${mapping.value}"`);
    });

    // add secrets logic here!
    const secrets = await this.secretService.prepareSecrets(manifest);
    (secrets || []).forEach(({ hostFile, containerFile }) => {
      args.push(`-v ${hostFile}:${containerFile}`);
    });

    // **** this MUST be the last one ****
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
