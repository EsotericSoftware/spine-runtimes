import { execSync } from 'node:child_process';
import * as fs from 'node:fs';
import * as path from 'node:path';

function findTypeScriptFiles (dir: string, files: string[] = []): string[] {
	if (!fs.existsSync(dir)) return files;

	fs.readdirSync(dir).forEach(name => {
		const filePath = path.join(dir, name);
		const stat = fs.statSync(filePath);

		if (stat.isDirectory()) {
			// Skip node_modules and dist directories
			if (name !== 'node_modules' && name !== 'dist') {
				findTypeScriptFiles(filePath, files);
			}
		} else if (name.endsWith('.ts') && !name.endsWith('.d.ts')) {
			files.push(filePath);
		}
	});

	return files;
}

// Find all TypeScript files in spine-* directories
const allFiles: string[] = [];
fs.readdirSync('.').forEach(name => {
	if (name.startsWith('spine-') && fs.statSync(name).isDirectory()) {
		findTypeScriptFiles(name, allFiles);
	}
});

if (allFiles.length > 0) {
	console.log(`Formatting ${allFiles.length} TypeScript files...`);
	execSync(`npx -y --package=typescript@6.0.3 --package=typescript-formatter@7.2.2 tsfmt -r ${allFiles.join(' ')}`, { stdio: 'inherit' });
} else {
	console.log('No TypeScript files found to format.');
}