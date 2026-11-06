/* Scroll choreography follows the reader; no background animation loop. */
(() => {
  const motion = matchMedia('(prefers-reduced-motion: reduce)');
  const gallery = document.querySelector('.gallery-scroll');
  const hero = document.querySelector('.product-stage');
  const flow = document.querySelector('.message-flow');
  if (!gallery || !hero || !flow) return;
  const clamp = value => Math.min(1, Math.max(0, value));
  let frame = 0;
  let flowPlayed = false;
  const flowValue = 12;
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
  }
  function update() {
    frame = 0;
    const enabled = !motion.matches;
    document.documentElement.classList.toggle('cinematic', enabled);
    if (!enabled) {
      gallery.querySelector('.detail-screenshot').style.transform = '';
      gallery.querySelector('.gallery-timeline i').style.transform = '';
      hero.style.transform = '';
      flow.classList.remove('flow-playing');
      return;
    }
    const rect = gallery.getBoundingClientRect();
    const progress = clamp((innerHeight * .65 - rect.top) / Math.max(1, rect.height - innerHeight * .3));
    const dark = clamp((progress - .25) / .45);
    gallery.querySelector('.detail-screenshot').style.transform = `perspective(1800px) rotateY(${dark * 180}deg)`;
    gallery.querySelector('.gallery-timeline i').style.transform = `scaleX(${progress})`;
    const flowRect = flow.getBoundingClientRect();
    if (flowRect.top < innerHeight * .8 && flowRect.bottom > 0) animateFlow();
  }
  function queue() { if (!frame) frame = requestAnimationFrame(update); }
  motion.addEventListener('change', queue);
  document.addEventListener('xratio-language-change', () => { renderNumbers(); queue(); });
  addEventListener('scroll', queue, {passive:true});
  addEventListener('resize', queue, {passive:true});
  addEventListener('pagehide', () => { cancelAnimationFrame(frame); frame = 0; });
  addEventListener('pageshow', queue);
  renderNumbers();
  update();
})();
