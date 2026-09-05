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
  copy.es = { title:'XRatio — Tus torrents. Tu control.', description:'Interceptación local de trackers y simulación independiente en una aplicación nativa de Windows.', navigation:'Navegación principal', language:'Elegir idioma', theme:'Tema de la vista previa', preview:'Captura de la aplicación', enlarge:'Ampliar la captura', close:'Cerrar la captura', alt:'Vista general de XRatio', doc:'documentation.html' };
  copy.de = { title:'XRatio — Deine Torrents. Deine Kontrolle.', description:'Lokale Tracker-Interception und unabhängige Simulation in einer nativen Windows-App.', navigation:'Hauptnavigation', language:'Sprache wählen', theme:'Vorschaudesign', preview:'App-Screenshot', enlarge:'Screenshot vergrößern', close:'Screenshot schließen', alt:'XRatio-Übersicht', doc:'documentation.html' };
  function updateImageLabel() {
    if (!image || !fullImage) return;
    const name = ({en:{light:'light',dim:'dim'},fr:{light:'clair',dim:'tamisé'},es:{light:'claro',dim:'atenuado'},de:{light:'hell',dim:'gedimmt'}})[language][theme];
    image.alt = `${(copy[language] || copy.en).alt} — ${name}`;
    fullImage.alt = image.alt;
  }
  function setLanguage(value, persist = true) {
    language = ['en', 'fr', 'es', 'de'].includes(value) ? value : 'en';
    root.lang = language;
    const selected = copy[language] || copy.en;
    const docs = root.dataset.page === 'documentation';
    const title = docs ? `${({es:'Documentación',de:'Dokumentation'})[language] || 'Documentation'} — XRatio` : selected.title;
    const description = docs && ['es','de'].includes(language) ? ({es:'Guía de XRatio: instalación, proxy qBittorrent, HTTPS, simulación y solución de problemas.',de:'XRatio-Anleitung: Installation, qBittorrent-Proxy, HTTPS, Simulation und Fehlerbehebung.'})[language] : docs ? (language === 'fr' ? 'Guide XRatio : installation, proxy qBittorrent, HTTPS, simulation et dépannage.' : 'XRatio guide: installation, qBittorrent proxy, HTTPS, simulation and troubleshooting.') : selected.description;
    document.title = title;
    document.querySelector('meta[property="og:title"]').content = title;
    document.querySelector('meta[name="description"]').content = description;
    document.querySelector('meta[property="og:description"]').content = description;
    document.querySelectorAll('[data-en][data-fr]').forEach(element => { element.textContent = language === 'fr' ? element.dataset.fr : window.XRatioLocales?.[language]?.[element.dataset.en] || element.dataset.en; });
    document.querySelectorAll('[data-language]').forEach(button => button.setAttribute('aria-pressed', String(button.dataset.language === language)));
    document.querySelector('nav').setAttribute('aria-label', selected.navigation);
    document.querySelector('#language-select').value = language;
    document.querySelector('#language-select').setAttribute('aria-label', ({es:'Elegir idioma',de:'Sprache wählen'})[language] || selected.language);
    document.querySelector('.theme-switch')?.setAttribute('aria-label', selected.theme);
    document.querySelector('[data-open-preview]')?.setAttribute('aria-label', selected.enlarge);
    document.querySelector('[data-close-preview]')?.setAttribute('aria-label', selected.close);
    dialog?.setAttribute('aria-label', selected.preview);
    document.querySelectorAll('[data-doc]').forEach(link => { link.href = selected.doc; });
    document.querySelectorAll('[data-en-alt]').forEach(element => { element.alt = element.dataset[`${language}Alt`] || window.XRatioLocales?.[language]?.[element.dataset.enAlt] || element.dataset.enAlt; });
    document.querySelectorAll('[data-en-label]').forEach(element => { element.setAttribute('aria-label', element.dataset[`${language}Label`] || window.XRatioLocales?.[language]?.[element.dataset.enLabel] || element.dataset.enLabel); });
    updateImageLabel();
    if (persist) { try { localStorage.setItem('xratio-language', language); } catch {} }
    document.dispatchEvent(new Event('xratio-language-change'));
  }
  document.querySelector('#language-select').addEventListener('change', event => setLanguage(event.target.value));
  const navigation = document.querySelector('.navigation');
  const menuToggle = document.querySelector('.nav-toggle');
  function closeMenu() { navigation.classList.remove('menu-open'); menuToggle.setAttribute('aria-expanded', 'false'); }
  menuToggle.addEventListener('click', () => {
    const open = navigation.classList.toggle('menu-open');
    menuToggle.setAttribute('aria-expanded', String(open));
  });
  document.querySelectorAll('.nav-links a').forEach(link => link.addEventListener('click', closeMenu));
  document.addEventListener('keydown', event => { if (event.key === 'Escape' && navigation.classList.contains('menu-open')) { closeMenu(); menuToggle.focus(); } });
  document.addEventListener('click', event => { if (!navigation.contains(event.target)) closeMenu(); });
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
  const reveals = document.querySelectorAll('.reveal, .feature-heading, .feature-pair article, .desktop-note, .get-app, .demo-app, .tracker-card');
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
