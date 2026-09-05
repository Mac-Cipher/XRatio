/* Local illustration only: decimal GB, 60 virtual seconds per real second. */
class XRatioDemoModel {
  constructor() { this.reset(); }
  reset() {
    this.upload = 4;
    this.reported = 4;
    this.download = 10;
    this.speed = 10;
    this.running = false;
    this.finished = false;
    this.elapsed = 0;
    this.sinceAnnounce = 0;
    this.announces = 0;
    this.history = [{ time: 0, ratio: 0.4 }];
  }
  get ratio() { return this.reported / this.download; }
  get nextAnnounce() { return Math.max(0, 2.5 - this.sinceAnnounce); }
  setSpeed(value) {
    if (Number.isFinite(value)) this.speed = Math.max(1, Math.min(50, value));
  }
  start() { if (!this.finished) this.running = true; }
  pause() { this.running = false; }
  announce() {
    if (this.upload <= this.reported) return false;
    this.reported = this.upload;
    this.announces += 1;
    this.sinceAnnounce = 0;
    this.history.push({ time: this.elapsed, ratio: this.ratio });
    return true;
  }
  tick(seconds) {
    if (!this.running || !Number.isFinite(seconds) || seconds <= 0) return false;
    let remaining = seconds;
    let updated = false;
    while (remaining > 0 && this.running) {
      const step = Math.min(remaining, this.nextAnnounce);
      this.upload = Math.min(30, this.upload + step * this.speed * 60 / 1000);
      this.elapsed += step;
      this.sinceAnnounce += step;
      remaining -= step;
      if (this.sinceAnnounce >= 2.5 - 1e-9 || this.upload >= 30) {
        updated = this.announce() || updated;
      }
      if (this.upload >= 30) { this.finished = true; this.running = false; }
    }
    return updated;
  }
}
if (typeof module !== 'undefined' && module.exports) module.exports = XRatioDemoModel;
else globalThis.XRatioDemoModel = XRatioDemoModel;
