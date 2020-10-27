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
    const { service } = yaml.parse(text);

    const result = {
      ...service,
      ...{
        ports: transformPorts(service.ports),
      },
    };

    resolve(result);
  });
}
