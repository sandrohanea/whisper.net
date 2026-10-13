for (const button of document.querySelectorAll('.copy-btn')) {
  let resetTimer;
  button.addEventListener('click', async () => {
    const text = button.closest('.install-bar')?.querySelector('code').textContent
      ?? button.closest('.code-block').querySelector('pre').textContent;
    try {
      await navigator.clipboard.writeText(text);
      button.textContent = '✓ copied';
      clearTimeout(resetTimer);
      resetTimer = setTimeout(() => { button.textContent = 'copy'; }, 2000);
    } catch {
      // Clipboard access can be unavailable or denied.
    }
  });
}

document.querySelector('.scroll-cue').addEventListener('click', () => {
  const target = document.querySelector('#content').offsetTop;
  const start = window.scrollY;
  const distance = target - start;
  let startTime;

  function step(timestamp) {
    startTime ??= timestamp;
    const progress = Math.min((timestamp - startTime) / 800, 1);
    const ease = progress < 0.5
      ? 4 * progress * progress * progress
      : 1 - Math.pow(-2 * progress + 2, 3) / 2;
    window.scrollTo(0, start + distance * ease);
    if (progress < 1) requestAnimationFrame(step);
  }

  requestAnimationFrame(step);
});
