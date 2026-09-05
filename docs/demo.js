(() => {
  const container = document.querySelector('[data-demo]');
  if (!container) return;
  const model = new XRatioDemoModel();
  const get = id => document.getElementById(id);
  const toggle = get('demo-toggle');
  const speed = get('demo-speed');
  let timer = null;
  let lastTime = 0;
  let event = 'ready';
  const text = (en, fr) => document.documentElement.lang === 'fr' ? fr : en;
  const number = (value, digits = 2) => value.toLocaleString(document.documentElement.lang, { minimumFractionDigits: digits, maximumFractionDigits: digits });
  const gb = value => `${number(value)} ${text('GB', 'Go')}`;
  function announceStatus() {
    const messages = {
      ready: text('Ready to send the first example announce.', 'Prêt à envoyer la première annonce d’exemple.'),
      running: text('Demo running. Counters will be sent at the next announce.', 'Démo en cours. Les compteurs seront envoyés à la prochaine annonce.'),
      paused: text('Demo paused. Counters and the timer are frozen.', 'Démo en pause. Les compteurs et le minuteur sont figés.'),
      announce: text(`Example announce #${model.announces} received. Ratio ${number(model.ratio)}.`, `Annonce d’exemple n° ${model.announces} reçue. Ratio ${number(model.ratio)}.`),
      completed: text('Example complete: ratio 3.00. Reset to try again.', 'Exemple terminé : ratio 3,00. Réinitialisez pour recommencer.')
    };
    get('demo-event').textContent = messages[event];
  }
  function render() {
    container.classList.toggle('is-running', model.running);
    get('demo-state').textContent = model.finished ? text('Complete', 'Terminé') : model.running ? text('Running', 'En cours') : model.elapsed > 0 ? text('Paused', 'En pause') : text('Ready', 'Prêt');
    toggle.textContent = model.running ? 'Pause' : model.elapsed > 0 ? text('Resume', 'Reprendre') : text('Start demo', 'Lancer la démo');
    if (model.finished) toggle.textContent = text('Complete', 'Terminé');
    toggle.disabled = model.finished;
    toggle.setAttribute('aria-pressed', String(model.running));
    get('demo-announce').disabled = model.upload <= model.reported;
    speed.disabled = model.finished;
    get('demo-speed-value').textContent = `${model.speed} ${text('MB/s', 'Mo/s')}`;
    get('demo-upload').textContent = gb(model.upload - 4);
    get('demo-countdown').textContent = model.finished ? '—' : `${number(model.nextAnnounce, 1)} s`;
    get('tracker-ratio').textContent = number(model.ratio);
    get('tracker-upload').textContent = gb(model.reported);
    get('tracker-download').textContent = gb(model.download);
    get('tracker-announces').textContent = `${model.announces} ${text(model.announces === 1 ? 'announce' : 'announces', model.announces === 1 ? 'annonce' : 'annonces')}`;
    get('tracker-badge').textContent = model.ratio >= 1 ? text('Ratio ≥ 1', 'Ratio ≥ 1') : text('Ratio below 1', 'Ratio inférieur à 1');
    get('tracker-badge').classList.toggle('positive', model.ratio >= 1);
    const maxTime = Math.max(10, model.elapsed);
    const points = model.history.map(point => `${(point.time / maxTime * 310 + 5).toFixed(1)},${(90 - (point.ratio - .4) / 2.6 * 80).toFixed(1)}`);
    if (points.length === 1) points.push('315,90');
    get('ratio-line').setAttribute('points', points.join(' '));
    get('ratio-chart').setAttribute('aria-label', text(`Example ratio history: 0.40 to ${number(model.ratio)}`, `Historique du ratio d’exemple : de 0,40 à ${number(model.ratio)}`));
  }
  function stopTimer() { clearInterval(timer); timer = null; }
  function pause() {
    model.pause(); stopTimer(); event = 'paused'; render(); announceStatus();
  }
  toggle.addEventListener('click', () => {
    if (model.running) { pause(); return; }
    if (model.finished) return;
    model.start(); lastTime = performance.now(); event = 'running';
    render(); announceStatus();
    timer = setInterval(() => {
      const now = performance.now();
      const updated = model.tick(Math.min((now - lastTime) / 1000, .5));
      lastTime = now;
      if (updated) { event = 'announce'; announceStatus(); }
      if (model.finished) { stopTimer(); event = 'completed'; announceStatus(); }
      render();
    }, 100);
  });
  get('demo-reset').addEventListener('click', () => {
    stopTimer(); model.reset(); speed.value = '10'; event = 'ready'; render(); announceStatus();
  });
  get('demo-announce').addEventListener('click', () => {
    if (!model.announce()) return;
    event = 'announce'; render(); announceStatus();
  });
  speed.addEventListener('input', () => { model.setSpeed(Number(speed.value)); render(); });
  document.addEventListener('visibilitychange', () => { if (document.hidden && model.running) pause(); });
  window.addEventListener('pagehide', () => { model.pause(); stopTimer(); });
  document.addEventListener('xratio-language-change', () => { render(); announceStatus(); });
  render(); announceStatus();
})();
