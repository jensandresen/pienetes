import hash from "object-hash";

export default class ManifestService {
  constructor(options) {
    const { containerService, manifestRepository, logger } = options;

    if (!containerService) {
      throw new Error('Invalid argument value of "containerService".');
    }

    if (!manifestRepository) {
      throw new Error('Invalid argument value of "manifestRepository".');
    }

    this.containerService = containerService;
    this.manifestRepository = manifestRepository;
    this.logger = logger || (() => {});

    this.applyManifest = this.applyManifest.bind(this);
    this.hasPortConflictWithRunningContainers = this.hasPortConflictWithRunningContainers.bind(
      this
    );
    this.isAlreadyRunning = this.isAlreadyRunning.bind(this);
    this.handleUpdateExisting = this.handleUpdateExisting.bind(this);
    this.handleNewContainer = this.handleNewContainer.bind(this);
    this.extractServiceDefinition = this.extractServiceDefinition.bind(this);
    this.hasBeenApplied = this.hasBeenApplied.bind(this);
    this.storeManifest = this.storeManifest.bind(this);
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
    this.logger(`Creating a brand new container "${serviceDefinition.name}"`);

    this.logger(`Pulling new image "${serviceDefinition.image}"`);
    await this.containerService.pullContainer(serviceDefinition.image);

    try {
      this.logger("Removing any old stopped container");
      await this.containerService.removeContainer(serviceDefinition.name);
    } catch {}

    this.logger("Creating a new container from service definition");
    await this.containerService.createContainer(serviceDefinition);

    this.logger("Starting the newly created container");
    await this.containerService.startContainer(serviceDefinition.name);

    this.logger("Done!");
  }

  async handleUpdateExisting(serviceDefinition) {
    this.logger(`Updating existing countainer "${serviceDefinition.name}"`);

    this.logger(`Pulling new image "${serviceDefinition.image}"`);
    await this.containerService.pullContainer(serviceDefinition.image);

    this.logger("Giving running container a temporary name");
    const tempContainerName = serviceDefinition.name + "-old";
    await this.containerService.renameContainer(
      serviceDefinition.name,
      tempContainerName
    );

    this.logger("Creating a new container from service definition");
    await this.containerService.createContainer(serviceDefinition);

    this.logger("Stopping running container");
    await this.containerService.stopContainer(tempContainerName);

    this.logger("Starting the newly created container");
    await this.containerService.startContainer(serviceDefinition.name);

    this.logger("Removing the old stopped container");
    await this.containerService.removeContainer(tempContainerName);

    this.logger("Done!");
  }

  extractServiceDefinition(manifest) {
    return manifest.service;
  }

  async hasBeenApplied(manifest) {
    const manifestChecksum = hash(manifest);
    const existingManifest = await this.manifestRepository.findByChecksum(
      manifestChecksum
    );
    return !!existingManifest;
  }

  async storeManifest(serviceName, manifest) {
    const manifestChecksum = hash(manifest);
    await this.manifestRepository.storeManifest(
      serviceName,
      manifest,
      manifestChecksum
    );
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

    const hasBeenApplied = await this.hasBeenApplied(manifest);
    if (hasBeenApplied) {
      this.logger(
        `Manifest for "${serviceDefinition.name}" has already been applied."`
      );
      return;
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

    await this.storeManifest(serviceDefinition.name, manifest);
  }
}
