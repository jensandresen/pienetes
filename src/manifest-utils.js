import yaml from "yaml";

function transformPorts(ports) {
  return (ports || []).map((portDefinition) => {
    const [host, container] = portDefinition
      .replace('"', "")
      .replace("'", "")
      .split(":")
      .map((port) => parseInt(port.trim()));

    return { host, container };
  });
}

export function manifestDeserializer(text) {
  return new Promise((resolve) => {
    const manifest = yaml.parse(text);

    if (manifest.service.ports) {
      manifest.service.ports = transformPorts(manifest.service.ports);
    }

    resolve(manifest);
  });
}

export function manifestSerializer(manifest) {
  return new Promise((resolve) => {
    const copy = { ...manifest };

    if (manifest.service.ports) {
      manifest.service.ports = manifest.service.ports.map(
        (map) => `${map.host}:${map.container}`
      );
    }

    resolve(yaml.stringify(copy));
  });
}
