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

function transformSecrets(secrets) {
  if (!secrets) {
    return undefined;
  }

  return (secrets || []).map((secret) => {
    const [name, mountPath] = secret
      .replace('"', "")
      .replace("'", "")
      .split(":")
      .map((x) => x.trim());

    return {
      name: name,
      mountPath: mountPath,
    };
  });
}

export function manifestDeserializer(text) {
  return new Promise((resolve) => {
    const manifest = yaml.parse(text);

    if (manifest.service.ports) {
      manifest.service.ports = transformPorts(manifest.service.ports);
    }

    manifest.service.secrets = transformSecrets(manifest.service.secrets);

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
