(() => {
  const root = document.documentElement;
  const dialog = document.querySelector('#preview-dialog');
  const image = document.querySelector('#screenshot');
  const fullImage = document.querySelector('#full-screenshot');
  let language = 'en';
  let theme = 'light';
  const copy = {
    en: {
      title: 'XRatio — Your torrents. Your control.',
      description: 'XRatio brings local tracker interception and independent torrent simulation to one native Windows app.',
      navigation: 'Main navigation', language: 'Choose language', theme: 'Preview theme',
      preview: 'Application screenshot', enlarge: 'Enlarge application screenshot', close: 'Close screenshot',
      alt: 'XRatio application overview', doc: 'https://github.com/Mac-Cipher/XRatio#readme'
    },
    fr: {
      title: 'XRatio — Vos torrents. Votre contrôle.',
      description: 'XRatio réunit interception locale des trackers et simulation torrent indépendante dans une application Windows native.',
      navigation: 'Navigation principale', language: 'Choisir la langue', theme: 'Thème de l’aperçu',
      preview: 'Capture de l’application', enlarge: 'Agrandir la capture de l’application', close: 'Fermer la capture',
      alt: 'Vue d’ensemble de l’application XRatio', doc: 'https://github.com/Mac-Cipher/XRatio/blob/master/README.fr.md'
    }
  };
  function updateImageLabel() {
    const name = theme === 'light' ? (language === 'fr' ? 'clair' : 'light') : (language === 'fr' ? 'tamisé' : 'dim');
    image.alt = `${copy[language].alt} — ${name}`;
    fullImage.alt = image.alt;
  }
  function setLanguage(value, persist = true) {
    language = value === 'fr' ? 'fr' : 'en';
    root.lang = language;
    const selected = copy[language];
    document.title = selected.title;
    document.querySelector('meta[property="og:title"]').content = selected.title;
    document.querySelector('meta[name="description"]').content = selected.description;
    document.querySelector('meta[property="og:description"]').content = selected.description;
    document.querySelectorAll('[data-en][data-fr]').forEach(element => { element.textContent = element.dataset[language]; });
    document.querySelectorAll('[data-language]').forEach(button => button.setAttribute('aria-pressed', String(button.dataset.language === language)));
    document.querySelector('nav').setAttribute('aria-label', selected.navigation);
    document.querySelector('.languages').setAttribute('aria-label', selected.language);
    document.querySelector('.theme-switch').setAttribute('aria-label', selected.theme);
    document.querySelector('[data-open-preview]').setAttribute('aria-label', selected.enlarge);
    document.querySelector('[data-close-preview]').setAttribute('aria-label', selected.close);
    dialog.setAttribute('aria-label', selected.preview);
    document.querySelectorAll('[data-doc]').forEach(link => { link.href = selected.doc; });
    updateImageLabel();
    if (persist) { try { localStorage.setItem('xratio-language', language); } catch {} }
  }
  document.querySelectorAll('[data-language]').forEach(button => {
    button.addEventListener('click', () => setLanguage(button.dataset.language));
  });
  document.querySelectorAll('[data-theme]').forEach(button => {
    button.addEventListener('click', () => {
      theme = button.dataset.theme;
      const source = theme === 'light' ? 'screenshots/overview-light.png' : 'screenshots/overview-dim-theme.png';
      image.src = source;
      fullImage.src = source;
      document.querySelectorAll('[data-theme]').forEach(item => item.setAttribute('aria-pressed', String(item === button)));
      updateImageLabel();
    });
  });
  document.querySelector('[data-open-preview]').addEventListener('click', () => dialog.showModal());
  document.querySelector('[data-close-preview]').addEventListener('click', () => dialog.close());
  dialog.addEventListener('click', event => {
    if (event.target !== dialog) return;
    const bounds = dialog.getBoundingClientRect();
    if (event.clientX < bounds.left || event.clientX > bounds.right || event.clientY < bounds.top || event.clientY > bounds.bottom) dialog.close();
  });
  let saved = 'en';
  try { saved = localStorage.getItem('xratio-language') || 'en'; } catch {}
  setLanguage(saved, false);
})();
