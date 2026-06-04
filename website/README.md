# Whisper.net Presentation Website

A static single-page presentation website for the [Whisper.net](https://github.com/sandrohanea/whisper.net) project. Built with React + Vite.

## What's in this folder

- `src/App.jsx` — Main component with hero section, feature cards, code example, runtimes table, and CTA sections
- `src/App.css` — All component styles (dark theme, cards, waveform animation, syntax highlighting)
- `src/index.css` — Global CSS reset and custom properties (color scheme)
- `index.html` — Entry point with Google Fonts (Inter) and page metadata

## Getting started

### Prerequisites

- [Node.js](https://nodejs.org/) (v18 or later)

### Install dependencies

```bash
cd website
npm install
```

### Run the development server

```bash
npm run dev
```

The site will be available at [http://localhost:5173](http://localhost:5173).

### Build for production

```bash
npm run build
```

The output will be in the `dist/` folder, ready to deploy as a static site.
