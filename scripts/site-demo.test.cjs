const test = require('node:test');
const assert = require('node:assert/strict');
const Model = require('../docs/demo-model.js');

test('tracker changes only when an announce is sent', () => {
  const model = new Model();
  model.start();
  model.tick(1);
  assert.ok(Math.abs(model.upload - 4.6) < 1e-9);
  assert.equal(model.ratio, 0.4);
  model.tick(1.5);
  assert.ok(Math.abs(model.ratio - 0.55) < 1e-9);
  assert.equal(model.announces, 1);
});
test('pause freezes time and counters; manual announce publishes pending values', () => {
  const model = new Model();
  model.start(); model.tick(1); model.pause();
  const before = JSON.stringify(model);
  model.tick(60);
  assert.equal(JSON.stringify(model), before);
  assert.equal(model.announce(), true);
  assert.equal(model.announce(), false);
  assert.equal(model.reported, model.upload);
});
test('rate change, completed state, invalid input and reset stay consistent', () => {
  const model = new Model();
  model.setSpeed(Infinity); assert.equal(model.speed, 10);
  model.setSpeed(100); assert.equal(model.speed, 50);
  model.start(); model.tick(100);
  assert.equal(model.upload, 30); assert.equal(model.ratio, 3);
  assert.equal(model.running, false); assert.equal(model.finished, true);
  model.start(); assert.equal(model.running, false);
  model.reset(); assert.equal(model.ratio, .4); assert.equal(model.speed, 10);
  assert.equal(model.history.length, 1); assert.equal(model.announces, 0);
  model.tick(NaN); assert.equal(model.upload, 4);
});
test('the same elapsed time produces the same counters across frame sizes', () => {
  const a = new Model(), b = new Model(); a.start(); b.start();
  a.tick(10); for (let n = 0; n < 100; n++) b.tick(.1);
  assert.ok(Math.abs(a.upload - b.upload) < 1e-8);
  assert.ok(Math.abs(a.reported - b.reported) < 1e-8);
  assert.equal(a.announces, b.announces);
});
