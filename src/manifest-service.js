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
    this.extractServiceDefinition = this.extractServiceDefinition.bind(this);
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

  async handleNewContainer(serviceDefinition) {
    console.log(`Creating a brand new container "${serviceDefinition.name}"`);

    console.log(`Pulling new image "${serviceDefinition.image}"`);
    await this.containerService.pullContainer(serviceDefinition.image);

    console.log("Creating a new container from service definition");
    await this.containerService.createContainer(serviceDefinition);

    console.log("Starting the newly created container");
    await this.containerService.startContainer(serviceDefinition.name);

    console.log("Done!");
  }

  async handleUpdateExisting(serviceDefinition) {
    console.log(`Updating existing countainer "${serviceDefinition.name}"`);

    console.log(`Pulling new image "${serviceDefinition.image}"`);
    await this.containerService.pullContainer(serviceDefinition.image);

    console.log("Giving running container a temporary name");
    const tempContainerName = serviceDefinition.name + "-old";
    await this.containerService.renameContainer(
      serviceDefinition.name,
      tempContainerName
    );

    console.log("Creating a new container from service definition");
    await this.containerService.createContainer(serviceDefinition);

    console.log("Stopping running container");
    await this.containerService.stopContainer(tempContainerName);

    console.log("Starting the newly created container");
    await this.containerService.startContainer(serviceDefinition.name);

    console.log("Removing the old stopped container");
    await this.containerService.removeContainer(tempContainerName);

    console.log("Done!");
  }

  extractServiceDefinition(manifest) {
    return manifest.service;
  }

  async applyManifest(manifest) {
    if (!manifest) {
      throw Error("Invalid manifest. It has not been initialized.");
    }

    const serviceDefinition = this.extractServiceDefinition(manifest);
    if (!serviceDefinition) {
      throw Error("Invalid manifest. Missing required service definition.");
    }

    if (!serviceDefinition.name) {
      throw Error("Invalid manifest. Missing required name.");
    }

    if (!serviceDefinition.image) {
      throw Error("Invalid manifest. Missing required image.");
    }

    if (await this.hasPortConflictWithRunningContainers(serviceDefinition)) {
      throw Error(
        `Invalid port mapping! Manifest has port mapping that is already in use by another container on the host.`
      );
    }

    const isAlreadyRunning = await this.isAlreadyRunning(
      serviceDefinition.name
    );

    if (isAlreadyRunning) {
      await this.handleUpdateExisting(serviceDefinition);
    } else {
      await this.handleNewContainer(serviceDefinition);
    }
  }
}
