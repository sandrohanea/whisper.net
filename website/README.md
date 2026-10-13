# Whisper.net Presentation Website

A static single-page presentation website for the [Whisper.net](https://github.com/sandrohanea/whisper.net) project. Uses HTML, CSS, and browser JavaScript with no npm dependencies.

## Files

- `index.html` — Page content, waveform bars, runtime cards, and code example
- `src/App.css` — Component styles, waveform animations, and syntax highlighting
- `src/index.css` — Global reset and custom properties
- `src/main.js` — Clipboard buttons and animated scrolling
- `scripts/` — Build and local server using only Node.js built-in modules

## Local development

Use Node.js 24, matching the Pages workflow. No dependency installation is needed.

```bash
cd website
npm run dev
```

Open [http://localhost:5173](http://localhost:5173). Reload the browser after editing files.

`npm run lint` checks JavaScript syntax using Node.js; it does not run ESLint.

## Production build

```bash
npm run build
npm run preview
```

The build copies the static assets into `dist/`. Preview serves them at [http://localhost:4173](http://localhost:4173).

`npm run build:pages` produces the same output. Relative asset URLs support both a domain root and a GitHub Pages repository subpath. The Pages workflow builds without an npm install step and deploys `dist/`.

Keep the site dependency-free: edit the HTML, CSS, and JavaScript directly instead of adding a bundler or framework for this presentation page.
