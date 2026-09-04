(() => {
  const root = document.documentElement;
  const dialog = document.querySelector('#preview-dialog');
  const image = document.querySelector('#screenshot');
  const fullImage = document.querySelector('#full-screenshot');
  const modeButtons = [...document.querySelectorAll('[data-mode]')];
  let language = 'en';
  let theme = 'light';

  const copy = {
    en: {
      title: 'XRatio — Tracker announce control for Windows',
      description: 'XRatio is a Windows app for tracker announce interception and independent torrent simulation.',
      navigation: 'Main navigation', language: 'Choose language', theme: 'Screenshot theme',
      preview: 'Application preview', enlarge: 'Enlarge application screenshot', close: 'Close screenshot',
      mode: 'Operating mode', alt: 'XRatio application overview',
      doc: 'https://github.com/Mac-Cipher/XRatio#readme'
    },
    fr: {
      title: 'XRatio — Contrôle des annonces aux trackers pour Windows',
      description: 'XRatio est une application Windows d’interception des annonces aux trackers et de simulation torrent indépendante.',
      navigation: 'Navigation principale', language: 'Choisir la langue', theme: 'Thème de la capture',
      preview: 'Aperçu de l’application', enlarge: 'Agrandir la capture de l’application', close: 'Fermer la capture',
      mode: 'Mode de fonctionnement', alt: 'Vue d’ensemble de l’application XRatio',
      doc: 'https://github.com/Mac-Cipher/XRatio/blob/master/README.fr.md'
    }
  };

  function updateImageLabel() {
    const themeName = theme === 'light' ? (language === 'fr' ? 'clair' : 'light') : (language === 'fr' ? 'tamisé' : 'dim');
    image.alt = `${copy[language].alt} — ${themeName}`;
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
    document.querySelector('.language-switch').setAttribute('aria-label', selected.language);
    document.querySelector('.theme-switch').setAttribute('aria-label', selected.theme);
    document.querySelector('.showcase').setAttribute('aria-label', selected.preview);
    document.querySelector('[data-open-preview]').setAttribute('aria-label', selected.enlarge);
    document.querySelector('[data-close-preview]').setAttribute('aria-label', selected.close);
    document.querySelector('[role="tablist"]').setAttribute('aria-label', selected.mode);
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

  function activateMode(button) {
    modeButtons.forEach(item => {
      const selected = item === button;
      item.setAttribute('aria-selected', String(selected));
      item.tabIndex = selected ? 0 : -1;
      document.getElementById(item.getAttribute('aria-controls')).hidden = !selected;
    });
  }
  modeButtons.forEach((button, index) => {
    button.addEventListener('click', () => activateMode(button));
    button.addEventListener('keydown', event => {
      let target;
      if (['ArrowRight', 'ArrowDown'].includes(event.key)) target = (index + 1) % modeButtons.length;
      if (['ArrowLeft', 'ArrowUp'].includes(event.key)) target = (index + modeButtons.length - 1) % modeButtons.length;
      if (event.key === 'Home') target = 0;
      if (event.key === 'End') target = modeButtons.length - 1;
      if (target === undefined) return;
      event.preventDefault();
      activateMode(modeButtons[target]);
      modeButtons[target].focus();
    });
  });

  const navLinks = [...document.querySelectorAll('nav a')];
  function updateNavigation() {
    let current = navLinks[0];
    navLinks.forEach(link => {
      const section = document.querySelector(link.getAttribute('href'));
      if (section.getBoundingClientRect().top <= 180) current = link;
    });
    if (window.scrollY + window.innerHeight >= document.documentElement.scrollHeight - 8) current = navLinks.at(-1);
    navLinks.forEach(link => {
      link.classList.toggle('current', link === current);
      if (link === current) link.setAttribute('aria-current', 'location');
      else link.removeAttribute('aria-current');
    });
  }
  let scheduled = false;
  window.addEventListener('scroll', () => {
    if (scheduled) return;
    scheduled = true;
    requestAnimationFrame(() => { updateNavigation(); scheduled = false; });
  }, { passive: true });
  let saved = 'en';
  try { saved = localStorage.getItem('xratio-language') || 'en'; } catch {}
  setLanguage(saved, false);
  updateNavigation();
})();
