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
      alt: 'XRatio application overview', doc: 'documentation.html'
    },
    fr: {
      title: 'XRatio — Vos torrents. Votre contrôle.',
      description: 'XRatio réunit interception locale des trackers et simulation torrent indépendante dans une application Windows native.',
      navigation: 'Navigation principale', language: 'Choisir la langue', theme: 'Thème de l’aperçu',
      preview: 'Capture de l’application', enlarge: 'Agrandir la capture de l’application', close: 'Fermer la capture',
      alt: 'Vue d’ensemble de l’application XRatio', doc: 'documentation.html'
    }
  };
  function updateImageLabel() {
    if (!image || !fullImage) return;
    const name = theme === 'light' ? (language === 'fr' ? 'clair' : 'light') : (language === 'fr' ? 'tamisé' : 'dim');
    image.alt = `${copy[language].alt} — ${name}`;
    fullImage.alt = image.alt;
  }
  function setLanguage(value, persist = true) {
    language = value === 'fr' ? 'fr' : 'en';
    root.lang = language;
    const selected = copy[language];
    const docs = root.dataset.page === 'documentation';
    const title = docs ? 'Documentation — XRatio' : selected.title;
    const description = docs ? (language === 'fr' ? 'Guide XRatio : installation, proxy qBittorrent, HTTPS, simulation et dépannage.' : 'XRatio guide: installation, qBittorrent proxy, HTTPS, simulation and troubleshooting.') : selected.description;
    document.title = title;
    document.querySelector('meta[property="og:title"]').content = title;
    document.querySelector('meta[name="description"]').content = description;
    document.querySelector('meta[property="og:description"]').content = description;
    document.querySelectorAll('[data-en][data-fr]').forEach(element => { element.textContent = element.dataset[language]; });
    document.querySelectorAll('[data-language]').forEach(button => button.setAttribute('aria-pressed', String(button.dataset.language === language)));
    document.querySelector('nav').setAttribute('aria-label', selected.navigation);
    document.querySelector('.languages').setAttribute('aria-label', selected.language);
    document.querySelector('.theme-switch')?.setAttribute('aria-label', selected.theme);
    document.querySelector('[data-open-preview]')?.setAttribute('aria-label', selected.enlarge);
    document.querySelector('[data-close-preview]')?.setAttribute('aria-label', selected.close);
    dialog?.setAttribute('aria-label', selected.preview);
    document.querySelectorAll('[data-doc]').forEach(link => { link.href = selected.doc; });
    document.querySelectorAll('[data-en-alt]').forEach(element => { element.alt = element.dataset[`${language}Alt`]; });
    document.querySelectorAll('[data-en-label]').forEach(element => { element.setAttribute('aria-label', element.dataset[`${language}Label`]); });
    updateImageLabel();
    if (persist) { try { localStorage.setItem('xratio-language', language); } catch {} }
    document.dispatchEvent(new Event('xratio-language-change'));
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
  document.querySelector('[data-open-preview]')?.addEventListener('click', () => dialog.showModal());
  document.querySelector('[data-close-preview]')?.addEventListener('click', () => dialog.close());
  dialog?.addEventListener('click', event => {
    if (event.target !== dialog) return;
    const bounds = dialog.getBoundingClientRect();
    if (event.clientX < bounds.left || event.clientX > bounds.right || event.clientY < bounds.top || event.clientY > bounds.bottom) dialog.close();
  });
  let saved = 'en';
  try { saved = localStorage.getItem('xratio-language') || 'en'; } catch {}
  setLanguage(saved, false);
  const motion = matchMedia('(prefers-reduced-motion: reduce)');
  const reveals = document.querySelectorAll('.reveal, .feature-heading, .feature-pair article, .desktop-note, .get-app');
  if ('IntersectionObserver' in window && !motion.matches) {
    const observer = new IntersectionObserver(entries => {
      entries.forEach(entry => {
        if (!entry.isIntersecting) return;
        entry.target.classList.add('is-visible');
        entry.target.classList.remove('reveal-pending');
        observer.unobserve(entry.target);
      });
    }, { threshold: 0.08 });
    reveals.forEach(element => { element.classList.add('reveal-pending'); observer.observe(element); });
    motion.addEventListener('change', event => {
      if (!event.matches) return;
      observer.disconnect();
      reveals.forEach(element => { element.classList.remove('reveal-pending'); element.classList.add('is-visible'); });
    });
  }
})();
