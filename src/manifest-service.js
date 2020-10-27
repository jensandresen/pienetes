export default class ManifestService {
  constructor(options) {
    const { containerService } = options;

    if (!containerService) {
      throw new Error('Invalid argument value of "containerService".');
    }

    this.containerService = containerService;

    this.applyManifest = this.applyManifest.bind(this);
    this.hasPortConflictWithRunningContainers = this.hasPortConflictWithRunningContainers.bind(
      this
    );
    this.isAlreadyRunning = this.isAlreadyRunning.bind(this);
    this.handleUpdateExisting = this.handleUpdateExisting.bind(this);
    this.handleNewContainer = this.handleNewContainer.bind(this);
  }

  async hasPortConflictWithRunningContainers(manifest) {
    const manifestHostPorts = (manifest.ports || []).map((x) => x.host);

    const hostPortsInUse = [];
    const runningContainers = await this.containerService.getRunningContainers();

    runningContainers
      .filter((container) => container.name != manifest.name)
      .forEach((container) => {
        (container.portMappings || []).forEach((map) =>
          hostPortsInUse.push(map.host)
        );
      });

    const conflictingPorts = manifestHostPorts.filter((port) =>
      hostPortsInUse.includes(port)
    );

    return conflictingPorts.length > 0;
  }

  async isAlreadyRunning(containerName) {
    const containers = await this.containerService.getRunningContainers();
    return !!containers.find((x) => x.name == containerName);
  }

  async handleNewContainer(manifest) {
    await this.containerService.createContainer(manifest);
    await this.containerService.startContainer(manifest.name);
  }

  async handleUpdateExisting(manifest) {
    const tempContainerName = manifest.name + "-old";
    await this.containerService.renameContainer(
      manifest.name,
      tempContainerName
    );
    await this.containerService.createContainer(manifest);
    await this.containerService.stopContainer(tempContainerName);
    await this.containerService.startContainer(manifest.name);
    await this.containerService.removeContainer(tempContainerName);
  }

  async applyManifest(manifest) {
    if (!manifest) {
      throw Error("Invalid manifest. It has not been initialized.");
    }

    if (!manifest.name) {
      throw Error("Invalid manifest. Missing required name.");
    }

    if (!manifest.image) {
      throw Error("Invalid manifest. Missing required image.");
    }

    if (await this.hasPortConflictWithRunningContainers(manifest)) {
      throw Error(
        `Invalid port mapping! Manifest has port mapping that is already in use by another container on the host.`
      );
    }

    const isAlreadyRunning = await this.isAlreadyRunning(manifest.name);

    if (isAlreadyRunning) {
      await this.handleUpdateExisting(manifest);
    } else {
      await this.handleNewContainer(manifest);
    }
  }
}
