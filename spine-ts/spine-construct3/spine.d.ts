import type * as Spine from '@esotericsoftware/spine-construct3-lib';

declare global {
  namespace globalThis {
    var spine: typeof Spine;
  }
}
