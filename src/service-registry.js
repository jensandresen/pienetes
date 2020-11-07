import { run } from "./cmd-utils";
import {
  manifestSerializer as originalManifestSerializer,
  manifestDeserializer as originalManifestDeserializer,
} from "./manifest-utils";
import ContainerService from "./container-service";
import ManifestService from "./manifest-service";
import ManifestRepository, {
  fileBasedDatabaseFactory,
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
  containerService = new ContainerService({ cmdRunner: run });
  database = await fileBasedDatabaseFactory(process.env.DATABASE_FILE_PATH);

  manifestRepository = new ManifestRepository({
    db: database,
    serializer: manifestSerializer.serialize,
    deserializer: manifestSerializer.deserialize,
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
