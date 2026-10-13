import { copyFile, mkdir, rm } from 'node:fs/promises';

const root = new URL('../', import.meta.url);
const output = new URL('dist/', root);
await rm(output, { recursive: true, force: true });
await mkdir(new URL('src/', output), { recursive: true });
for (const file of ['index.html', 'src/index.css', 'src/App.css', 'src/main.js']) {
  await copyFile(new URL(file, root), new URL(file, output));
}
console.log('Built static website in dist/');
