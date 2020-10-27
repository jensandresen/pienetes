import { run } from "./cmd-utils";
import { parseManifest } from "./manifest-utils";
import ContainerService from "./container-service";
import ManifestService from "./manifest-service";

export const manifestParser = { parse: parseManifest };
export const containerService = new ContainerService({ cmdRunner: run });
export const manifestService = new ManifestService({
  containerService: containerService,
});
