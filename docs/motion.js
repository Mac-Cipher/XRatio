/* Scroll choreography follows the reader; no background animation loop. */
(() => {
  const motion = matchMedia('(prefers-reduced-motion: reduce)');
  const desktop = matchMedia('(min-width: 761px)');
  const gallery = document.querySelector('.gallery-scroll');
  const hero = document.querySelector('.product-stage');
  const flow = document.querySelector('.message-flow');
  if (!gallery || !hero || !flow) return;
  const clamp = value => Math.min(1, Math.max(0, value));
  let frame = 0;
  let flowAnimation = 0;
  let flowPlayed = false;
  let flowValue = 12;
  const renderNumbers = () => {
    const unit = document.documentElement.lang === 'fr' ? 'Go' : 'GB';
    flow.querySelector('.flow-number').textContent = `4 ${unit}`;
    flow.querySelector('[data-flow-adjusted]').textContent = `${flowValue} ${unit}`;
    flow.querySelector('[data-flow-received]').textContent = `${flowValue} ${unit}`;
  };
  function animateFlow() {
    if (flowPlayed || motion.matches) return;
    flowPlayed = true;
    flow.classList.add('flow-playing');
    const start = performance.now();
    const tick = now => {
      const progress = clamp((now - start - 500) / 1400);
      flowValue = Math.round(4 + 8 * (1 - Math.pow(1 - progress, 3)));
      renderNumbers();
      if (progress < 1 && !motion.matches) flowAnimation = requestAnimationFrame(tick);
      else flowAnimation = 0;
    };
    flowAnimation = requestAnimationFrame(tick);
  }
  function update() {
    frame = 0;
    const enabled = !motion.matches;
    document.documentElement.classList.toggle('cinematic', enabled);
    if (!enabled) return;
    const rect = gallery.getBoundingClientRect();
    const progress = clamp((innerHeight * .65 - rect.top) / Math.max(1, rect.height - innerHeight * .3));
    const dark = clamp((progress - .25) / .45);
    gallery.style.setProperty('--scene-dark', dark.toFixed(3));
    gallery.style.setProperty('--scene-scale', (.88 + .12 * clamp(progress / .45)).toFixed(3));
    gallery.style.setProperty('--scene-tilt', `${(7 * (1 - clamp(progress / .45))).toFixed(2)}deg`);
    gallery.style.setProperty('--scene-progress', progress.toFixed(3));
    const heroRect = hero.getBoundingClientRect();
    const entry = clamp((innerHeight - heroRect.top) / (innerHeight * .7));
    hero.style.setProperty('--hero-scale', (.89 + entry * .11).toFixed(3));
    hero.style.setProperty('--hero-tilt', `${(10 * (1 - entry)).toFixed(2)}deg`);
    const flowRect = flow.getBoundingClientRect();
    if (flowRect.top < innerHeight * .8 && flowRect.bottom > 0) animateFlow();
  }
  function queue() { if (!frame) frame = requestAnimationFrame(update); }
  motion.addEventListener('change', () => {
    if (motion.matches) { cancelAnimationFrame(flowAnimation); flowValue = 12; renderNumbers(); }
    queue();
  });
  desktop.addEventListener('change', queue);
  document.addEventListener('xratio-language-change', () => { renderNumbers(); queue(); });
  addEventListener('scroll', queue, {passive:true});
  addEventListener('resize', queue, {passive:true});
  addEventListener('pagehide', () => { cancelAnimationFrame(frame); cancelAnimationFrame(flowAnimation); frame = 0; flowValue = 12; renderNumbers(); });
  addEventListener('pageshow', queue);
  renderNumbers();
  update();
})();
