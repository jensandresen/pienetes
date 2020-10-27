import { readFileSync } from "fs";
import { resolve } from "path";

const splashContent = readFileSync(resolve("./splash.txt"), {
  encoding: "ascii",
});

export function printSplash() {
  console.log(splashContent);
}
