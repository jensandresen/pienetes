import express, { json, text } from "express";
import cors from "cors";
import { printSplash } from "./splash-screen";
import {
  manifestService,
  manifestParser,
  containerService,
} from "./service-registry";

const app = express();
app.use(cors());
app.use(json());
app.use(text({ type: "application/yaml" }));
app.use(text({ type: "text/yaml" }));
app.use(text({ type: "text/plain" }));

// ROUTES ---------------------------------------------------------------------

app.get("/api/containers", async (req, res) => {
  const containers = await containerService.getRunningContainers();
  res.send(containers);
});

app.post("/api/applymanifest", async (req, res) => {
  const manifest = await manifestParser.parse(req.body);
  await manifestService.applyManifest(manifest);

  res.status(201).send({ message: "OK" });
});

// ----------------------------------------------------------------------------

const port = process.env.PORT || 3000;
app.listen(port, () => {
  printSplash();
});
