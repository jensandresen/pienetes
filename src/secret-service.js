import { writeFile, mkdir } from "fs/promises";
import { existsSync } from "fs";
import path from "path";

export async function secretFileWriter(filePath, value) {
  const dir = path.dirname(filePath);
  const exists = existsSync(dir);

  if (!exists) {
    await mkdir(dir, { recursive: true });
  }

  await writeFile(filePath, value, { encoding: "utf8" });
}

export default class SecretService {
  constructor(options) {
    const {
      secretRepository,
      secretFileWriter,
      localSecretsDirPath,
      hostSecretsDirPath,
      logger,
    } = options;

    if (!secretRepository) {
      throw Error('Error! Missing required argument "secretRepository".');
    }

    if (!secretFileWriter) {
      throw Error('Error! Missing required argument "secretFileWriter".');
    }

    if (!localSecretsDirPath || localSecretsDirPath === "") {
      throw Error('Error! Missing required argument "localSecretsDirPath".');
    }

    if (!hostSecretsDirPath || hostSecretsDirPath === "") {
      throw Error('Error! Missing required argument "hostSecretsDirPath".');
    }

    this.localSecretsDirPath = localSecretsDirPath;
    this.hostSecretsDirPath = hostSecretsDirPath;
    this.secretRepository = secretRepository;
    this.secretFileWriter = secretFileWriter;
    this.logger = logger || (() => {});

    this.prepareSecrets = this.prepareSecrets.bind(this);
  }

  async prepareSecrets(serviceDefinition) {
    const serviceName = serviceDefinition.name;
    const secrets = serviceDefinition.secrets || [];

    const result = [];

    if (secrets.length > 0) {
      this.logger("Preparing secrets...");
    }

    for (let item of secrets) {
      const { name, mountPath } = item;

      const localFile = path.join(this.localSecretsDirPath, serviceName, name);
      const hostFile = path.join(this.hostSecretsDirPath, serviceName, name);

      const secretValue = await this.secretRepository.readSecret(name);
      await this.secretFileWriter(localFile, secretValue);

      let mount = mountPath || "";
      if (mount.trim() === "") {
        mount = path.join("/", "run", "secrets", name);
      }

      result.push({
        hostFile: hostFile,
        containerFile: mount.trim(),
      });
    }

    return result;
  }
}
