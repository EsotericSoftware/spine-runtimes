// build.js
import * as fs from 'node:fs';
import * as esbuild from 'esbuild';

// Plugin to append string after build
const appendStringPlugin = {
  name: 'append-string',
  setup(build) {
    build.onEnd((result) => {
      if (result.errors.length === 0) {
        const outputFile = 'spine-construct3-lib/dist/iife/spine-construct3-lib.js';

        try {
          const content = fs.readFileSync(outputFile, 'utf8');
          const appendString = 'if(!globalThis.spine)globalThis.spine=spine';

          // Only append if not already present
          if (!content.includes(appendString)) {
            fs.writeFileSync(outputFile, `${content}\n${appendString}`);
            console.log('✓ Appended spine global assignment');
          }
        } catch (err) {
          console.error('Error appending string:', err);
        }
      }
    });
  }
};

// Check if watch mode is requested
const isWatch = process.argv.includes('--watch');

// Build configuration
const buildOptions = {
  entryPoints: ['spine-construct3-lib/src/index.ts'],
  bundle: true,
  outfile: 'spine-construct3-lib/dist/iife/spine-construct3-lib.js',
  tsconfig: 'spine-construct3-lib/tsconfig.json',
  sourcemap: true,
  format: 'iife',
  globalName: 'spine',
  plugins: [appendStringPlugin],
};

if (isWatch) {
  const ctx = await esbuild.context(buildOptions);
  await ctx.watch();
  console.log('👀 Watching for changes...');
} else {
  await esbuild.build(buildOptions);
  console.log('✅ Build complete');
}