import yaml from "yaml";

function transformPorts(ports) {
  return (ports || []).map((portDefinition) => {
    const [host, container] = portDefinition
      .replace('"', "")
      .split(":")
      .map((port) => parseInt(port.trim()));

    return { host, container };
  });
}

export function parseManifest(text) {
  return new Promise((resolve) => {
    const manifest = yaml.parse(text);

    if (manifest.service.ports) {
      manifest.service.ports = transformPorts(manifest.service.ports);
    }

    resolve(manifest);
  });
}
