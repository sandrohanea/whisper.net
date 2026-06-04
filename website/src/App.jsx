import { useState, useRef } from 'react'
import './App.css'

function CopyButton({ text }) {
  const [copied, setCopied] = useState(false)
  const handleCopy = () => {
    navigator.clipboard.writeText(text).then(() => {
      setCopied(true)
      setTimeout(() => setCopied(false), 2000)
    }).catch(() => {
      // clipboard API unavailable or denied
    })
  }
  return (
    <button className="copy-btn" onClick={handleCopy} title="Copy to clipboard">
      {copied ? '✓ copied' : 'copy'}
    </button>
  )
}

function Waveform() {
  const bars = 72
  const durations = [1.8, 2.2, 2.6, 3.0, 3.4]
  const animations = ['pulse1', 'pulse2', 'pulse3']
  return (
    <div className="waveform" aria-hidden="true">
      {Array.from({ length: bars }).map((_, i) => {
        const anim = animations[i % animations.length]
        const dur = durations[(i * 7) % durations.length]
        const delay = ((i * 0.08) + (i % 3) * 0.15) % 2.5
        return (
          <div key={i} className="waveform-bar" style={{
            animationName: anim,
            animationDuration: `${dur}s`,
            animationDelay: `${delay}s`,
          }} />
        )
      })}
    </div>
  )
}

function App() {
  const contentRef = useRef(null)

  const scrollToContent = () => {
    const el = contentRef.current
    if (!el) return
    const target = el.offsetTop
    const start = window.scrollY
    const distance = target - start
    const duration = 800
    let startTime = null

    function step(timestamp) {
      if (!startTime) startTime = timestamp
      const progress = Math.min((timestamp - startTime) / duration, 1)
      const ease = progress < 0.5
        ? 4 * progress * progress * progress
        : 1 - Math.pow(-2 * progress + 2, 3) / 2
      window.scrollTo(0, start + distance * ease)
      if (progress < 1) requestAnimationFrame(step)
    }

    requestAnimationFrame(step)
  }

  return (
    <>
      {/* ── Hero ── */}
      <section className="hero">
        <div className="hero-body">
          <p className="hero-kicker">Open-source | .NET | whisper.cpp</p>
          <h1>
            Whisper<span className="accent">.net</span>
          </h1>
          <Waveform />
          <p className="hero-lead">
            Accurate, fast speech-to-text for .NET applications.
            <br />
            99+ languages. GPU-accelerated. Runs everywhere.
          </p>

          <div className="hero-cta-group">
            <div className="hero-row">
              <a href="https://github.com/sandrohanea/whisper.net" target="_blank" rel="noopener noreferrer" className="btn btn-gh">
              <svg className="icon-default" width="20" height="20" viewBox="0 0 24 24" fill="currentColor" aria-hidden="true">
                <path d="M12 .3a12 12 0 0 0-3.8 23.38c.6.1.83-.26.83-.57v-2.23c-3.34.73-4.04-1.42-4.04-1.42a3.18 3.18 0 0 0-1.34-1.75c-1.08-.74.08-.73.08-.73a2.53 2.53 0 0 1 1.84 1.24 2.56 2.56 0 0 0 3.5 1 2.56 2.56 0 0 1 .76-1.61c-2.67-.3-5.47-1.33-5.47-5.93a4.64 4.64 0 0 1 1.24-3.22 4.3 4.3 0 0 1 .12-3.18s1-.32 3.3 1.23a11.4 11.4 0 0 1 6.01 0c2.3-1.55 3.3-1.23 3.3-1.23a4.3 4.3 0 0 1 .12 3.18 4.64 4.64 0 0 1 1.24 3.22c0 4.61-2.8 5.63-5.48 5.92a2.87 2.87 0 0 1 .82 2.23v3.29c0 .31.22.68.83.57A12 12 0 0 0 12 .3"/>
              </svg>
              <svg className="icon-hover" width="20" height="20" viewBox="0 0 24 24" fill="currentColor" aria-hidden="true">
                <path d="M12 2l3.09 6.26L22 9.27l-5 4.87 1.18 6.88L12 17.77l-6.18 3.25L7 14.14 2 9.27l6.91-1.01L12 2z"/>
              </svg>
              Star on GitHub
            </a>
            <a href="https://www.nuget.org/packages/Whisper.net" target="_blank" rel="noopener noreferrer" className="btn btn-nu">
              <svg className="icon-default" width="20" height="20" viewBox="0 0 24 24" fill="none" stroke="currentColor" strokeWidth="2" strokeLinecap="round" strokeLinejoin="round" aria-hidden="true">
                <path d="M16.5 9.4l-9-5.19M21 16V8a2 2 0 0 0-1-1.73l-7-4a2 2 0 0 0-2 0l-7 4A2 2 0 0 0 3 8v8a2 2 0 0 0 1 1.73l7 4a2 2 0 0 0 2 0l7-4A2 2 0 0 0 21 16z"/>
                <polyline points="3.27 6.96 12 12.01 20.73 6.96"/>
                <line x1="12" y1="22.08" x2="12" y2="12"/>
              </svg>
              <svg className="icon-hover" width="20" height="20" viewBox="0 0 24 24" fill="none" stroke="currentColor" strokeWidth="2" strokeLinecap="round" strokeLinejoin="round" aria-hidden="true">
                <path d="M21 15v4a2 2 0 0 1-2 2H5a2 2 0 0 1-2-2v-4"/>
                <polyline points="7 10 12 15 17 10"/>
                <line x1="12" y1="15" x2="12" y2="3"/>
              </svg>
              NuGet
            </a>
          </div>

          <div className="install-bar">
            <span className="install-prompt">$</span>
            <code>dotnet add package Whisper.net.AllRuntimes</code>
            <CopyButton text="dotnet add package Whisper.net.AllRuntimes" />
          </div>
          </div>
        </div>
        <button type="button" className="scroll-cue" aria-label="Scroll down" onClick={scrollToContent}>↓</button>
      </section>

      {/* ── Content ── */}
      <div className="content" ref={contentRef}>

        <div className="features-grid">
          {/* Native Speed */}
          <section className="card card-sm">
            <div className="card-head">
              <svg className="card-icon" width="24" height="24" viewBox="0 0 24 24" fill="none" stroke="currentColor" strokeWidth="2" strokeLinecap="round" strokeLinejoin="round">
                <polygon points="13 2 3 14 12 14 11 22 21 10 12 10 13 2"/>
              </svg>
              <h2>Native speed</h2>
            </div>
            <p>C/C++ inference via whisper.cpp — no Python, no containers. Hardware-accelerated with CUDA, CoreML, Vulkan, and OpenVINO.</p>
          </section>

          {/* Async API */}
          <section className="card card-sm">
            <div className="card-head">
              <svg className="card-icon" width="24" height="24" viewBox="0 0 24 24" fill="none" stroke="currentColor" strokeWidth="2" strokeLinecap="round" strokeLinejoin="round">
                <polyline points="22 12 18 12 15 21 9 3 6 12 2 12"/>
              </svg>
              <h2>Async-first API</h2>
            </div>
            <p>Stream transcription results with <code>IAsyncEnumerable</code>. Process audio in real time or batch — your call.</p>
          </section>

          {/* Pluggable Runtimes */}
          <section className="card card-sm">
            <div className="card-head">
              <svg className="card-icon" width="24" height="24" viewBox="0 0 24 24" fill="none" stroke="currentColor" strokeWidth="2" strokeLinecap="round" strokeLinejoin="round">
                <rect x="2" y="6" width="20" height="12" rx="2"/>
                <path d="M6 12h4"/>
                <path d="M14 12h4"/>
                <path d="M12 6v12"/>
              </svg>
              <h2>Pluggable runtimes</h2>
            </div>
            <p>Install one NuGet package or many. The loader probes available runtimes and picks the fastest with automatic fallback.</p>
          </section>

          {/* 99+ Languages */}
          <section className="card card-sm">
            <div className="card-head">
              <svg className="card-icon" width="24" height="24" viewBox="0 0 24 24" fill="none" stroke="currentColor" strokeWidth="2" strokeLinecap="round" strokeLinejoin="round">
                <circle cx="12" cy="12" r="10"/>
                <line x1="2" y1="12" x2="22" y2="12"/>
                <path d="M12 2a15.3 15.3 0 0 1 4 10 15.3 15.3 0 0 1-4 10 15.3 15.3 0 0 1-4-10 15.3 15.3 0 0 1 4-10z"/>
              </svg>
              <h2>99+ languages</h2>
            </div>
            <p>Powered by OpenAI's Whisper model — accurate speech recognition and translation across virtually every spoken language.</p>
          </section>
        </div>

        {/* Runtimes */}
        <section className="card">
          <div className="card-head">
            <svg className="card-icon" width="24" height="24" viewBox="0 0 24 24" fill="none" stroke="currentColor" strokeWidth="2" strokeLinecap="round" strokeLinejoin="round">
              <rect x="4" y="4" width="16" height="16" rx="2"/>
              <rect x="9" y="9" width="6" height="6"/>
              <path d="M15 2v2"/><path d="M15 20v2"/><path d="M2 15h2"/><path d="M20 15h2"/>
              <path d="M9 2v2"/><path d="M9 20v2"/><path d="M2 9h2"/><path d="M20 9h2"/>
            </svg>
            <h2>Pick your hardware</h2>
          </div>
          <div className="rt-grid">
            {[
              ['CPU',        'All platforms — Win, Linux, macOS, Mobile, WASM'],
              ['CUDA 12/13', 'NVIDIA GPU — Win & Linux x64'],
              ['CoreML',     'Apple Neural Engine — macOS, iOS'],
              ['OpenVINO',   'Intel acceleration — Win & Linux x64'],
              ['Vulkan',     'GPU via Vulkan API — Win x64'],
              ['NoAvx',      'CPUs without AVX instructions'],
            ].map(([name, desc]) => (
              <div key={name} className="rt-item">
                <strong>{name}</strong>
                <span>{desc}</span>
              </div>
            ))}
          </div>
        </section>

        {/* Code */}
        <section className="card card-code">
          <div className="card-head">
            <svg className="card-icon" width="24" height="24" viewBox="0 0 24 24" fill="none" stroke="currentColor" strokeWidth="2" strokeLinecap="round" strokeLinejoin="round">
              <polyline points="16 18 22 12 16 6"/>
              <polyline points="8 6 2 12 8 18"/>
            </svg>
            <h2>A few lines of C#</h2>
          </div>
          <div className="code-block">
            <div className="code-chrome">
              <span /><span /><span />
              <span className="code-name">Program.cs</span>
              <CopyButton text={`using var factory = WhisperFactory.FromPath("ggml-base.bin");\n\nusing var processor = factory.CreateBuilder()\n    .WithLanguage("auto")\n    .Build();\n\nusing var stream = File.OpenRead("audio.wav");\n\nawait foreach (var r in processor.ProcessAsync(stream))\n{\n    Console.WriteLine($"{r.Start}->{r.End}: {r.Text}");\n}`} />
            </div>
            <pre><span className="kw">using</span> <span className="kw">var</span> factory = <span className="tp">WhisperFactory</span>.<span className="mt">FromPath</span>(<span className="st">"ggml-base.bin"</span>);{'\n'}{'\n'}<span className="kw">using</span> <span className="kw">var</span> processor = factory.<span className="mt">CreateBuilder</span>(){'\n'}    .<span className="mt">WithLanguage</span>(<span className="st">"auto"</span>){'\n'}    .<span className="mt">Build</span>();{'\n'}{'\n'}<span className="kw">using</span> <span className="kw">var</span> stream = <span className="tp">File</span>.<span className="mt">OpenRead</span>(<span className="st">"audio.wav"</span>);{'\n'}{'\n'}<span className="cf">await</span> <span className="cf">foreach</span> (<span className="kw">var</span> r <span className="cf">in</span> processor.<span className="mt">ProcessAsync</span>(stream)){'\n'}{'{'}{'\n'}    <span className="tp">Console</span>.<span className="mt">WriteLine</span>(<span className="st">$"</span>{'{'}r.Start{'}'}<span className="st">-&gt;</span>{'{'}r.End{'}'}<span className="st">: </span>{'{'}r.Text{'}'}<span className="st">"</span>);{'\n'}{'}'}</pre>
          </div>
        </section>

        {/* Platforms */}
        <section className="card">
          <div className="card-head">
            <svg className="card-icon" width="24" height="24" viewBox="0 0 24 24" fill="none" stroke="currentColor" strokeWidth="2" strokeLinecap="round" strokeLinejoin="round">
              <rect x="2" y="3" width="20" height="14" rx="2" ry="2"/>
              <line x1="8" y1="21" x2="16" y2="21"/>
              <line x1="12" y1="17" x2="12" y2="21"/>
            </svg>
            <h2>Runs everywhere</h2>
          </div>
          <div className="platforms">
            {['Windows', 'Linux', 'macOS', 'Android', 'iOS', 'MacCatalyst', 'tvOS', 'WebAssembly'].map((p) => (
              <span key={p} className="chip">{p}</span>
            ))}
          </div>
        </section>

        {/* Bottom CTA row */}
        <div className="bottom-row">
          {/* CTA */}
          <section className="card card-cta">
            <div className="card-head">
              <svg className="card-icon" width="24" height="24" viewBox="0 0 24 24" fill="none" stroke="currentColor" strokeWidth="2" strokeLinecap="round" strokeLinejoin="round">
                <path d="M4.5 16.5c-1.5 1.26-2 5-2 5s3.74-.5 5-2c.71-.84.7-2.13-.09-2.91a2.18 2.18 0 0 0-2.91-.09z"/>
                <path d="M12 15l-3-3a22 22 0 0 1 2-3.95A12.88 12.88 0 0 1 22 2c0 2.72-.78 7.5-6 11a22.35 22.35 0 0 1-4 2z"/>
                <path d="M9 12H4s.55-3.03 2-4c1.62-1.08 5 0 5 0"/>
                <path d="M12 15v5s3.03-.55 4-2c1.08-1.62 0-5 0-5"/>
              </svg>
              <h2>Ready to get started?</h2>
            </div>
            <p>Add Whisper.net to your project and start transcribing audio in minutes.</p>
            <div className="hero-row">
              <a href="https://github.com/sandrohanea/whisper.net" target="_blank" rel="noopener noreferrer" className="btn btn-gh">
                <svg className="icon-default" width="20" height="20" viewBox="0 0 24 24" fill="currentColor" aria-hidden="true">
                  <path d="M12 .3a12 12 0 0 0-3.8 23.38c.6.1.83-.26.83-.57v-2.23c-3.34.73-4.04-1.42-4.04-1.42a3.18 3.18 0 0 0-1.34-1.75c-1.08-.74.08-.73.08-.73a2.53 2.53 0 0 1 1.84 1.24 2.56 2.56 0 0 0 3.5 1 2.56 2.56 0 0 1 .76-1.61c-2.67-.3-5.47-1.33-5.47-5.93a4.64 4.64 0 0 1 1.24-3.22 4.3 4.3 0 0 1 .12-3.18s1-.32 3.3 1.23a11.4 11.4 0 0 1 6.01 0c2.3-1.55 3.3-1.23 3.3-1.23a4.3 4.3 0 0 1 .12 3.18 4.64 4.64 0 0 1 1.24 3.22c0 4.61-2.8 5.63-5.48 5.92a2.87 2.87 0 0 1 .82 2.23v3.29c0 .31.22.68.83.57A12 12 0 0 0 12 .3"/>
                </svg>
                <svg className="icon-hover" width="20" height="20" viewBox="0 0 24 24" fill="currentColor" aria-hidden="true">
                  <path d="M12 2l3.09 6.26L22 9.27l-5 4.87 1.18 6.88L12 17.77l-6.18 3.25L7 14.14 2 9.27l6.91-1.01L12 2z"/>
                </svg>
                Star on GitHub
              </a>
              <a href="https://www.nuget.org/packages/Whisper.net" target="_blank" rel="noopener noreferrer" className="btn btn-nu">
                <svg className="icon-default" width="20" height="20" viewBox="0 0 24 24" fill="none" stroke="currentColor" strokeWidth="2" strokeLinecap="round" strokeLinejoin="round" aria-hidden="true">
                  <path d="M16.5 9.4l-9-5.19M21 16V8a2 2 0 0 0-1-1.73l-7-4a2 2 0 0 0-2 0l-7 4A2 2 0 0 0 3 8v8a2 2 0 0 0 1 1.73l7 4a2 2 0 0 0 2 0l7-4A2 2 0 0 0 21 16z"/>
                  <polyline points="3.27 6.96 12 12.01 20.73 6.96"/>
                  <line x1="12" y1="22.08" x2="12" y2="12"/>
                </svg>
                <svg className="icon-hover" width="20" height="20" viewBox="0 0 24 24" fill="none" stroke="currentColor" strokeWidth="2" strokeLinecap="round" strokeLinejoin="round" aria-hidden="true">
                  <path d="M21 15v4a2 2 0 0 1-2 2H5a2 2 0 0 1-2-2v-4"/>
                  <polyline points="7 10 12 15 17 10"/>
                  <line x1="12" y1="15" x2="12" y2="3"/>
                </svg>
                Get on NuGet
              </a>
            </div>
          </section>

          {/* Contribute */}
          <section className="card card-cta">
            <div className="card-head">
              <svg className="card-icon" width="24" height="24" viewBox="0 0 24 24" fill="none" stroke="currentColor" strokeWidth="2" strokeLinecap="round" strokeLinejoin="round">
                <path d="M20.84 4.61a5.5 5.5 0 0 0-7.78 0L12 5.67l-1.06-1.06a5.5 5.5 0 0 0-7.78 7.78l1.06 1.06L12 21.23l7.78-7.78 1.06-1.06a5.5 5.5 0 0 0 0-7.78z"/>
              </svg>
              <h2>Want to contribute?</h2>
            </div>
            <p>Whisper.net is open source and welcomes contributions — bug reports, feature requests, and pull requests are all appreciated.</p>
            <div className="hero-row">
              <a href="https://github.com/sandrohanea/whisper.net/issues" target="_blank" rel="noopener noreferrer" className="btn btn-gh">
                <svg className="icon-default" width="20" height="20" viewBox="0 0 24 24" fill="none" stroke="currentColor" strokeWidth="2" strokeLinecap="round" strokeLinejoin="round" aria-hidden="true">
                  <circle cx="12" cy="12" r="10"/>
                  <line x1="12" y1="8" x2="12" y2="12"/>
                  <line x1="12" y1="16" x2="12.01" y2="16"/>
                </svg>
                <svg className="icon-hover" width="20" height="20" viewBox="0 0 24 24" fill="none" stroke="currentColor" strokeWidth="2" strokeLinecap="round" strokeLinejoin="round" aria-hidden="true">
                  <circle cx="12" cy="12" r="10"/>
                  <line x1="12" y1="8" x2="12" y2="16"/>
                  <line x1="8" y1="12" x2="16" y2="12"/>
                </svg>
                Open Issue
              </a>
              <a href="https://github.com/sandrohanea/whisper.net/blob/main/CONTRIBUTING.md" target="_blank" rel="noopener noreferrer" className="btn btn-gh">
                <svg className="icon-default" width="20" height="20" viewBox="0 0 24 24" fill="none" stroke="currentColor" strokeWidth="2" strokeLinecap="round" strokeLinejoin="round" aria-hidden="true">
                  <path d="M12 20h9"/>
                  <path d="M16.5 3.5a2.121 2.121 0 0 1 3 3L7 19l-4 1 1-4L16.5 3.5z"/>
                </svg>
                <svg className="icon-hover" width="20" height="20" viewBox="0 0 24 24" fill="none" stroke="currentColor" strokeWidth="2" strokeLinecap="round" strokeLinejoin="round" aria-hidden="true">
                  <path d="M15 3h6v6"/>
                  <path d="M10 14L21 3"/>
                  <path d="M18 13v6a2 2 0 0 1-2 2H5a2 2 0 0 1-2-2V8a2 2 0 0 1 2-2h6"/>
                </svg>
                Contribute
              </a>
            </div>
          </section>
        </div>

      </div>

      {/* ── Footer ── */}
      <footer className="site-footer">
        <p>
          Created by <a href="https://sandro.rocks/" target="_blank" rel="noopener noreferrer">Sandro Hanea</a>
          {' · '}Built by the <a href="https://github.com/sandrohanea/whisper.net" target="_blank" rel="noopener noreferrer">Whisper.net</a> community
          {' · '}Powered by <a href="https://github.com/ggerganov/whisper.cpp" target="_blank" rel="noopener noreferrer">whisper.cpp</a>
        </p>
        <p className="mit">
          Released under the <a href="https://github.com/sandrohanea/whisper.net/blob/main/LICENSE" target="_blank" rel="noopener noreferrer">MIT License</a>
        </p>
      </footer>
    </>
  )
}

export default App
