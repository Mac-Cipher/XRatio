(() => {
  const root = document.documentElement;
  const languageButtons = [...document.querySelectorAll('[data-language]')];
  const translatable = [...document.querySelectorAll('[data-fr][data-en]')];
  const description = document.querySelector('meta[name="description"]');
  const ogDescription = document.querySelector('meta[property="og:description"]');
  const heroVisual = document.querySelector('.hero-visual');
  const localizedAlts = [...document.querySelectorAll('[data-fr-alt][data-en-alt]')];
  const localizedLinks = [...document.querySelectorAll('[data-fr-href][data-en-href]')];

  const copy = {
    fr: {
      title: 'XRatio — Contrôle ratio local',
      description: 'XRatio — contrôle local des annonces de trackers et simulation torrent maîtrisée.',
      ogDescription: 'Une interface native pour observer, comprendre et contrôler les annonces de trackers.',
      navLabel: 'Navigation principale',
      languageLabel: 'Choisir la langue',
      heroVisualLabel: 'Aperçu de l’application XRatio'
    },
    en: {
      title: 'XRatio — Local ratio control',
      description: 'XRatio — local tracker announce control and deliberate torrent simulation.',
      ogDescription: 'A native interface to observe, understand, and control tracker announces.',
      navLabel: 'Main navigation',
      languageLabel: 'Choose language',
      heroVisualLabel: 'XRatio application preview'
    }
  };

  function setLanguage(language, persist = true) {
    const selected = copy[language] ? language : 'fr';
    root.lang = selected;
    document.title = copy[selected].title;
    description?.setAttribute('content', copy[selected].description);
    ogDescription?.setAttribute('content', copy[selected].ogDescription);
    document.querySelector('nav')?.setAttribute('aria-label', copy[selected].navLabel);
    document.querySelector('.language-switch')?.setAttribute('aria-label', copy[selected].languageLabel);
    heroVisual?.setAttribute('aria-label', copy[selected].heroVisualLabel);
    localizedAlts.forEach((element) => {
      const value = element.dataset[`${selected}Alt`];
      if (value) element.setAttribute('alt', value);
    });
    localizedLinks.forEach((element) => {
      const value = element.dataset[`${selected}Href`];
      if (value) element.setAttribute('href', value);
    });

    translatable.forEach((element) => {
      const value = element.dataset[selected];
      if (!value) return;
      const icon = element.querySelector('[aria-hidden]');
      element.textContent = value;
      if (icon) element.append(icon);
    });

    languageButtons.forEach((button) => {
      const active = button.dataset.language === selected;
      button.classList.toggle('is-active', active);
      button.setAttribute('aria-pressed', String(active));
    });

    if (persist) localStorage.setItem('xratio-language', selected);
  }

  languageButtons.forEach((button) => {
    button.addEventListener('click', () => setLanguage(button.dataset.language));
  });

  const savedLanguage = localStorage.getItem('xratio-language');
  setLanguage(savedLanguage || 'en', false);

  const header = document.querySelector('[data-header]');
  const updateHeader = () => header?.classList.toggle('is-scrolled', window.scrollY > 12);
  updateHeader();
  window.addEventListener('scroll', updateHeader, { passive: true });
})();
