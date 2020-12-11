import { run } from "./cmd-utils";
import {
  manifestSerializer as originalManifestSerializer,
  manifestDeserializer as originalManifestDeserializer,
} from "./manifest-utils";
import ContainerService from "./container-service";
import ManifestService from "./manifest-service";
import ManifestRepository, {
  databaseFactory,
  getConnectionString,
} from "./manifest-repository";
import SecretRepository from "./secret-repository";
import SecretService, { secretFileWriter } from "./secret-service";

let database = null;
let containerService = null;
let manifestRepository = null;
let secretRepository = null;
let secretService = null;
let manifestService = null;
const defaultLogger = (text) => console.log(text);
const manifestSerializer = {
  serialize: originalManifestSerializer,
  deserialize: originalManifestDeserializer,
};

async function init() {
  const connectionString = await getConnectionString();
  database = await databaseFactory(connectionString);

  manifestRepository = new ManifestRepository({
    db: database,
    serializer: manifestSerializer.serialize,
    deserializer: manifestSerializer.deserialize,
    logger: defaultLogger,
  });

  secretRepository = new SecretRepository({
    db: database,
    logger: defaultLogger,
  });

  secretService = new SecretService({
    secretRepository: secretRepository,
    secretFileWriter: secretFileWriter,
    localSecretsDirPath: process.env.LOCAL_SECRETS_DIR,
    hostSecretsDirPath: process.env.HOST_SECRETS_DIR,
    logger: defaultLogger,
  });

  containerService = new ContainerService({
    cmdRunner: run,
    secretService: secretService,
    logger: defaultLogger,
  });

  manifestService = new ManifestService({
    containerService: containerService,
    manifestRepository: manifestRepository,
    logger: defaultLogger,
  });
}

init();

export const getContainerService = () => containerService;
export const getManifestRepository = () => manifestRepository;
export const getManifestService = () => manifestService;
export const getManifestSerializer = () => manifestSerializer;
export const logger = defaultLogger;
export const getSecretRepository = () => secretRepository;
export const getSecretService = () => secretService;
