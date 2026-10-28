/**
 * ECLIPSE - MAIN MENU CONTROLLER
 * Барлық батырмалар, модальді терезелер, руналық синтез және аудио жүйесі.
 */

// ==========================================================================
// 1. ДЫБЫС ЖҮЙЕСІ (WEB AUDIO API СИНТЕЗАТОРЫ)
// ==========================================================================
class SoundFX {
  constructor() {
    this.ctx = null;
    this.enabled = true;
    this.masterVol = 0.8;
    this.sfxVol = 0.9;
  }

  init() {
    if (!this.ctx) {
      const AudioCtx = window.AudioContext || window.webkitAudioContext;
      this.ctx = new AudioCtx();
    }
    if (this.ctx.state === 'suspended') {
      this.ctx.resume();
    }
  }

  playClick() {
    if (!this.enabled) return;
    this.init();
    const osc = this.ctx.createOscillator();
    const gain = this.ctx.createGain();
    
    osc.type = 'triangle';
    osc.frequency.setValueAtTime(220, this.ctx.currentTime);
    osc.frequency.exponentialRampToValueAtTime(90, this.ctx.currentTime + 0.08);

    const v = this.masterVol * this.sfxVol * 0.4;
    gain.gain.setValueAtTime(v, this.ctx.currentTime);
    gain.gain.exponentialRampToValueAtTime(0.001, this.ctx.currentTime + 0.08);

    osc.connect(gain);
    gain.connect(this.ctx.destination);
    osc.start();
    osc.stop(this.ctx.currentTime + 0.08);
  }

  playHover() {
    if (!this.enabled) return;
    this.init();
    const osc = this.ctx.createOscillator();
    const gain = this.ctx.createGain();

    osc.type = 'sine';
    osc.frequency.setValueAtTime(650, this.ctx.currentTime);
    osc.frequency.exponentialRampToValueAtTime(850, this.ctx.currentTime + 0.04);

    const v = this.masterVol * this.sfxVol * 0.12;
    gain.gain.setValueAtTime(v, this.ctx.currentTime);
    gain.gain.exponentialRampToValueAtTime(0.001, this.ctx.currentTime + 0.04);

    osc.connect(gain);
    gain.connect(this.ctx.destination);
    osc.start();
    osc.stop(this.ctx.currentTime + 0.04);
  }

  playMagic(baseFreq = 380) {
    if (!this.enabled) return;
    this.init();
    const freqs = [baseFreq, baseFreq * 1.25, baseFreq * 1.5, baseFreq * 2.0];
    
    freqs.forEach((f, i) => {
      const osc = this.ctx.createOscillator();
      const gain = this.ctx.createGain();

      osc.type = 'sine';
      osc.frequency.setValueAtTime(f, this.ctx.currentTime + i * 0.03);
      osc.frequency.exponentialRampToValueAtTime(f * 1.1, this.ctx.currentTime + 0.35);

      const v = (this.masterVol * this.sfxVol * 0.15) / (i + 1);
      gain.gain.setValueAtTime(0.001, this.ctx.currentTime + i * 0.03);
      gain.gain.linearRampToValueAtTime(v, this.ctx.currentTime + i * 0.03 + 0.05);
      gain.gain.exponentialRampToValueAtTime(0.001, this.ctx.currentTime + 0.4);

      osc.connect(gain);
      gain.connect(this.ctx.destination);
      osc.start(this.ctx.currentTime + i * 0.03);
      osc.stop(this.ctx.currentTime + 0.42);
    });
  }

  playGong() {
    if (!this.enabled) return;
    this.init();
    const osc1 = this.ctx.createOscillator();
    const osc2 = this.ctx.createOscillator();
    const gain = this.ctx.createGain();

    osc1.type = 'sawtooth';
    osc1.frequency.setValueAtTime(110, this.ctx.currentTime);
    osc1.frequency.exponentialRampToValueAtTime(55, this.ctx.currentTime + 1.2);

    osc2.type = 'sine';
    osc2.frequency.setValueAtTime(220, this.ctx.currentTime);
    osc2.frequency.exponentialRampToValueAtTime(110, this.ctx.currentTime + 1.0);

    const v = this.masterVol * this.sfxVol * 0.5;
    gain.gain.setValueAtTime(v, this.ctx.currentTime);
    gain.gain.exponentialRampToValueAtTime(0.001, this.ctx.currentTime + 1.2);

    osc1.connect(gain);
    osc2.connect(gain);
    gain.connect(this.ctx.destination);

    osc1.start();
    osc2.start();
    osc1.stop(this.ctx.currentTime + 1.2);
    osc2.stop(this.ctx.currentTime + 1.2);
  }
}

const sfx = new SoundFX();

// ==========================================================================
// 2. ДЕРЕКТЕР ҚОРЫ (LORE & STATS DATA)
// ==========================================================================
const zoneData = {
  player: {
    title: "ҚАРАҢҒЫ БАТЫР АРЕНАСЫ",
    desc: "Ойыншының бастапқы қасиетті алаңы. Қараңғы батыр қанды орағымен гоблиндер армиясына қарсы тұруға сақадай сай.",
    stats: "Денсаулық (HP): 100/100 | Шабуыл күші: 25 | Жылдамдық: 5.0 | Қабілет: 'Көлеңкелі жарылыс'"
  },
  "main-area": {
    title: "БАСТЫ ҰРЫС АЙМАҒЫ",
    desc: "Аренаның негізгі тас шеңбері. Мұнда барлық жаулар мен құбыжықтар әр бағыттан батырды қоршауға алады.",
    stats: "Қауіп деңгейі: Орташа | Сыйлықтар: XP Кристалдары және Ежелгі руналар"
  },
  melee: {
    title: "ҚАНҚҰМАР ГОБЛИНДЕР СПАВН АЙМАҒЫ",
    desc: "Жақыннан топтасып шабуылдайтын кіші гоблиндер. Жылдам қимылдап, топпен қоршауға тырысады.",
    stats: "HP: 40 | Зақым: 10 | Жылдамдық: 3.5 | Әлсіздік: Аймақтық от сиқыры"
  },
  ranged: {
    title: "МЕРГЕН ГОБЛИНДЕР СПАВН АЙМАҒЫ",
    desc: "Алыстан улы жебелер ататын гоблин мергендері. Батырды қашықтықтан атқылап мазалайды.",
    stats: "HP: 30 | Оқ зақымы: 15 | Қашықтық: 9 метр | Әлсіздік: Шапшаң соққылар"
  },
  boss: {
    title: "ГОБЛИНДЕР ПАТШАСЫ (БАС БОСС)",
    desc: "Тақ үстінде масаттанған алып Гоблиндер Патшасы. Жойқын тас балғасымен тұтас аренаны сілкіндіреді!",
    stats: "HP: 1,500 | Зақым: 50 | Фазалар: 3 түрлі кезең | Олжа: Аңызға айналған 'Күн Тұтылу' тәжі"
  },
  elite: {
    title: "ЭЛИТАЛЫҚ ГОБЛИН (ЧЕМПИОН)",
    desc: "Патшаның бас оққағары. Қалың сауыт киген, шабуылдары ауыр және батырдың соққыларына төзімді.",
    stats: "HP: 450 | Зақым: 28 | Қабілет: Ауыр найза екпіні | Олжа: Руналық сиқыр тасы"
  }
};

const runeData = {
  fire: {
    title: "ОТ РУНАСЫ (ҚЫЗЫЛ)",
    desc: "Негізгі табиғи элемент. Қылышқа жалын шашып, қарсыластарды өртейді.",
    stats: "Жалын зақымы: +15% | Күйдіру әсері: Секундына 5 зақым"
  },
  water: {
    title: "СУ / МҰЗ РУНАСЫ (КӨК)",
    desc: "Мұз бен су элементі. Қарсыластың қанын мұздатып, қимылын баяулатады.",
    stats: "Баяулату: -25% жау жылдамдығы | Мұздату мүмкіндігі: 20%"
  },
  nature: {
    title: "ТАБИҒАТ / ҚАЛҚАН РУНАСЫ (ЖАСЫЛ)",
    desc: "Жер мен тіршілік элементі. Батырдың денсаулығын қалпына келтіріп, қорғаныс қалқанын түзеді.",
    stats: "Қорғаныс қалқаны: +30 | Әр секунд сайын 5 HP қалпына келтіру"
  },
  void: {
    title: "БОС КЕҢІСТІК СОҚҚЫСЫ (VOID STRIKE)",
    desc: "От + Мұз руналарының синтезі. Кеңістікті қақ жарып, күлгін қараңғы энергия толқынын таратады.",
    stats: "Аймақтық зақым: 85 | Жауларды орталыққа тартып жұту әсері | Қайта жүктеу: 8 сек"
  },
  blood: {
    title: "ҚАНДЫ ОРАҚ (BLOOD HARVEST)",
    desc: "От + Табиғат синтезі. Батырдың орағы жаулардың қанын сорып, өмірлік күшін толтырады.",
    stats: "Зақым: 60 | Өмір сүру (Lifesteal): Келтірілген зақымның 35%-ын батырға қайтару"
  },
  soul: {
    title: "ЖАН СІҢІРУ (SOUL SIPHON)",
    desc: "Мұз + Табиғат синтезі. Құбыжықтардың рухын сіңіріп, оларды қатып қалған тасқа айналдырады.",
    stats: "Қарсыласты қатыру: 2.5 сек | Кристалл табу мүмкіндігі: 2x есе артады"
  }
};

const slotData = {
  1: { title: "ҚАРАҢҒЫ ҚЫЛЫШ", desc: "Батырдың негізгі қаруы. Жылдам әрі өткір.", stats: "Зақым: 25 | Жылдамдық: 1.2 сек" },
  2: { title: "ОТТЫ ШАР", desc: "Жалынды сиқырлы снаряд.", stats: "Зақым: 45 | Жарылу радиусы: 2.5 метр" },
  3: { title: "МҰЗ ҚАЛҚАНЫ", desc: "Қарсыластардың соққыларын тойтаратын қорған.", stats: "+50 Қорғаныс | 3 секундқа мұздату" },
  4: { title: "ӨЛІМ ОРАҒЫ", desc: "Айналмалы жойқын соққы.", stats: "Зақым: 70 | 360° барлық жауларға" }
};

// ==========================================================================
// 3. UI ИНИЦИАЛИЗАЦИЯСЫ ЖӘНЕ БАТЫРМАЛАР
// ==========================================================================
document.addEventListener('DOMContentLoaded', () => {
  initParticleFX();
  initEventListeners();
});

function initEventListeners() {
  // Дыбысты қосу/өшіру батырмасы
  const soundBtn = document.getElementById('soundToggleBtn');
  const soundIcon = document.getElementById('soundIcon');
  soundBtn.addEventListener('click', () => {
    sfx.enabled = !sfx.enabled;
    soundIcon.textContent = sfx.enabled ? '🔊' : '🔇';
    if (sfx.enabled) sfx.playClick();
  });

  // Сол жақтағы негізгі мәзір батырмалары
  document.getElementById('btnRunSim').addEventListener('click', () => {
    sfx.playGong();
    startSimulation();
  });

  document.getElementById('btnLoadProgress').addEventListener('click', () => {
    sfx.playClick();
    openModal('modalLoadProgress');
  });

  document.getElementById('btnSettings').addEventListener('click', () => {
    sfx.playClick();
    openModal('modalSettings');
  });

  document.getElementById('btnQuit').addEventListener('click', () => {
    sfx.playClick();
    openModal('modalQuit');
  });

  // Жол картасы батырмасы
  document.getElementById('scopePanel').addEventListener('click', () => {
    sfx.playClick();
    showInfoModal(
      "ЖОБА ЖОЛ КАРТАСЫ (ROADMAP)",
      "• 1-АПТА: Негізгі шайқас жүйесі және Гоблиндер механикасы\n• 2-АПТА: Басқару динамикасы, секіру мен әрекеттер\n• 3-АПТА: Жаңа жаулар мен Марапат жүйесі\n• 4-АПТА: Босс шайқасы және Даму сатысы",
      "Ағымдағы нұсқа: MVP Protocol 1-4 | Дайындық: 100%"
    );
  });

  // Модальді терезелерді жабу (✕ және data-close)
  document.querySelectorAll('[data-close]').forEach(btn => {
    btn.addEventListener('click', (e) => {
      sfx.playClick();
      const modalId = e.target.getAttribute('data-close');
      closeModal(modalId);
    });
  });

  // Backdrop басу арқылы жабу
  const backdrop = document.getElementById('modalBackdrop');
  backdrop.addEventListener('click', (e) => {
    if (e.target === backdrop) {
      sfx.playClick();
      closeAllModals();
    }
  });

  // Шығуды растау
  document.getElementById('btnConfirmQuit').addEventListener('click', () => {
    sfx.playClick();
    closeAllModals();
    alert("Ойын тоқтатылды. Ойыннан шықтыңыз!");
  });

  // Сақтау слотын таңдау
  document.querySelectorAll('.save-slot-card').forEach(card => {
    card.addEventListener('click', () => {
      sfx.playClick();
      document.querySelectorAll('.save-slot-card').forEach(c => c.classList.remove('selected'));
      card.classList.add('selected');
    });
  });

  // Таңдалған сақтауды жүктеу
  document.getElementById('btnLoadSelectedSave').addEventListener('click', () => {
    closeAllModals();
    sfx.playGong();
    startSimulation();
  });

  // Баптаулар слайдерлері
  const masterVol = document.getElementById('masterVolume');
  const masterVal = document.getElementById('masterVolVal');
  masterVol.addEventListener('input', (e) => {
    masterVal.textContent = e.target.value + '%';
    sfx.masterVol = e.target.value / 100;
  });

  const sfxVol = document.getElementById('sfxVolume');
  const sfxVal = document.getElementById('sfxVolVal');
  sfxVol.addEventListener('input', (e) => {
    sfxVal.textContent = e.target.value + '%';
    sfx.sfxVol = e.target.value / 100;
  });

  const musicVol = document.getElementById('musicVolume');
  const musicVal = document.getElementById('musicVolVal');
  musicVol.addEventListener('input', (e) => {
    musicVal.textContent = e.target.value + '%';
  });

  // Толық экран батырмасы
  const toggleFs = document.getElementById('toggleFullscreen');
  toggleFs.addEventListener('click', () => {
    sfx.playClick();
    if (!document.fullscreenElement) {
      document.documentElement.requestFullscreen().catch(() => {});
      toggleFs.textContent = 'ҚОСУЛЫ';
      toggleFs.classList.add('active');
    } else {
      document.exitFullscreen().catch(() => {});
      toggleFs.textContent = 'ӨШІРУЛІ';
      toggleFs.classList.remove('active');
    }
  });

  // Интерактивті Аймақтар (Arena Hotspots)
  const tooltip = document.getElementById('tooltip');
  const ttTitle = document.getElementById('tooltipTitle');
  const ttDesc = document.getElementById('tooltipDesc');
  const ttStats = document.getElementById('tooltipStats');

  document.querySelectorAll('.arena-hotspot').forEach(spot => {
    const target = spot.getAttribute('data-target');
    const data = zoneData[target];

    spot.addEventListener('mouseenter', (e) => {
      sfx.playHover();
      if (data) {
        ttTitle.textContent = data.title;
        ttDesc.textContent = data.desc;
        ttStats.textContent = data.stats;
        tooltip.classList.add('visible');
      }
    });

    spot.addEventListener('mousemove', (e) => {
      tooltip.style.left = (e.clientX + 16) + 'px';
      tooltip.style.top = (e.clientY + 16) + 'px';
    });

    spot.addEventListener('mouseleave', () => {
      tooltip.classList.remove('visible');
    });

    spot.addEventListener('click', () => {
      sfx.playMagic(420);
      tooltip.classList.remove('visible');
      if (data) {
        showInfoModal(data.title, data.desc, data.stats);
      }
    });
  });

  // Оң жақтағы Руналар
  document.querySelectorAll('.rune-card').forEach(card => {
    const runeKey = card.getAttribute('data-rune');
    const data = runeData[runeKey];

    card.addEventListener('mouseenter', () => sfx.playHover());
    card.addEventListener('click', (e) => {
      sfx.playMagic(runeKey === 'fire' ? 320 : runeKey === 'water' ? 440 : 360);
      spawnBurst(e.clientX, e.clientY, runeKey === 'fire' ? '#ef4444' : runeKey === 'water' ? '#38bdf8' : '#10b981');
      if (data) showInfoModal(data.title, data.desc, data.stats);
    });
  });

  // Оң жақтағы Біріккен Сиқырлар (Fusions)
  document.querySelectorAll('.fusion-card').forEach(card => {
    const fusionKey = card.getAttribute('data-fusion');
    const data = runeData[fusionKey];

    card.addEventListener('mouseenter', () => sfx.playHover());
    card.addEventListener('click', (e) => {
      sfx.playMagic(fusionKey === 'void' ? 520 : fusionKey === 'blood' ? 290 : 480);
      spawnBurst(e.clientX, e.clientY, fusionKey === 'void' ? '#c084fc' : fusionKey === 'blood' ? '#ef4444' : '#10b981');
      if (data) showInfoModal(data.title, data.desc, data.stats);
    });
  });

  // Төменгі Слоттар [1] [2] [3] [4]
  document.querySelectorAll('.slot-item').forEach(slot => {
    const idx = slot.getAttribute('data-slot');
    const data = slotData[idx];

    slot.addEventListener('mouseenter', () => sfx.playHover());
    slot.addEventListener('click', () => {
      sfx.playClick();
      if (data) {
        showInfoModal(`СЛОТ [${idx}]: ${data.title}`, data.desc, data.stats);
      }
    });
  });

  // Барлық батырмаларға Hover дыбысы
  document.querySelectorAll('button').forEach(btn => {
    btn.addEventListener('mouseenter', () => sfx.playHover());
  });

  // Симуляцияны тоқтату батырмасы
  document.getElementById('btnCancelSim').addEventListener('click', () => {
    cancelSimulation();
  });
}

// ==========================================================================
// 4. МОДАЛЬДІ ТЕРЕЗЕЛЕРДІ БАСҚАРУ
// ==========================================================================
function openModal(modalId) {
  const backdrop = document.getElementById('modalBackdrop');
  const modal = document.getElementById(modalId);

  document.querySelectorAll('.modal-window').forEach(m => m.classList.remove('active'));
  backdrop.classList.add('open');
  modal.classList.add('active');
}

function closeModal(modalId) {
  const modal = document.getElementById(modalId);
  if (modal) modal.classList.remove('active');
  const backdrop = document.getElementById('modalBackdrop');
  backdrop.classList.remove('open');
}

function closeAllModals() {
  document.querySelectorAll('.modal-window').forEach(m => m.classList.remove('active'));
  document.getElementById('modalBackdrop').classList.remove('open');
}

function showInfoModal(title, desc, stats) {
  document.getElementById('infoModalTitle').textContent = title;
  document.getElementById('infoModalBody').textContent = desc;
  document.getElementById('infoModalStats').textContent = stats;
  openModal('modalInfo');
}

// ==========================================================================
// 5. СИМУЛЯЦИЯНЫ БАСТАУ ПРОЦЕСІ
// ==========================================================================
let simInterval = null;

function startSimulation() {
  const overlay = document.getElementById('simulationOverlay');
  const fill = document.getElementById('simProgressFill');
  const percentText = document.getElementById('simPercent');
  
  overlay.classList.add('active');
  let progress = 0;
  fill.style.width = '0%';
  percentText.textContent = '0%';

  if (simInterval) clearInterval(simInterval);

  simInterval = setInterval(() => {
    progress += Math.floor(Math.random() * 8) + 4;
    if (progress > 100) progress = 100;

    fill.style.width = progress + '%';
    percentText.textContent = progress + '%';

    if (progress >= 100) {
      clearInterval(simInterval);
      setTimeout(() => {
        alert("СИМУЛЯЦИЯ ІСКЕ ҚОСЫЛДЫ!\nГоблиндер толқыны басталды! Ойын сәтті жүктелді.");
        overlay.classList.remove('active');
      }, 400);
    }
  }, 90);
}

function cancelSimulation() {
  if (simInterval) clearInterval(simInterval);
  document.getElementById('simulationOverlay').classList.remove('active');
  sfx.playClick();
}

// ==========================================================================
// 6. БӨЛШЕКТЕР КЕНЕБІ (PARTICLE FX CANVAS)
// ==========================================================================
let canvas, ctx;
let particles = [];

function initParticleFX() {
  canvas = document.getElementById('fxCanvas');
  ctx = canvas.getContext('2d');

  function resize() {
    canvas.width = window.innerWidth;
    canvas.height = window.innerHeight;
  }
  window.addEventListener('resize', resize);
  resize();

  // Атмосфералық сиқырлы шаң түйіршіктері
  for (let i = 0; i < 45; i++) {
    particles.push({
      x: Math.random() * canvas.width,
      y: Math.random() * canvas.height,
      radius: Math.random() * 2 + 0.5,
      speedY: -Math.random() * 0.4 - 0.1,
      speedX: (Math.random() - 0.5) * 0.2,
      opacity: Math.random() * 0.5 + 0.2,
      color: Math.random() > 0.5 ? '#38bdf8' : '#f59e0b'
    });
  }

  animateParticles();
}

function animateParticles() {
  ctx.clearRect(0, 0, canvas.width, canvas.height);

  for (let i = 0; i < particles.length; i++) {
    const p = particles[i];
    p.y += p.speedY;
    p.x += p.speedX;

    if (p.isBurst) {
      p.speedX *= 0.94;
      p.speedY *= 0.94;
      p.opacity -= 0.02;
      if (p.opacity <= 0) {
        particles.splice(i, 1);
        i--;
        continue;
      }
    } else {
      if (p.y < 0) {
        p.y = canvas.height;
        p.x = Math.random() * canvas.width;
      }
    }

    ctx.save();
    ctx.beginPath();
    ctx.arc(p.x, p.y, p.radius, 0, Math.PI * 2);
    ctx.fillStyle = p.color;
    ctx.globalAlpha = p.opacity;
    ctx.shadowBlur = p.isBurst ? 10 : 4;
    ctx.shadowColor = p.color;
    ctx.fill();
    ctx.restore();
  }

  requestAnimationFrame(animateParticles);
}

function spawnBurst(x, y, color) {
  for (let i = 0; i < 24; i++) {
    const angle = Math.random() * Math.PI * 2;
    const speed = Math.random() * 4 + 1.5;
    particles.push({
      x: x,
      y: y,
      radius: Math.random() * 3 + 1,
      speedX: Math.cos(angle) * speed,
      speedY: Math.sin(angle) * speed,
      opacity: 1,
      color: color,
      isBurst: true
    });
  }
}
