import express, { json, text } from "express";
import cors from "cors";
import { printSplash } from "./splash-screen";
import {
  getManifestService,
  getManifestSerializer,
  getContainerService,
  getManifestRepository,
  getSecretRepository,
  logger,
} from "./service-registry";

const app = express();
app.use(cors());
app.use(json());
app.use(text({ type: "application/yaml" }));
app.use(text({ type: "text/yaml" }));
app.use(text({ type: "text/plain" }));

// ROUTES ---------------------------------------------------------------------

app.get("/api/containers", async (req, res) => {
  const containers = await getContainerService().getRunningContainers();
  res.send(containers);
});

app.get("/api/manifests", async (req, res) => {
  const manifests = await getManifestRepository().getAll();

  const list = [];
  for (let m of manifests) {
    const manifestYaml = await getManifestSerializer().serialize(m);
    list.push(manifestYaml);
  }

  const result = list.join("\n---\n\n");

  res.set("Content-Type", "text/yaml").send(result);
});

app.get("/api/manifests/:serviceName", async (req, res) => {
  const serviceName = req.params.serviceName || "";
  const manifest = await getManifestRepository().getByName(serviceName);

  if (!manifest) {
    res.status(404).send({
      message: `Unable to find a manifest with service name "${serviceName}".`,
    });
  } else {
    const result = await getManifestSerializer().serialize(manifest);
    res.set("Content-Type", "text/yaml").send(result);
  }
});

app.post("/api/applymanifest", async (req, res) => {
  const manifest = await getManifestSerializer().deserialize(req.body);
  getManifestService().applyManifest(manifest);

  res.status(202).send();
});

app.get("/api/secrets/:name", async (req, res) => {
  const name = req.params.name || "";
  const secret = await getSecretRepository().readSecret(name);

  if (!secret) {
    res.status(404).send({
      message: `Unable to find secret with name "${name}".`,
    });
  } else {
    res.send(secret);
  }
});

app.put("/api/secrets/:name", async (req, res) => {
  const name = req.params.name || "";
  const value = req.body;

  try {
    await getSecretRepository().writeSecret(name, value);
    res.status(204).send();
  } catch (err) {
    logger(err);
    res.status(500).send(err.toString());
  }
});

// ----------------------------------------------------------------------------

const port = process.env.PORT || 3000;
app.listen(port, () => {
  printSplash();
});
