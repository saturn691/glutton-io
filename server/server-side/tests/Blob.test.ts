import { expect, test } from '@jest/globals';
import { Blob } from '../classes/Blob';

test('Blob.WhoAteWho', () => {
  let blob1 = new Blob("0", {x: 0, y: 0}, 1);
  let blob2 = new Blob("1", {x: 0, y: 0}, 1); 
  expect(Blob.WhoAteWho(blob1, blob2)).toBe(0);

  blob1.size = 2;
  expect(Blob.WhoAteWho(blob1, blob2)).toBe(1);
 
  blob1.size = 1;
  blob2.size = 2;
  expect(Blob.WhoAteWho(blob1, blob2)).toBe(2);
});