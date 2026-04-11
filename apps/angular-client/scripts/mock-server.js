const http = require('http');
const url = require('url');
const fs = require('fs');

const dataPath = __dirname + '/../mockoon/movie-api.json';
const raw = fs.readFileSync(dataPath, 'utf8');
const env = JSON.parse(raw).data;

const port = env.port || 3001;

const movies = [
  { id: 1, title: 'The Matrix' },
  { id: 2, title: 'Inception' }
];

const server = http.createServer((req, res) => {
  const parsed = url.parse(req.url, true);
  res.setHeader('Content-Type', 'application/json');

  if (req.method === 'GET' && parsed.pathname === '/movies') {
    res.writeHead(200);
    res.end(JSON.stringify(movies));
    return;
  }

  const movieIdMatch = parsed.pathname.match(/^\/movies\/(\d+)$/);
  if (req.method === 'GET' && movieIdMatch) {
    const id = Number(movieIdMatch[1]);
    const movie = movies.find((m) => m.id === id) || null;
    if (movie) {
      res.writeHead(200);
      res.end(JSON.stringify(movie));
    } else {
      res.writeHead(404);
      res.end(JSON.stringify({ message: 'Not found' }));
    }
    return;
  }

  if (req.method === 'POST' && parsed.pathname === '/movies') {
    let body = '';
    req.on('data', (chunk) => (body += chunk));
    req.on('end', () => {
      try {
        const parsedBody = JSON.parse(body || '{}');
        const newMovie = { id: movies.length + 1, title: parsedBody.title || 'New Movie' };
        movies.push(newMovie);
        res.writeHead(201);
        res.end(JSON.stringify(newMovie));
      } catch (e) {
        res.writeHead(400);
        res.end(JSON.stringify({ message: 'Invalid JSON' }));
      }
    });
    return;
  }

  res.writeHead(404);
  res.end(JSON.stringify({ message: 'Not found' }));
});

server.listen(port, () => {
  console.log(`Mock server listening on http://localhost:${port}`);
});
