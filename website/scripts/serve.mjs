import { readFile } from 'node:fs/promises';
import { createServer } from 'node:http';

const preview = process.argv.includes('--preview');
const root = new URL(preview ? '../dist/' : '../', import.meta.url);
const files = new Map([
  ['/', ['index.html', 'text/html; charset=utf-8']],
  ['/index.html', ['index.html', 'text/html; charset=utf-8']],
  ['/src/index.css', ['src/index.css', 'text/css; charset=utf-8']],
  ['/src/App.css', ['src/App.css', 'text/css; charset=utf-8']],
  ['/src/main.js', ['src/main.js', 'text/javascript; charset=utf-8']],
]);
const port = Number(process.env.PORT ?? (preview ? 4173 : 5173));

createServer(async (request, response) => {
  if (!['GET', 'HEAD'].includes(request.method)) {
    response.writeHead(405, { Allow: 'GET, HEAD' }).end();
    return;
  }
  const file = files.get(request.url.split('?')[0]);
  if (!file) {
    response.writeHead(404).end('Not found');
    return;
  }
  try {
    const content = await readFile(new URL(file[0], root));
    response.writeHead(200, { 'Content-Type': file[1], 'Cache-Control': 'no-store' });
    response.end(request.method === 'HEAD' ? undefined : content);
  } catch (error) {
    response.writeHead(error.code === 'ENOENT' ? 404 : 500).end('Unable to read file');
  }
}).listen(port, '127.0.0.1', () => {
  console.log(`Website available at http://localhost:${port}`);
});
