import { useState } from 'react'
import './App.css'

function CopyButton({ text }) {
  const [copied, setCopied] = useState(false)

  const handleCopy = () => {
    navigator.clipboard.writeText(text)
    setCopied(true)
    setTimeout(() => setCopied(false), 2000)
  }

  return (
    <button className="copy-btn" onClick={handleCopy} title="Copy to clipboard">
      {copied ? '✓' : '⧉'}
    </button>
  )
}

const cards = [
  {
    tag: 'Features',
    title: 'Built for speed',
    items: [
      { icon: '🎙️', label: 'Speech Recognition', desc: 'OpenAI Whisper via whisper.cpp — 99+ languages.' },
      { icon: '⚡', label: 'Native Performance', desc: 'C/C++ inference with CUDA, CoreML, Vulkan, OpenVINO.' },
      { icon: '🔄', label: 'Async Streaming', desc: 'IAsyncEnumerable for real-time transcription results.' },
    ],
  },
  {
    tag: 'Runtimes',
    title: 'Pick your hardware',
    items: [
      { icon: '🖥️', label: 'CPU', desc: 'All platforms — Win, Linux, macOS, Mobile, WASM.' },
      { icon: '🎮', label: 'CUDA 12 / 13', desc: 'NVIDIA GPU acceleration — Win & Linux x64.' },
      { icon: '🍎', label: 'CoreML', desc: 'Apple Neural Engine — macOS, iOS, MacCatalyst.' },
      { icon: '🔷', label: 'OpenVINO', desc: 'Intel acceleration — Win & Linux x64.' },
      { icon: '🌋', label: 'Vulkan', desc: 'GPU via Vulkan API — Win x64.' },
    ],
  },
  {
    tag: 'Platforms',
    title: 'Runs everywhere',
    chips: ['Windows', 'Linux', 'macOS', 'Android', 'iOS', 'MacCatalyst', 'tvOS', 'WebAssembly'],
  },
  {
    tag: 'Usage',
    title: 'A few lines of C#',
    code: true,
  },
]

function App() {
  return (
    <>
      {/* ── Hero ── */}
      <section className="hero">
        <div className="hero-glow" />
        <div className="hero-content">
          <h1>
            <span className="hero-top">Whisper</span>
            <span className="hero-dot">.net</span>
          </h1>
          <p className="hero-sub">
            Open-source speech-to-text for .NET<br />
            powered by whisper.cpp
          </p>

          <div className="hero-actions">
            <a
              href="https://github.com/sandrohanea/whisper.net"
              target="_blank"
              rel="noopener noreferrer"
              className="btn-star"
            >
              ⭐ Star on GitHub
            </a>
            <a
              href="https://www.nuget.org/packages/Whisper.net"
              target="_blank"
              rel="noopener noreferrer"
              className="btn-nuget"
            >
              📦 NuGet
            </a>
          </div>

          <div className="install-bar">
            <code>dotnet add package Whisper.net.AllRuntimes</code>
            <CopyButton text="dotnet add package Whisper.net.AllRuntimes" />
          </div>
        </div>

        <div className="scroll-hint">↓</div>
      </section>

      {/* ── Cards ── */}
      <main className="cards">
        {cards.map((card) => (
          <section key={card.tag} className="card">
            <span className="card-tag">{card.tag}</span>
            <h2>{card.title}</h2>

            {card.items && (
              <div className="card-items">
                {card.items.map((it) => (
                  <div key={it.label} className="card-item">
                    <span className="card-item-icon">{it.icon}</span>
                    <div>
                      <strong>{it.label}</strong>
                      <p>{it.desc}</p>
                    </div>
                  </div>
                ))}
              </div>
            )}

            {card.chips && (
              <div className="chip-wrap">
                {card.chips.map((c) => (
                  <span key={c} className="chip">{c}</span>
                ))}
              </div>
            )}

            {card.code && (
              <div className="code-block">
                <div className="code-dots">
                  <span /><span /><span />
                  <span className="code-file">Program.cs</span>
                </div>
                <pre>{
`using var factory = WhisperFactory.FromPath("ggml-base.bin");

using var processor = factory.CreateBuilder()
    .WithLanguage("auto")
    .Build();

using var stream = File.OpenRead("audio.wav");

await foreach (var r in processor.ProcessAsync(stream))
{
    Console.WriteLine($"{r.Start}->{r.End}: {r.Text}");
}`
                }</pre>
              </div>
            )}
          </section>
        ))}
      </main>

      {/* ── Footer ── */}
      <footer className="footer">
        <div className="footer-inner">
          <p>
            Built with ❤️ by the Whisper.net community · Powered by{' '}
            <a href="https://github.com/ggerganov/whisper.cpp" target="_blank" rel="noopener noreferrer">
              whisper.cpp
            </a>
          </p>
          <p className="footer-license">
            Released under the{' '}
            <a href="https://github.com/sandrohanea/whisper.net/blob/main/LICENSE" target="_blank" rel="noopener noreferrer">
              MIT License
            </a>
          </p>
        </div>
      </footer>
    </>
  )
}

export default App
