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

let database = null;
let containerService = null;
let manifestRepository = null;
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

  containerService = new ContainerService({ cmdRunner: run });

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
