import './App.css'

const features = [
  {
    icon: '🎙️',
    title: 'Speech Recognition',
    description: 'Accurate speech-to-text powered by OpenAI\'s Whisper model via whisper.cpp, supporting 99+ languages.',
  },
  {
    icon: '⚡',
    title: 'High Performance',
    description: 'Native C/C++ inference with hardware acceleration — CUDA, CoreML, Vulkan, and OpenVINO support.',
  },
  {
    icon: '🔀',
    title: 'Cross-Platform',
    description: 'Runs on Windows, Linux, macOS, Android, iOS, MacCatalyst, tvOS, and even WebAssembly.',
  },
  {
    icon: '📦',
    title: 'Easy NuGet Install',
    description: 'Simple package installation with automatic runtime selection. Just add the NuGet package and go.',
  },
  {
    icon: '🔄',
    title: 'Async Streaming',
    description: 'Process audio with async streaming via IAsyncEnumerable for real-time transcription results.',
  },
  {
    icon: '🧩',
    title: 'Pluggable Runtimes',
    description: 'Mix and match runtimes — CPU, CUDA 12/13, CoreML, Vulkan, OpenVINO — with automatic fallback.',
  },
]

const runtimes = [
  {
    icon: '🖥️',
    name: 'CPU Runtime',
    pkg: 'Whisper.net.Runtime',
    description: 'Default CPU inference for all platforms.',
    platforms: 'Win · Linux · macOS · Mobile · WASM',
  },
  {
    icon: '🎮',
    name: 'CUDA 13',
    pkg: 'Whisper.net.Runtime.Cuda',
    description: 'NVIDIA GPU acceleration with CUDA 13.',
    platforms: 'Win x64 · Linux x64',
  },
  {
    icon: '🎮',
    name: 'CUDA 12',
    pkg: 'Whisper.net.Runtime.Cuda12',
    description: 'NVIDIA GPU acceleration with CUDA 12.',
    platforms: 'Win x64 · Linux x64',
  },
  {
    icon: '🍎',
    name: 'CoreML',
    pkg: 'Whisper.net.Runtime.CoreML',
    description: 'Apple Neural Engine acceleration.',
    platforms: 'macOS · iOS · MacCatalyst',
  },
  {
    icon: '🔷',
    name: 'OpenVINO',
    pkg: 'Whisper.net.Runtime.OpenVino',
    description: 'Intel hardware acceleration.',
    platforms: 'Win x64 · Linux x64',
  },
  {
    icon: '🌋',
    name: 'Vulkan',
    pkg: 'Whisper.net.Runtime.Vulkan',
    description: 'GPU inference via Vulkan API.',
    platforms: 'Win x64',
  },
]

const platforms = [
  { icon: '🪟', name: 'Windows' },
  { icon: '🐧', name: 'Linux' },
  { icon: '🍎', name: 'macOS' },
  { icon: '🤖', name: 'Android' },
  { icon: '📱', name: 'iOS' },
  { icon: '🖥️', name: 'MacCatalyst' },
  { icon: '📺', name: 'tvOS' },
  { icon: '🌐', name: 'WebAssembly' },
]

function App() {
  return (
    <>
      {/* Navigation */}
      <nav className="nav">
        <div className="nav-inner">
          <div className="nav-logo">
            Whisper<span className="dot">.net</span>
          </div>
          <ul className="nav-links">
            <li><a href="#features">Features</a></li>
            <li><a href="#runtimes">Runtimes</a></li>
            <li><a href="#code">Usage</a></li>
            <li><a href="#platforms">Platforms</a></li>
          </ul>
          <a
            href="https://github.com/sandrohanea/whisper.net"
            target="_blank"
            rel="noopener noreferrer"
            className="nav-cta"
          >
            ⭐ GitHub
          </a>
        </div>
      </nav>

      {/* Hero */}
      <section className="hero">
        <div className="hero-content">
          <div className="hero-badge">
            🚀 Open Source · MIT Licensed
          </div>
          <h1>
            Speech-to-Text for <span className="gradient">.NET</span>
          </h1>
          <p className="hero-subtitle">
            Whisper.net brings OpenAI's Whisper speech recognition to .NET with
            native performance. Powered by whisper.cpp with GPU acceleration,
            cross-platform support, and a simple async API.
          </p>
          <div className="hero-actions">
            <a
              href="https://www.nuget.org/packages/Whisper.net"
              target="_blank"
              rel="noopener noreferrer"
              className="btn btn-primary"
            >
              📦 Get on NuGet
            </a>
            <a
              href="https://github.com/sandrohanea/whisper.net"
              target="_blank"
              rel="noopener noreferrer"
              className="btn btn-secondary"
            >
              View on GitHub →
            </a>
          </div>
          <div className="hero-install">
            <span>$</span>
            <code>dotnet add package Whisper.net.AllRuntimes</code>
          </div>
        </div>
      </section>

      {/* Features */}
      <section id="features" className="section section-alt">
        <div className="section-header">
          <h2>Why Whisper.net?</h2>
          <p>
            Everything you need for speech recognition in .NET, with native
            performance and a developer-friendly API.
          </p>
        </div>
        <div className="features-grid">
          {features.map((f) => (
            <div key={f.title} className="feature-card">
              <div className="feature-icon">{f.icon}</div>
              <h3>{f.title}</h3>
              <p>{f.description}</p>
            </div>
          ))}
        </div>
      </section>

      {/* Runtimes */}
      <section id="runtimes" className="section">
        <div className="section-header">
          <h2>Pick Your Runtime</h2>
          <p>
            Install one or many — Whisper.net automatically selects the best
            available runtime with intelligent fallback.
          </p>
        </div>
        <div className="runtimes-grid">
          {runtimes.map((r) => (
            <div key={r.name} className="runtime-card">
              <div className="runtime-icon">{r.icon}</div>
              <div className="runtime-info">
                <h3>{r.name}</h3>
                <p>{r.description}</p>
                <span className="runtime-badge">{r.platforms}</span>
              </div>
            </div>
          ))}
        </div>
      </section>

      {/* Code Example */}
      <section id="code" className="section section-alt">
        <div className="section-header">
          <h2>Simple, Powerful API</h2>
          <p>
            Just a few lines of C# to transcribe audio. Async streaming gives
            you results as they arrive.
          </p>
        </div>
        <div className="code-section">
          <div className="code-block">
            <div className="code-header">
              <span className="code-dot"></span>
              <span className="code-dot"></span>
              <span className="code-dot"></span>
              <span className="code-filename">Program.cs</span>
            </div>
            <pre>
              <span className="keyword">using</span> <span className="type">var</span> whisperFactory = <span className="type">WhisperFactory</span>.<span className="method">FromPath</span>(<span className="string">"ggml-base.bin"</span>);{'\n'}
{'\n'}
<span className="keyword">using</span> <span className="type">var</span> processor = whisperFactory.<span className="method">CreateBuilder</span>(){'\n'}
    .<span className="method">WithLanguage</span>(<span className="string">"auto"</span>){'\n'}
    .<span className="method">Build</span>();{'\n'}
{'\n'}
<span className="keyword">using</span> <span className="type">var</span> fileStream = <span className="type">File</span>.<span className="method">OpenRead</span>(<span className="string">"audio.wav"</span>);{'\n'}
{'\n'}
<span className="keyword">await foreach</span> (<span className="type">var</span> result <span className="keyword">in</span> processor.<span className="method">ProcessAsync</span>(fileStream)){'\n'}
{'{'}{'\n'}
    <span className="type">Console</span>.<span className="method">WriteLine</span>(<span className="string">$"</span>{'{'}result.Start{'}'}<span className="string">-&gt;</span>{'{'}result.End{'}'}<span className="string">: </span>{'{'}result.Text{'}'}<span className="string">"</span>);{'\n'}
{'}'}
            </pre>
          </div>
        </div>
      </section>

      {/* Platforms */}
      <section id="platforms" className="section">
        <div className="section-header">
          <h2>Runs Everywhere</h2>
          <p>
            From servers to mobile devices to the browser — Whisper.net has you
            covered.
          </p>
        </div>
        <div className="platforms-list">
          {platforms.map((p) => (
            <div key={p.name} className="platform-chip">
              <span className="icon">{p.icon}</span>
              {p.name}
            </div>
          ))}
        </div>
      </section>

      {/* CTA */}
      <section className="cta">
        <div className="cta-content">
          <h2>Ready to get started?</h2>
          <p>
            Add Whisper.net to your project today and start transcribing audio
            in minutes.
          </p>
          <div className="hero-actions">
            <a
              href="https://www.nuget.org/packages/Whisper.net"
              target="_blank"
              rel="noopener noreferrer"
              className="btn btn-primary"
            >
              📦 Install from NuGet
            </a>
            <a
              href="https://github.com/sandrohanea/whisper.net#readme"
              target="_blank"
              rel="noopener noreferrer"
              className="btn btn-secondary"
            >
              📖 Read the Docs
            </a>
          </div>
        </div>
      </section>

      {/* Footer */}
      <footer className="footer">
        <ul className="footer-links">
          <li><a href="https://github.com/sandrohanea/whisper.net" target="_blank" rel="noopener noreferrer">GitHub</a></li>
          <li><a href="https://www.nuget.org/packages/Whisper.net" target="_blank" rel="noopener noreferrer">NuGet</a></li>
          <li><a href="https://github.com/sandrohanea/whisper.net/blob/main/LICENSE" target="_blank" rel="noopener noreferrer">MIT License</a></li>
        </ul>
        <p>
          Built with ❤️ by the Whisper.net community · Powered by{' '}
          <a href="https://github.com/ggerganov/whisper.cpp" target="_blank" rel="noopener noreferrer">whisper.cpp</a>
        </p>
      </footer>
    </>
  )
}

export default App
