(() => {
  const root = document.documentElement;
  let theme = 'dim';
  const text = {
    en: {title:'XRatio — Your torrents. A clearer view.',description:'Observe tracker announces, control reported counters and run independent torrent simulations with XRatio for Windows.',nav:'Main navigation',language:'Choose language',preview:'Preview theme',alt:'XRatio application overview',skip:'Skip to content'},
    fr: {title:'XRatio — Vos torrents. Une vision plus claire.',description:'Observez les annonces aux trackers, ajustez les compteurs rapportés et lancez des simulations indépendantes avec XRatio pour Windows.',nav:'Navigation principale',language:'Choisir la langue',preview:'Thème de l’aperçu',alt:'Vue d’ensemble de l’application XRatio',skip:'Aller au contenu'}
  };
  function setLanguage(value, persist = true) {
    const language = value === 'fr' ? 'fr' : 'en';
    root.lang = language;
    const copy = text[language];
    document.title = copy.title;
    document.querySelector('meta[property="og:title"]').content = copy.title;
    for (const selector of ['meta[name="description"]','meta[property="og:description"]']) document.querySelector(selector).content = copy.description;
    document.querySelectorAll('[data-en][data-fr]').forEach(el => { el.textContent = el.dataset[language]; });
    document.querySelectorAll('[data-language]').forEach(el => el.setAttribute('aria-pressed', String(el.dataset.language === language)));
    document.querySelector('nav').setAttribute('aria-label', copy.nav);
    document.querySelector('.language-switch').setAttribute('aria-label', copy.language);
    document.querySelector('.theme-switch').setAttribute('aria-label', copy.preview);
    document.querySelector('#screenshot').alt = copy.alt;
    document.querySelector('.skip').textContent = copy.skip;
    document.querySelector('[data-doc-link]').href = language === 'fr' ? 'https://github.com/Mac-Cipher/XRatio/blob/master/README.fr.md' : 'https://github.com/Mac-Cipher/XRatio#install-and-configure-a-torrent-client';
    if (persist) { try { localStorage.setItem('xratio-language', language); } catch {} }
  }
  document.querySelectorAll('[data-language]').forEach(el => el.addEventListener('click', () => setLanguage(el.dataset.language)));
  document.querySelectorAll('[data-theme]').forEach(el => el.addEventListener('click', () => {
    theme = el.dataset.theme;
    const src = theme === 'light' ? 'screenshots/overview-light.png' : 'screenshots/overview-dim-theme.png';
    document.querySelector('#screenshot').src = src;
    document.querySelector('#screenshot-link').href = src;
    document.querySelectorAll('[data-theme]').forEach(button => button.setAttribute('aria-pressed', String(button.dataset.theme === theme)));
  }));
  let saved = 'en';
  try { saved = localStorage.getItem('xratio-language') || 'en'; } catch {}
  setLanguage(saved, false);
})();
