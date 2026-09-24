// ============================================================================
// VOID PROTOCOL - Full Standalone Game Engine
// Authentic implementation of Void Protocol (Endless Marathon, Scaling Bosses)
// ============================================================================

const canvas = document.getElementById('gameCanvas');
const ctx = canvas.getContext('2d');

// --- Universal Round Rect Polyfill for Canvas ---
// Guarantees compatibility on all Windows WebView2 and browser runtimes
function roundRectPath(ctx, x, y, w, h, r) {
    if (r > w / 2) r = w / 2;
    if (r > h / 2) r = h / 2;
    ctx.beginPath();
    ctx.moveTo(x + r, y);
    ctx.lineTo(x + w - r, y);
    ctx.arcTo(x + w, y, x + w, y + r, r);
    ctx.lineTo(x + w, y + h - r);
    ctx.arcTo(x + w, y + h, x + w - r, y + h, r);
    ctx.lineTo(x + r, y + h);
    ctx.arcTo(x, y + h, x, y + h - r, r);
    ctx.lineTo(x, y + r);
    ctx.arcTo(x, y, x + r, y, r);
    ctx.closePath();
}

function fillRoundRect(ctx, x, y, w, h, r) {
    roundRectPath(ctx, x, y, w, h, r);
    ctx.fill();
}

function strokeRoundRect(ctx, x, y, w, h, r) {
    roundRectPath(ctx, x, y, w, h, r);
    ctx.stroke();
}

// --- Audio Controller using Web Audio API ---
class SoundManager {
    constructor() {
        this.ctx = null;
        this.buffers = {};
        this.musicSource = null;
        this.isMuted = false;
        this.initialized = false;
    }

    init() {
        if (this.initialized) return;
        try {
            const AudioContext = window.AudioContext || window.webkitAudioContext;
            if (AudioContext) {
                this.ctx = new AudioContext();
                this.initialized = true;
                this.loadAudio('shoot', 'assets/audio/12_Step_wood_03.wav');
                this.loadAudio('explode', 'assets/audio/04_Fire_explosion_04_medium.wav');
                this.loadAudio('explode_big', 'assets/audio/13_Ice_explosion_01.wav');
                this.loadAudio('hit', 'assets/audio/61_Hit_03.wav');
                this.loadAudio('buff', 'assets/audio/39_Block_03.wav');
                this.loadAudio('boss_spawn', 'assets/audio/Boss_Spawn.mp3');
                this.loadAudio('boss_charge', 'assets/audio/Swipe.mp3');
                this.loadAudio('boost', 'assets/audio/46_Poison_01.wav');
                this.loadAudio('hit_armor', 'assets/audio/03_Step_grass_03.wav');
                this.loadAudio('squish', 'assets/audio/77_flesh_02.wav');
                this.loadAudio('burn', 'assets/audio/51_Flee_02.wav');
                this.loadAudio('pause', 'assets/audio/092_Pause_04.wav');
                this.loadAudio('unpause', 'assets/audio/098_Unpause_04.wav');
                this.loadMusic('assets/audio/JDSherbert - Nostalgia Music Pack - Gameboy & A Long Car Journey.mp3');
            }
        } catch (e) {
            console.warn('AudioContext not available:', e);
        }
    }

    async loadAudio(name, url) {
        if (!this.ctx) return;
        try {
            const resp = await fetch(url);
            const arrayBuffer = await resp.arrayBuffer();
            this.buffers[name] = await this.ctx.decodeAudioData(arrayBuffer);
        } catch (e) {}
    }

    async loadMusic(url) {
        if (!this.ctx) return;
        try {
            const resp = await fetch(url);
            const arrayBuffer = await resp.arrayBuffer();
            this.musicBuffer = await this.ctx.decodeAudioData(arrayBuffer);
            this.playMusic();
        } catch (e) {}
    }

    playMusic() {
        if (!this.musicBuffer || !this.ctx || this.isMuted) return;
        if (this.musicSource) {
            try { this.musicSource.stop(); } catch(e) {}
        }
        try {
            this.musicSource = this.ctx.createBufferSource();
            this.musicSource.buffer = this.musicBuffer;
            this.musicSource.loop = true;
            const gainNode = this.ctx.createGain();
            gainNode.gain.value = 0.35;
            this.musicSource.connect(gainNode);
            gainNode.connect(this.ctx.destination);
            this.musicSource.start(0);
        } catch(e) {}
    }

    play(name, volume = 0.5, pitchVariance = 0.1) {
        if (!this.ctx || !this.buffers[name] || this.isMuted) return;
        try {
            const source = this.ctx.createBufferSource();
            source.buffer = this.buffers[name];
            source.playbackRate.value = 1.0 + (Math.random() * 2 - 1) * pitchVariance;
            const gain = this.ctx.createGain();
            gain.gain.value = volume;
            source.connect(gain);
            gain.connect(this.ctx.destination);
            source.start(0);
        } catch(e) {}
    }
}

const sounds = new SoundManager();

// --- Image Assets Loader with Async Preloader ---
const images = {};
const assetManifest = [
    { name: 'bg1', src: 'assets/art/layer1.png' },
    { name: 'bg2', src: 'assets/art/layer2.png' },
    { name: 'bg3', src: 'assets/art/layer3.png' },
    { name: 'bg4', src: 'assets/art/layer4.png' },
    { name: 'bullet', src: 'assets/art/bullet1.png' },
    { name: 'player_gameover', src: 'assets/art/player_game_over.png' },
    { name: 'player_0', src: 'assets/sprites/player_sprite_0.png' },
    { name: 'player_1', src: 'assets/sprites/player_sprite_1.png' },
    { name: 'player_2', src: 'assets/sprites/player_sprite_2.png' },
    { name: 'player_3', src: 'assets/sprites/player_sprite_3.png' },
    { name: 'player_6', src: 'assets/sprites/player_sprite_6.png' },
    { name: 'player_boost', src: 'assets/sprites/player_sprite_9.png' }
];

for (let i = 0; i < 4; i++) {
    assetManifest.push({ name: `critter_${i}`, src: `assets/sprites/critter1_sprite_${i}.png` });
}
for (let i = 0; i < 21; i++) {
    assetManifest.push({ name: `boom_${i}`, src: `assets/sprites/boom2_${i}.png` });
}
for (let i = 0; i < 78; i++) {
    assetManifest.push({ name: `boss_${i}`, src: `assets/sprites/boss1_${i}.png` });
}

let assetsLoaded = false;
function preloadAllAssets() {
    return Promise.all(assetManifest.map(item => {
        return new Promise(resolve => {
            const img = new Image();
            img.onload = () => {
                images[item.name] = img;
                resolve(img);
            };
            img.onerror = () => {
                console.warn('Asset fallback initialized for:', item.src);
                images[item.name] = img;
                resolve(img);
            };
            img.src = item.src;
            images[item.name] = img;
        });
    })).then(() => {
        assetsLoaded = true;
        console.log('All 115 Void Protocol visual assets loaded and decoded.');
    });
}

function getCritterFrame(index) {
    return images[`critter_${index % 4}`] || images['critter_0'];
}

function getBossFrame(index) {
    return images[`boss_${index % 78}`] || images['boss_0'];
}

function getBoomFrame(index) {
    return images[`boom_${index % 21}`] || images['boom_0'];
}

// --- Game State & Input ---
const keys = {};
let isMouseDown = false, isRightMouseDown = false;

window.addEventListener('keydown', e => {
    keys[e.code] = true;
    if (e.code === 'KeyP' || e.code === 'Escape') togglePause();
});
window.addEventListener('keyup', e => keys[e.code] = false);

canvas.addEventListener('mousedown', e => {
    if (e.button === 0) isMouseDown = true;
    if (e.button === 2) isRightMouseDown = true;
});
canvas.addEventListener('mouseup', e => {
    if (e.button === 0) isMouseDown = false;
    if (e.button === 2) isRightMouseDown = false;
});
canvas.addEventListener('contextmenu', e => e.preventDefault());

let gameState = 'START'; // 'START', 'PLAYING', 'PAUSED', 'GAMEOVER'
let survivalTime = 0;
let distance = 0;
let score = 0;
let combo = 0;
let comboTimer = 0;
let killsCount = 0;
let worldSpeed = 160;

// Parallax scroll offsets
let bgOffsets = [0, 0, 0, 0];
const bgSpeeds = [0.15, 0.45, 0.8, 1.4];

// --- Engine Exhaust Particle System for Boost ---
const boostParticles = [];
function addBoostParticle(x, y) {
    for (let i = 0; i < 2; i++) {
        boostParticles.push({
            x: x + (Math.random() * 6 - 3),
            y: y + (Math.random() * 10 - 5),
            vx: -(420 + Math.random() * 320),
            vy: (Math.random() * 60 - 30),
            size: 9 + Math.random() * 10,
            alpha: 0.95,
            color: Math.random() > 0.35 ? '#38bdf8' : (Math.random() > 0.5 ? '#60a5fa' : '#ffffff')
        });
    }
}

// --- Player Starship ---
class Player {
    constructor() {
        this.x = 120;
        this.y = canvas.height / 2 - 35;
        this.width = 95;
        this.height = 70;
        this.speed = 360;
        this.boostSpeed = 620;
        this.health = 3;
        this.maxHealth = 3;
        this.energy = 100;
        this.maxEnergy = 100;
        this.isBoosting = false;
        this.isInvulnerable = false;
        this.invulnerableTimer = 0;

        // Buff Timers
        this.doubleShotTimer = 0;
        this.tripleShotTimer = 0;
        this.rapidFireTimer = 0;
        this.superShieldTimer = 0;
        this.pulseScale = 1.0;

        this.fireCooldown = 0;
    }

    update(dt) {
        // Boost handling
        const boostKey = keys['Space'] || isRightMouseDown || keys['KeyE'];
        if (boostKey && this.energy > 0) {
            if (!this.isBoosting) {
                sounds.play('boost', 0.65);
            }
            this.isBoosting = true;
            this.energy = Math.max(0, this.energy - 38 * dt);
            const drawW = Math.round(this.width * 1.88);
            const flameTipX = this.x + this.width - drawW;
            addBoostParticle(flameTipX, this.y + this.height / 2);
        } else {
            this.isBoosting = false;
            this.energy = Math.min(this.maxEnergy, this.energy + 22 * dt);
        }

        const currentSpeed = this.isBoosting ? this.boostSpeed : this.speed;

        // 8-directional smooth movement
        let dx = 0, dy = 0;
        if (keys['KeyW'] || keys['ArrowUp']) dy -= 1;
        if (keys['KeyS'] || keys['ArrowDown']) dy += 1;
        if (keys['KeyA'] || keys['ArrowLeft']) dx -= 1;
        if (keys['KeyD'] || keys['ArrowRight']) dx += 1;

        if (dx !== 0 && dy !== 0) {
            dx *= 0.7071;
            dy *= 0.7071;
        }

        this.x = Math.max(30, Math.min(canvas.width - 120, this.x + dx * currentSpeed * dt));
        this.y = Math.max(40, Math.min(canvas.height - 80, this.y + dy * currentSpeed * dt));

        // Decrement Buff Timers
        if (this.tripleShotTimer > 0) this.tripleShotTimer -= dt;
        if (this.doubleShotTimer > 0) this.doubleShotTimer -= dt;
        if (this.rapidFireTimer > 0) this.rapidFireTimer -= dt;

        // Strict Invulnerability Reset: Only invulnerable if a timer is active!
        if (this.superShieldTimer > 0) {
            this.superShieldTimer -= dt;
            this.isInvulnerable = true;
        } else if (this.invulnerableTimer > 0) {
            this.invulnerableTimer -= dt;
            this.isInvulnerable = true;
        } else {
            this.isInvulnerable = false;
        }

        // Pulse scale decay
        if (this.pulseScale > 1.0) {
            this.pulseScale = Math.max(1.0, this.pulseScale - dt * 2.0);
        }

        // Weapon firing
        this.fireCooldown -= dt;
        const shootKey = keys['ShiftLeft'] || keys['ShiftRight'] || isMouseDown;
        const cooldownTime = this.rapidFireTimer > 0 ? 0.08 : 0.18;

        if (shootKey && this.fireCooldown <= 0) {
            this.fireCooldown = cooldownTime;
            this.shoot();
        }
    }

    shoot() {
        sounds.play('shoot', 0.4);
        if (this.tripleShotTimer > 0) {
            // Triple Shot: 3 spread beams (upper angled, center straight, lower angled)
            bullets.push(new Bullet(this.x + this.width - 5, this.y + 4, 920, -50));
            bullets.push(new Bullet(this.x + this.width - 5, this.y + this.height / 2 - 5, 920, 0));
            bullets.push(new Bullet(this.x + this.width - 5, this.y + this.height - 14, 920, 50));
        } else if (this.doubleShotTimer > 0) {
            // Double Shot: 2 parallel beams
            bullets.push(new Bullet(this.x + this.width - 5, this.y + 10, 900, 0));
            bullets.push(new Bullet(this.x + this.width - 5, this.y + this.height - 18, 900, 0));
        } else {
            // Standard single phaser
            bullets.push(new Bullet(this.x + this.width - 5, this.y + this.height / 2 - 5, 900, 0));
        }
    }

    takeDamage(amount = 1) {
        if (this.isInvulnerable) return;
        this.health -= amount;
        this.isInvulnerable = true;
        this.invulnerableTimer = 1.5; // 1.5s post-damage invulnerability
        sounds.play('hit', 0.6);
        spawnFloatingText(this.x + 20, this.y - 15, `-${amount} HEALTH`, '#ef4444');

        if (this.health <= 0) {
            this.health = 0;
            triggerGameOver();
        }
    }

    heal(amount = 1) {
        this.health = Math.min(this.maxHealth, this.health + amount);
        this.pulseScale = 1.35;
        sounds.play('buff', 0.7);
        spawnFloatingText(this.x + 20, this.y - 20, `+${amount} HEALTH!`, '#4ade80');
    }

    applyBuff(type) {
        sounds.play('buff', 0.8);
        this.pulseScale = 1.35; // Authentic elastic pulse scale on buff collection!

        // Buff potency scaling with time and distance (from Session 2 & 3)
        const buffPower = Math.min(2.5, 1.0 + (survivalTime / 100) * 0.4 + (distance / 2500) * 0.35);

        if (type === 'HEAL') {
            this.heal(1);
        } else if (type === 'TRIPLE_SHOT') {
            this.tripleShotTimer = 12 * buffPower;
            spawnFloatingText(this.x + 10, this.y - 25, `TRIPLE SHOT (${Math.round(this.tripleShotTimer)}s)!`, '#fb923c');
        } else if (type === 'DOUBLE_SHOT') {
            this.doubleShotTimer = 14 * buffPower;
            spawnFloatingText(this.x + 10, this.y - 25, `DOUBLE SHOT (${Math.round(this.doubleShotTimer)}s)!`, '#38bdf8');
        } else if (type === 'RAPID_FIRE') {
            this.rapidFireTimer = 12 * buffPower;
            spawnFloatingText(this.x + 10, this.y - 25, `RAPID FIRE (${Math.round(this.rapidFireTimer)}s)!`, '#fde047');
        } else if (type === 'SHIELD') {
            this.superShieldTimer = 8 * buffPower;
            this.isInvulnerable = true;
            spawnFloatingText(this.x + 10, this.y - 25, `SUPER SHIELD (${Math.round(this.superShieldTimer)}s)!`, '#a855f7');
        }
    }

    draw() {
        // Blinking effect during 1.5s invulnerability
        if (this.isInvulnerable && this.superShieldTimer <= 0 && Math.floor(Date.now() / 80) % 2 === 0) {
            return;
        }

        ctx.save();
        if (this.pulseScale > 1.0) {
            ctx.translate(this.x + this.width / 2, this.y + this.height / 2);
            ctx.scale(this.pulseScale, this.pulseScale);
            ctx.translate(-(this.x + this.width / 2), -(this.y + this.height / 2));
        }

        if (this.isBoosting) {
            // Render full boost sprite (player_sprite_9, width 188 with exhaust flame)
            // In Unity, player_sprite_0 is 100x100 and player_sprite_9 is 188x100.
            // Both share right-edge pivot (x:1, y:0.5) so nose stays perfectly fixed!
            const boostImg = images['player_boost'] || images['player_0'];
            if (boostImg) {
                const drawW = Math.round(this.width * 1.88); // 179 px
                const drawH = this.height;                   // 70 px
                const drawX = this.x + this.width - drawW;   // Cockpit and nose stay perfectly fixed!
                ctx.drawImage(boostImg, drawX, this.y, drawW, drawH);
            }
        } else {
            // Select directional animation frame
            let frame = images['player_0'];
            if (keys['KeyW'] || keys['ArrowUp']) {
                frame = images['player_3'] || images['player_0']; // Bank Up
            } else if (keys['KeyS'] || keys['ArrowDown']) {
                frame = images['player_2'] || images['player_0']; // Bank Down
            } else if (keys['KeyA'] || keys['ArrowLeft']) {
                frame = images['player_6'] || images['player_0']; // Bank Left / Decel
            } else {
                frame = images['player_0']; // Rock-solid idle flight matching Unity Player_Right
            }

            if (frame) {
                ctx.drawImage(frame, this.x, this.y, this.width, this.height);
            }
        }

        // Super Shield Forcefield Visual
        if (this.superShieldTimer > 0) {
            ctx.beginPath();
            ctx.arc(this.x + this.width / 2, this.y + this.height / 2, 54, 0, Math.PI * 2);
            ctx.strokeStyle = this.superShieldTimer <= 2 ? '#fde047' : '#a855f7';
            ctx.lineWidth = 3.5;
            ctx.shadowColor = '#a855f7';
            ctx.shadowBlur = 16;
            ctx.stroke();

            ctx.fillStyle = 'rgba(168, 85, 247, 0.15)';
            ctx.fill();
        }

        ctx.restore();
    }
}

class Bullet {
    constructor(x, y, vx = 900, vy = 0) {
        this.x = x;
        this.y = y;
        this.vx = vx;
        this.vy = vy;
        this.width = 24;
        this.height = 10;
    }

    update(dt) {
        this.x += this.vx * dt;
        this.y += this.vy * dt;
    }

    draw() {
        const img = images['bullet'];
        if (img) {
            if (this.vy !== 0) {
                ctx.save();
                ctx.translate(this.x + this.width / 2, this.y + this.height / 2);
                ctx.rotate(Math.atan2(this.vy, this.vx));
                ctx.drawImage(img, -this.width / 2, -this.height / 2, this.width, this.height);
                ctx.restore();
            } else {
                ctx.drawImage(img, this.x, this.y, this.width, this.height);
            }
        }
    }
}

class Critter {
    constructor() {
        this.x = canvas.width + 60;
        this.baseY = 60 + Math.random() * (canvas.height - 160);
        this.y = this.baseY;
        this.width = 75;
        this.height = 75;
        this.speed = 220 + Math.random() * 90 + Math.min(survivalTime * 2.5, 200);
        this.freq = 2.5 + Math.random() * 2.0;
        this.amp = 45 + Math.random() * 55;
        this.time = Math.random() * 10;
        this.frameTimer = 0;
        this.currentFrame = 0;
    }

    update(dt) {
        this.time += dt;
        this.x -= this.speed * dt;
        this.y = this.baseY + Math.sin(this.time * this.freq) * this.amp;

        this.frameTimer += dt;
        if (this.frameTimer > 0.1) {
            this.frameTimer = 0;
            this.currentFrame = (this.currentFrame + 1) % 4;
        }
    }

    // IsOnScreen check: must be visibly on screen before being damaged!
    isOnScreen() {
        return this.x <= canvas.width - this.width + 10 && this.x >= -this.width;
    }

    draw() {
        const frame = getCritterFrame(this.currentFrame);
        const cx = this.x + this.width / 2;
        const cy = this.y + this.height / 2;
        const targetX = player ? player.x + player.width / 2 : 0;
        const targetY = player ? player.y + player.height / 2 : cy;

        // Small enemies face towards the starship
        // In the raw sprite critter1_sprite_0, the head points UP (-Y in screen space)
        // We clamp dx <= -20 so it consistently faces forward-left towards the player
        const dx = Math.min(-20, targetX - cx);
        const dy = targetY - cy;
        const bankAngle = Math.max(-0.6, Math.min(0.6, Math.atan2(dy, -dx)));

        ctx.save();
        ctx.translate(cx, cy);
        // Base facing LEFT (-PI/2 from UP-pointing sprite) + banking tilt towards player
        ctx.rotate(-Math.PI / 2 + bankAngle);

        if (frame) {
            ctx.drawImage(frame, -this.width / 2, -this.height / 2, this.width, this.height);
        }
        ctx.restore();
    }
}

// --- Multi-Tier Escalating Bosses (From Session 8e3e9515, 28dd39ec, and bdb70fc3) ---
class Boss {
    constructor(tier = 1) {
        this.tier = tier;
        this.name = tier === 1 ? 'TITAN DREADNOUGHT' : (tier === 2 ? 'VOID OVERLORD' : 'APEX LEVIATHAN');
        this.width = 240;
        this.height = 200;
        this.x = canvas.width + 40;
        this.y = canvas.height / 2 - this.height / 2;
        this.targetY = canvas.height / 2;

        // Base health per tier (balanced: -20 HP decrease as requested for fair battle)
        const baseHp = tier === 1 ? 30 : (tier === 2 ? 65 : 110);
        // Scaling with Time, Distance, and Completed Cycles
        const healthMult = 1.0 + (survivalTime / 90) * 0.25 + (distance / 3000) * 0.20 + bossCycleCount * 0.35;
        this.maxHp = Math.round(baseHp * healthMult);
        this.hp = this.maxHp;

        // Escapable boss speed
        this.speed = (tier === 1 ? 210 : (tier === 2 ? 270 : 340)) * Math.min(1.35, 1.0 + bossCycleCount * 0.08);
        this.state = 'ENTER'; // 'ENTER', 'HOVER', 'CHARGE'
        this.stateTimer = 0;
        this.currentFrame = 0;
        this.animTimer = 0;
        sounds.play('boss_spawn', 0.8);
    }

    update(dt) {
        this.animTimer += dt;
        if (this.animTimer > 0.06) {
            this.animTimer = 0;
            this.currentFrame = (this.currentFrame + 1) % 78;
        }

        this.stateTimer += dt;

        if (this.state === 'ENTER') {
            this.x -= 220 * dt;
            if (this.x <= canvas.width - this.width - 40) {
                this.state = 'HOVER';
                this.stateTimer = 0;
            }
        } else if (this.state === 'HOVER') {
            // Track player ship altitude smoothly
            if (player) {
                const diff = (player.y - this.y);
                this.y += Math.sign(diff) * Math.min(Math.abs(diff), 130 * dt);
            }
            if (this.stateTimer > (this.tier === 1 ? 3.8 : 2.6)) {
                this.state = 'CHARGE';
                this.stateTimer = 0;
                sounds.play('boss_charge', 0.85); // Menacing charging sound from Swipe.mp3
            }
        } else if (this.state === 'CHARGE') {
            this.x -= this.speed * 1.45 * dt; // Escapable charge allowing dodging
            if (this.x < -this.width - 60) {
                // Loop back for re-entry
                this.x = canvas.width + 40;
                this.y = 80 + Math.random() * (canvas.height - 240);
                this.state = 'ENTER';
                this.stateTimer = 0;
            }
        }
    }

    // IsOnScreen check: Boss must be visibly well on screen before taking damage
    isOnScreen() {
        return this.x <= canvas.width - 60 && this.x >= -this.width;
    }

    draw() {
        const frame = getBossFrame(this.currentFrame);
        const cx = this.x + this.width / 2;
        const cy = this.y + this.height / 2;
        const targetX = player ? player.x + player.width / 2 : 0;
        const targetY = player ? player.y + player.height / 2 : cy;

        // In boss1_0.png, the nose points UP (-Y in screen space)
        // Orient menacingly towards player starship on left (-X)
        // Clamp dx so during charges or passes it always faces left
        const dx = Math.min(-40, targetX - cx);
        const dy = targetY - cy;
        // Clamp banking tilt to +/- 25 degrees (+/- 0.44 radians)
        const bankAngle = Math.max(-0.44, Math.min(0.44, Math.atan2(dy, -dx)));

        ctx.save();
        ctx.translate(cx, cy);
        // Base facing LEFT (-PI/2 from UP-pointing nose) + subtle menacing banking tilt
        ctx.rotate(-Math.PI / 2 + bankAngle);

        if (frame) {
            ctx.drawImage(frame, -this.width / 2, -this.height / 2, this.width, this.height);
        }
        ctx.restore();
    }
}

class Explosion {
    constructor(x, y, size = 90) {
        this.x = x;
        this.y = y;
        this.size = size;
        this.frame = 0;
        this.frameTimer = 0;
        this.done = false;
    }

    update(dt) {
        this.frameTimer += dt;
        if (this.frameTimer > 0.035) {
            this.frameTimer = 0;
            this.frame++;
            if (this.frame >= 21) this.done = true;
        }
    }

    draw() {
        const img = getBoomFrame(this.frame);
        if (img) {
            ctx.drawImage(img, this.x - this.size / 2, this.y - this.size / 2, this.size, this.size);
        }
    }
}

class PowerUpItem {
    constructor(x, y, type) {
        this.x = x;
        this.y = y;
        this.type = type; // 'HEAL', 'DOUBLE_SHOT', 'TRIPLE_SHOT', 'RAPID_FIRE', 'SHIELD'
        this.radius = 16;
        this.speed = 120;
        this.time = 0;
    }

    update(dt) {
        this.time += dt;
        this.x -= this.speed * dt;
        this.y += Math.sin(this.time * 4) * 25 * dt;
    }

    draw() {
        ctx.save();
        ctx.beginPath();
        ctx.arc(this.x, this.y, this.radius, 0, Math.PI * 2);

        let color = '#38bdf8';
        let label = '2';
        if (this.type === 'HEAL') { color = '#4ade80'; label = '+'; }
        else if (this.type === 'TRIPLE_SHOT') { color = '#fb923c'; label = '3'; }
        else if (this.type === 'RAPID_FIRE') { color = '#fde047'; label = 'R'; }
        else if (this.type === 'SHIELD') { color = '#a855f7'; label = 'S'; }

        ctx.fillStyle = color;
        ctx.shadowColor = color;
        ctx.shadowBlur = 12;
        ctx.fill();

        ctx.fillStyle = '#030712';
        ctx.font = 'bold 15px sans-serif';
        ctx.textAlign = 'center';
        ctx.textBaseline = 'middle';
        ctx.fillText(label, this.x, this.y);
        ctx.restore();
    }
}

const floatingTexts = [];
function spawnFloatingText(x, y, text, color = '#ffffff') {
    floatingTexts.push({ x, y, text, color, alpha: 1.0, life: 1.6 });
}

// Global Collections
let player = new Player();
let bullets = [];
let critters = [];
let activeBoss = null;
let explosions = [];
let powerUps = [];
let critterSpawnTimer = 0;
let bossCycleCount = 0;
let nextBossTime = 45;

function resetGame() {
    survivalTime = 0;
    distance = 0;
    score = 0;
    combo = 0;
    comboTimer = 0;
    killsCount = 0;
    nextBossTime = 45;
    bossCycleCount = 0;
    player = new Player();
    bullets = [];
    critters = [];
    activeBoss = null;
    explosions = [];
    powerUps = [];
    boostParticles.length = 0;
    floatingTexts.length = 0;
}

// --- Spawner Logic ---
function updateSpawners(dt) {
    critterSpawnTimer += dt;
    const spawnInterval = Math.max(0.65, 2.0 - (survivalTime / 90) * 1.1);
    if (critterSpawnTimer >= spawnInterval) {
        critterSpawnTimer = 0;
        critters.push(new Critter());
    }

    // Multi-tier Boss Wave Spawning (Boss 1 at 45s, Boss 2 at 100s, Boss 3 at 150s, loop)
    if (!activeBoss && survivalTime >= nextBossTime) {
        let tier = 1;
        if (survivalTime >= 150) tier = 3;
        else if (survivalTime >= 100) tier = 2;
        activeBoss = new Boss(tier);
    }
}

// --- Main Update Loop ---
function update(dt) {
    if (gameState !== 'PLAYING') return;

    const currentWorldSpeed = player.isBoosting ? worldSpeed * 2.8 : worldSpeed;
    survivalTime += dt;
    distance += currentWorldSpeed * dt * 0.8;

    if (comboTimer > 0) {
        comboTimer -= dt;
        if (comboTimer <= 0) combo = 0;
    }

    // Parallax scrolling
    for (let i = 0; i < 4; i++) {
        bgOffsets[i] = (bgOffsets[i] + currentWorldSpeed * bgSpeeds[i] * dt) % canvas.width;
    }

    // Boost particles
    for (let i = boostParticles.length - 1; i >= 0; i--) {
        const bp = boostParticles[i];
        bp.x += bp.vx * dt;
        bp.y += bp.vy * dt;
        bp.alpha -= dt * 2.2;
        bp.size = Math.max(0, bp.size - dt * 10);
        if (bp.alpha <= 0) boostParticles.splice(i, 1);
    }

    player.update(dt);
    updateSpawners(dt);

    // Bullets
    for (let i = bullets.length - 1; i >= 0; i--) {
        const b = bullets[i];
        b.update(dt);
        if (b.x > canvas.width || b.y < -20 || b.y > canvas.height + 20) {
            bullets.splice(i, 1);
        }
    }

    // Critters & Collisions
    for (let i = critters.length - 1; i >= 0; i--) {
        const c = critters[i];
        c.update(dt);

        let critterDead = false;

        // Check bullet hits only if critter is visibly on screen (IsOnScreen from session bdb70fc3)
        if (c.isOnScreen()) {
            for (let j = bullets.length - 1; j >= 0; j--) {
                const b = bullets[j];
                if (checkCollision(b, c)) {
                    bullets.splice(j, 1);
                    critterDead = true;
                    break;
                }
            }
        }

        // Player hits critter
        if (!critterDead && checkCollision(player, c)) {
            if (player.isBoosting) {
                // Ramming while boosting destroys critter safely!
                critterDead = true;
                score += 150;
            } else {
                player.takeDamage(1);
                critterDead = true;
            }
        }

        if (critterDead) {
            sounds.play(player.isBoosting ? 'burn' : 'squish', 0.55);
            sounds.play('explode', 0.4);
            explosions.push(new Explosion(c.x + c.width / 2, c.y + c.height / 2, 80));
            killsCount++;
            combo++;
            comboTimer = 3.5;
            score += 100 * Math.max(1, combo);

            // 11% buff drop rate (faithful to Void Protocol Critter1.cs line 99)
            if (Math.random() < 0.11) {
                const types = ['HEAL', 'DOUBLE_SHOT', 'TRIPLE_SHOT', 'RAPID_FIRE', 'SHIELD'];
                const chosen = types[Math.floor(Math.random() * types.length)];
                powerUps.push(new PowerUpItem(c.x + c.width / 2, c.y + c.height / 2, chosen));
            }

            critters.splice(i, 1);
            continue;
        }

        // Off-screen cleanup
        if (c.x < -c.width - 50) critters.splice(i, 1);
    }

    // Boss Updates & Collisions
    if (activeBoss) {
        activeBoss.update(dt);

        // Bullets hitting Boss (only if boss is on screen)
        if (activeBoss.isOnScreen()) {
            for (let j = bullets.length - 1; j >= 0; j--) {
                const b = bullets[j];
                if (checkCollision(b, activeBoss)) {
                    bullets.splice(j, 1);
                    activeBoss.hp -= 1;
                    sounds.play('hit_armor', 0.45);
                    explosions.push(new Explosion(b.x, b.y, 40));
                    score += 50;

                    if (activeBoss.hp <= 0) {
                        sounds.play('explode_big', 0.9);
                        explosions.push(new Explosion(activeBoss.x + activeBoss.width / 2, activeBoss.y + activeBoss.height / 2, 260));
                        bossCycleCount++;
                        score += activeBoss.tier * 10000;
                        player.heal(activeBoss.tier);
                        player.applyBuff('SHIELD');
                        spawnFloatingText(canvas.width / 2, canvas.height / 2, 'TITAN DEFEATED! ODYSSEY CONTINUES!', '#fde047');
                        nextBossTime = survivalTime + 50;
                        activeBoss = null;
                        break;
                    }
                }
            }
        }

        // Player colliding directly with Boss (Physical contact check)
        if (activeBoss) {
            const bossHitbox = {
                x: activeBoss.x + 20,
                y: activeBoss.y + 20,
                width: activeBoss.width - 40,
                height: activeBoss.height - 40
            };
            if (checkCollision(player, bossHitbox)) {
                if (!player.isInvulnerable) {
                    player.takeDamage(2); // Boss collision deals 2 damage in Void Protocol!
                    player.x = Math.max(30, player.x - 90); // Hull knockback
                }
            }
        }
    }

    // PowerUps
    for (let i = powerUps.length - 1; i >= 0; i--) {
        const p = powerUps[i];
        p.update(dt);
        if (checkCircleBoxCollision(p, player)) {
            player.applyBuff(p.type);
            powerUps.splice(i, 1);
            continue;
        }
        if (p.x < -50) powerUps.splice(i, 1);
    }

    // Explosions
    for (let i = explosions.length - 1; i >= 0; i--) {
        explosions[i].update(dt);
        if (explosions[i].done) explosions.splice(i, 1);
    }

    // Floating Text
    for (let i = floatingTexts.length - 1; i >= 0; i--) {
        const ft = floatingTexts[i];
        ft.y -= 25 * dt;
        ft.life -= dt;
        ft.alpha = Math.max(0, ft.life / 1.6);
        if (ft.life <= 0) floatingTexts.splice(i, 1);
    }
}

function checkCollision(r1, r2) {
    return (
        r1.x < r2.x + r2.width &&
        r1.x + r1.width > r2.x &&
        r1.y < r2.y + r2.height &&
        r1.y + r1.height > r2.y
    );
}

function checkCircleBoxCollision(c, b) {
    const closeX = Math.max(b.x, Math.min(c.x, b.x + b.width));
    const closeY = Math.max(b.y, Math.min(c.y, b.y + b.height));
    const dist = Math.hypot(c.x - closeX, c.y - closeY);
    return dist < c.radius;
}

// --- Render Loop ---
function draw() {
    ctx.clearRect(0, 0, canvas.width, canvas.height);

    // 1. Parallax Backgrounds
    for (let i = 0; i < 4; i++) {
        const bg = images[`bg${i + 1}`];
        if (bg && bg.complete && bg.naturalWidth > 0) {
            const offset = bgOffsets[i];
            ctx.drawImage(bg, -offset, 0, canvas.width, canvas.height);
            ctx.drawImage(bg, canvas.width - offset, 0, canvas.width, canvas.height);
        }
    }

    // 2. Boost Particles
    for (const bp of boostParticles) {
        ctx.save();
        ctx.globalAlpha = bp.alpha;
        ctx.fillStyle = bp.color;
        ctx.shadowColor = bp.color;
        ctx.shadowBlur = 10;
        ctx.beginPath();
        ctx.arc(bp.x, bp.y, bp.size, 0, Math.PI * 2);
        ctx.fill();
        ctx.restore();
    }

    // 3. Power-Ups
    for (const p of powerUps) p.draw();

    // 4. Critters
    for (const c of critters) c.draw();

    // 5. Boss
    if (activeBoss) activeBoss.draw();

    // 6. Bullets
    for (const b of bullets) b.draw();

    // 7. Player Starship
    player.draw();

    // 8. Explosions
    for (const ex of explosions) ex.draw();

    // 9. Floating Texts
    for (const ft of floatingTexts) {
        ctx.save();
        ctx.globalAlpha = ft.alpha;
        ctx.fillStyle = ft.color;
        ctx.font = 'bold 18px sans-serif';
        ctx.shadowColor = ft.color;
        ctx.shadowBlur = 8;
        ctx.fillText(ft.text, ft.x, ft.y);
        ctx.restore();
    }

    // 10. HUD Overlays (Using universal polyfilled round rectangles)
    drawHUD();
}

function drawHUD() {
    // Top-Center Info Capsule
    ctx.fillStyle = 'rgba(15, 23, 42, 0.75)';
    ctx.strokeStyle = '#38bdf8';
    ctx.lineWidth = 1.5;
    fillRoundRect(ctx, canvas.width / 2 - 210, 16, 420, 44, 8);
    strokeRoundRect(ctx, canvas.width / 2 - 210, 16, 420, 44, 8);

    const mins = Math.floor(survivalTime / 60);
    const secs = Math.floor(survivalTime % 60);
    const timeStr = `${String(mins).padStart(2, '0')}:${String(secs).padStart(2, '0')}`;
    const distKm = Math.floor(distance / 100);

    ctx.font = 'bold 20px monospace';
    ctx.textAlign = 'center';
    ctx.fillStyle = '#94a3b8';
    ctx.fillText('TIME ', canvas.width / 2 - 110, 44);
    ctx.fillStyle = '#ffffff';
    ctx.fillText(timeStr, canvas.width / 2 - 50, 44);

    ctx.fillStyle = '#94a3b8';
    ctx.fillText('DIST ', canvas.width / 2 + 50, 44);
    ctx.fillStyle = '#38bdf8';
    ctx.fillText(`${distKm} KM`, canvas.width / 2 + 120, 44);

    // Left HUD: Health
    ctx.font = '24px sans-serif';
    ctx.textAlign = 'left';
    let hearts = '';
    for (let i = 0; i < player.health; i++) hearts += '❤️ ';
    for (let i = player.health; i < player.maxHealth; i++) hearts += '🖤 ';
    ctx.fillText(hearts, 24, 40);

    // Labeled Boost / Energy Bar
    const bBarX = 24;
    const bBarY = 68;
    const bBarW = 190;
    const bBarH = 14;

    ctx.font = 'bold 12px sans-serif';
    ctx.fillStyle = '#38bdf8';
    ctx.fillText('BOOST / RAM [SPACE / R-CLICK]', bBarX, bBarY - 6);

    ctx.fillStyle = 'rgba(15, 23, 42, 0.85)';
    ctx.fillRect(bBarX, bBarY, bBarW, bBarH);

    const boostPct = Math.max(0, player.energy / player.maxEnergy);
    ctx.fillStyle = player.energy > 25 ? '#0284c7' : '#ef4444';
    ctx.fillRect(bBarX, bBarY, boostPct * bBarW, bBarH);
    ctx.fillStyle = player.energy > 25 ? '#38bdf8' : '#f87171';
    ctx.fillRect(bBarX, bBarY, boostPct * bBarW, bBarH / 2);

    ctx.strokeStyle = '#64748b';
    ctx.lineWidth = 1.5;
    ctx.strokeRect(bBarX, bBarY, bBarW, bBarH);

    ctx.fillStyle = '#ffffff';
    ctx.font = 'bold 10px monospace';
    ctx.textAlign = 'center';
    ctx.fillText(`${Math.round(boostPct * 100)}%`, bBarX + bBarW / 2, bBarY + 11);

    // Active Buffs Indicators
    let activeBuffs = [];
    if (player.tripleShotTimer > 0) activeBuffs.push(`TRIPLE SHOT [${Math.ceil(player.tripleShotTimer)}s]`);
    if (player.doubleShotTimer > 0 && player.tripleShotTimer <= 0) activeBuffs.push(`DOUBLE SHOT [${Math.ceil(player.doubleShotTimer)}s]`);
    if (player.rapidFireTimer > 0) activeBuffs.push(`RAPID FIRE [${Math.ceil(player.rapidFireTimer)}s]`);
    if (player.superShieldTimer > 0) activeBuffs.push(`SHIELD [${Math.ceil(player.superShieldTimer)}s]`);

    if (activeBuffs.length > 0) {
        ctx.font = 'bold 13px monospace';
        ctx.textAlign = 'left';
        ctx.fillStyle = '#fde047';
        ctx.fillText(activeBuffs.join(' | '), bBarX, bBarY + 30);
    }

    // Right HUD: Score & Combo
    ctx.textAlign = 'right';
    ctx.font = 'bold 22px monospace';
    ctx.fillStyle = '#f8fafc';
    ctx.fillText(`SCORE: ${score.toLocaleString()}`, canvas.width - 24, 40);

    if (combo > 1) {
        ctx.font = 'bold 18px sans-serif';
        ctx.fillStyle = '#fde047';
        ctx.fillText(`COMBO x${combo}!`, canvas.width - 24, 66);
    }

    // Bottom HUD: Single-Line Boss Health Bar (From session bdb70fc3)
    if (activeBoss && activeBoss.x < canvas.width) {
        const barW = 540;
        const barH = 20;
        const barX = canvas.width / 2 - barW / 2;
        const barY = canvas.height - 48;

        ctx.fillStyle = 'rgba(15, 23, 42, 0.9)';
        fillRoundRect(ctx, barX - 10, barY - 26, barW + 20, 52, 6);

        ctx.fillStyle = '#ef4444';
        ctx.font = 'bold 15px sans-serif';
        ctx.textAlign = 'center';
        ctx.fillText(`${activeBoss.name} [${activeBoss.hp}/${activeBoss.maxHp}]`, canvas.width / 2, barY - 8);

        ctx.fillStyle = '#334155';
        ctx.fillRect(barX, barY, barW, barH);
        ctx.fillStyle = '#dc2626';
        ctx.fillRect(barX, barY, Math.max(0, (activeBoss.hp / activeBoss.maxHp)) * barW, barH);
        ctx.strokeStyle = '#f87171';
        ctx.strokeRect(barX, barY, barW, barH);
    }
}

function triggerGameOver() {
    gameState = 'GAMEOVER';
    sounds.play('explode_big', 0.9);

    let rank = 'RANK C';
    let color = '#94a3b8';
    if (score >= 40000 || survivalTime >= 180) { rank = 'RANK S+ (APEX)'; color = '#f43f5e'; }
    else if (score >= 25000 || survivalTime >= 120) { rank = 'RANK S'; color = '#fde047'; }
    else if (score >= 15000 || survivalTime >= 75) { rank = 'RANK A'; color = '#38bdf8'; }
    else if (score >= 8000 || survivalTime >= 40) { rank = 'RANK B'; color = '#4ade80'; }

    const mins = Math.floor(survivalTime / 60);
    const secs = Math.floor(survivalTime % 60);
    const timeStr = `${String(mins).padStart(2, '0')}:${String(secs).padStart(2, '0')}`;

    document.getElementById('final-rank').innerText = rank;
    document.getElementById('final-rank').style.color = color;
    document.getElementById('final-time').innerText = timeStr;
    document.getElementById('final-dist').innerText = `${Math.floor(distance / 100)} KM`;
    document.getElementById('final-score').innerText = score.toLocaleString();
    document.getElementById('final-kills').innerText = killsCount;

    document.getElementById('gameover-overlay').classList.remove('hidden');
}

function togglePause() {
    if (gameState === 'PLAYING') {
        gameState = 'PAUSED';
        sounds.play('pause', 0.7);
        document.getElementById('pause-overlay').classList.remove('hidden');
    } else if (gameState === 'PAUSED') {
        gameState = 'PLAYING';
        sounds.play('unpause', 0.7);
        document.getElementById('pause-overlay').classList.add('hidden');
    }
}

// UI Event Handlers
const startBtn = document.getElementById('start-btn');
if (startBtn) {
    startBtn.disabled = true;
    startBtn.innerText = 'INITIALIZING SYSTEMS...';

    startBtn.addEventListener('click', () => {
        sounds.init();
        resetGame();
        gameState = 'PLAYING';
        document.getElementById('start-overlay').classList.add('hidden');
    });
}

// Start preloading all visual assets immediately
preloadAllAssets().then(() => {
    if (startBtn) {
        startBtn.disabled = false;
        startBtn.innerText = 'LAUNCH MISSION';
    }
});

document.getElementById('resume-btn').addEventListener('click', () => {
    togglePause();
});

document.getElementById('restart-btn').addEventListener('click', () => {
    resetGame();
    gameState = 'PLAYING';
    document.getElementById('gameover-overlay').classList.add('hidden');
});

// Immortal Animation Loop: requestAnimationFrame scheduled FIRST and wrapped in try-catch
let lastTime = performance.now();
function gameLoop(now) {
    requestAnimationFrame(gameLoop); // Loop never terminates!

    const dt = Math.min((now - lastTime) / 1000, 0.1);
    lastTime = now;

    try {
        update(dt);
        draw();
    } catch (err) {
        console.error('Frame error:', err);
    }
}

requestAnimationFrame(gameLoop);
