#!/usr/bin/env node

import { existsSync, writeFileSync, mkdirSync, rmSync, statSync } from 'fs';
import { resolve, dirname, join } from 'path';
import { execSync } from 'child_process';
import { fileURLToPath } from 'url';

// Logging functions following formatters/logging/README.md style
function log_title (title: string): void {
	console.error(`\x1b[1m${title}\x1b[0m`);
}

function log_action (action: string): void {
	process.stderr.write(`  ${action}... `);
}

function log_ok (): void {
	console.error('\x1b[32m✓\x1b[0m');
}

function log_fail (): void {
	console.error('\x1b[31m✗\x1b[0m');
}

function log_detail (detail: string): void {
	console.error(`\x1b[90m${detail}\x1b[0m`);
}

function log_summary (summary: string): void {
	console.error(`\x1b[1m${summary}\x1b[0m`);
}

function cleanOutputDirectory (): void {
	const outputDir = join(SPINE_ROOT, 'tests', 'output');

	log_action('Cleaning output directory');
	try {
		if (existsSync(outputDir)) {
			rmSync(outputDir, { recursive: true, force: true });
		}
		mkdirSync(outputDir, { recursive: true });
		log_ok();
	} catch (error: any) {
		log_fail();
		log_detail(`Failed to clean output directory: ${error.message}`);
		process.exit(1);
	}
}

function getNewestFileTime (directory: string, pattern: string): number {
	try {
		const files = execSync(`find "${directory}" -name "${pattern}" -type f`, { encoding: 'utf8' })
			.trim()
			.split('\n')
			.filter(f => f);

		if (files.length === 0) return 0;

		return Math.max(...files.map(file => {
			try {
				return statSync(file).mtime.getTime();
			} catch {
				return 0;
			}
		}));
	} catch {
		return 0;
	}
}

function needsJavaBuild (): boolean {
	const javaDir = join(SPINE_ROOT, 'spine-libgdx');
	const testDir = join(javaDir, 'spine-libgdx-tests');
	const buildDir = join(testDir, 'build', 'libs');

	try {
		// Check if fat jar exists (specifically look for the headless test jar)
		const fatJarFiles = execSync(`ls ${buildDir}/spine-headless-test-*.jar 2>/dev/null || true`, { encoding: 'utf8' }).trim();
		if (!fatJarFiles) return true;

		// Get jar modification time
		const jarTime = statSync(fatJarFiles.split('\n')[0]).mtime.getTime();

		// Check Java source files
		const javaSourceTime = getNewestFileTime(join(SPINE_ROOT, 'spine-libgdx'), '*.java');

		// Check Gradle files
		const gradleTime = getNewestFileTime(javaDir, 'build.gradle*');

		return javaSourceTime > jarTime || gradleTime > jarTime;
	} catch {
		return true;
	}
}

function needsCppBuild (): boolean {
	const cppDir = join(SPINE_ROOT, 'spine-cpp');
	const buildDir = join(cppDir, 'build');
	const headlessTest = join(buildDir, 'headless-test');

	try {
		// Check if executable exists
		if (!existsSync(headlessTest)) return true;

		// Get executable modification time
		const execTime = statSync(headlessTest).mtime.getTime();

		// Check C++ source files
		const cppSourceTime = getNewestFileTime(join(SPINE_ROOT, 'spine-cpp'), '*.cpp');
		const hppSourceTime = getNewestFileTime(join(SPINE_ROOT, 'spine-cpp'), '*.h');
		const cmakeTime = getNewestFileTime(cppDir, 'CMakeLists.txt');

		const newestSourceTime = Math.max(cppSourceTime, hppSourceTime, cmakeTime);

		return newestSourceTime > execTime;
	} catch {
		return true;
	}
}

function needsHaxeBuild (): boolean {
	const haxeDir = join(SPINE_ROOT, 'spine-haxe');
	const buildDir = join(haxeDir, 'build');
	const headlessTest = join(buildDir, 'headless-test', 'HeadlessTest');

	try {
		// Check if executable exists
		if (!existsSync(headlessTest)) return true;

		// Get executable modification time
		const execTime = statSync(headlessTest).mtime.getTime();

		// Check Haxe source files
		const haxeSourceTime = getNewestFileTime(join(haxeDir, 'spine-haxe'), '*.hx');
		const testSourceTime = getNewestFileTime(join(haxeDir, 'tests'), '*.hx');
		const buildScriptTime = getNewestFileTime(haxeDir, 'build-headless-test.sh');

		const newestSourceTime = Math.max(haxeSourceTime, testSourceTime, buildScriptTime);

		return newestSourceTime > execTime;
	} catch {
		return true;
	}
}

const __filename = fileURLToPath(import.meta.url);
const __dirname = dirname(__filename);
const SPINE_ROOT = resolve(__dirname, '../..');

interface TestArgs {
	language: string;
	skeletonPath: string;
	atlasPath: string;
	animationName?: string;
	animationName2?: string;
}

interface SkeletonFiles {
	jsonPath: string;
	skelPath: string;
	atlasPath: string;
}

function findSkeletonFiles (skeletonName: string): SkeletonFiles {
	const examplesDir = join(SPINE_ROOT, 'examples', skeletonName);
	const exportDir = join(examplesDir, 'export');

	if (!existsSync(exportDir)) {
		log_detail(`Export directory not found: ${exportDir}`);
		process.exit(1);
	}

	// Try to find skeleton files (pro first, then ess)
	let jsonPath: string | null = null;
	let skelPath: string | null = null;

	const proJson = join(exportDir, `${skeletonName}-pro.json`);
	const proSkel = join(exportDir, `${skeletonName}-pro.skel`);
	const essJson = join(exportDir, `${skeletonName}-ess.json`);
	const essSkel = join(exportDir, `${skeletonName}-ess.skel`);

	if (existsSync(proJson) && existsSync(proSkel)) {
		jsonPath = proJson;
		skelPath = proSkel;
	} else if (existsSync(essJson) && existsSync(essSkel)) {
		jsonPath = essJson;
		skelPath = essSkel;
	} else {
		log_detail(`Could not find matching .json and .skel files for ${skeletonName}`);
		log_detail(`Tried: ${proJson}, ${proSkel}, ${essJson}, ${essSkel}`);
		process.exit(1);
	}

	// Try to find atlas file (pma first, then regular)
	let atlasPath: string | null = null;
	const pmaAtlas = join(exportDir, `${skeletonName}-pma.atlas`);
	const regularAtlas = join(exportDir, `${skeletonName}.atlas`);

	if (existsSync(pmaAtlas)) {
		atlasPath = pmaAtlas;
	} else if (existsSync(regularAtlas)) {
		atlasPath = regularAtlas;
	} else {
		log_detail(`Could not find atlas file for ${skeletonName}`);
		log_detail(`Tried: ${pmaAtlas}, ${regularAtlas}`);
		process.exit(1);
	}

	return {
		jsonPath,
		skelPath,
		atlasPath
	};
}

function validateArgs (): { language: string; files?: SkeletonFiles; skeletonPath?: string; atlasPath?: string; animationName?: string; animationName2?: string; fixFloats?: boolean } {
	const args = process.argv.slice(2);

	if (args.length < 2) {
		log_detail('Usage: headless-test-runner <target-language> -s <skeleton-name> [animation-name] [-f]');
		log_detail('       headless-test-runner <target-language> <skeleton-path> <atlas-path> [animation-name] [-f]');
		log_detail('Target languages: cpp');
		log_detail('Flags: -f (fix floats and propertyIds to match Java values)');
		process.exit(1);
	}

	// Check for -f flag
	const fixFloats = args.includes('-f');
	const filteredArgs = args.filter(arg => arg !== '-f');

	const [language, ...restArgs] = filteredArgs;

	if (!['cpp', 'haxe'].includes(language)) {
		log_detail(`Invalid target language: ${language}. Must be cpp or haxe`);
		process.exit(1);
	}

	// Check if using -s flag
	if (restArgs[0] === '-s') {
		if (restArgs.length < 2) {
			log_detail('Usage: headless-test-runner <target-language> -s <skeleton-name> [animation-name]');
			process.exit(1);
		}

		const [, skeletonName, animationName, animationName2] = restArgs;
		const files = findSkeletonFiles(skeletonName);

		return {
			language,
			files,
			animationName,
			animationName2,
			fixFloats
		};
	} else {
		// Explicit paths mode
		if (restArgs.length < 2) {
			log_detail('Usage: headless-test-runner <target-language> <skeleton-path> <atlas-path> [animation-name]');
			process.exit(1);
		}

		const [skeletonPath, atlasPath, animationName, animationName2] = restArgs;
		const resolvedSkeletonPath = resolve(skeletonPath);
		const resolvedAtlasPath = resolve(atlasPath);

		if (!existsSync(resolvedSkeletonPath)) {
			log_detail(`Skeleton file not found: ${resolvedSkeletonPath}`);
			process.exit(1);
		}

		if (!existsSync(resolvedAtlasPath)) {
			log_detail(`Atlas file not found: ${resolvedAtlasPath}`);
			process.exit(1);
		}

		return {
			language,
			skeletonPath: resolvedSkeletonPath,
			atlasPath: resolvedAtlasPath,
			animationName,
			animationName2,
			fixFloats
		};
	}
}

function executeJava (args: TestArgs): string {
	const javaDir = join(SPINE_ROOT, 'spine-libgdx');
	const testDir = join(javaDir, 'spine-libgdx-tests');

	if (!existsSync(testDir)) {
		log_detail(`Java test directory not found: ${testDir}`);
		process.exit(1);
	}

	// Check if we need to build
	if (needsJavaBuild()) {
		// Check if we have a gradle wrapper or gradle
		const hasGradlew = existsSync(join(javaDir, 'gradlew'));
		const gradleCmd = hasGradlew ? './gradlew' : 'gradle';

		// Build the fat jar
		log_action('Building Java HeadlessTest fat jar');
		try {
			execSync(`${gradleCmd} :spine-libgdx-tests:fatJar`, {
				cwd: javaDir,
				stdio: ['inherit', 'pipe', 'inherit']
			});
			log_ok();
		} catch (error: any) {
			log_fail();
			log_detail(`Java build failed: ${error.message}`);
			process.exit(1);
		}
	}

	// Find the fat jar file
	const buildDir = join(testDir, 'build', 'libs');
	const fatJarFiles = execSync(`ls ${buildDir}/spine-headless-test-*.jar`, { encoding: 'utf8' }).trim().split('\n');

	if (fatJarFiles.length === 0) {
		log_detail('No fat jar files found in build/libs directory');
		process.exit(1);
	}

	const jarFile = fatJarFiles[0]; // Use the first fat jar file found

	// Run the HeadlessTest from jar
	const testArgs = [args.skeletonPath, args.atlasPath];
	if (args.animationName) {
		testArgs.push(args.animationName);
	}

	if (args.animationName2) {
		testArgs.push(args.animationName2);
	}

	log_action('Running Java HeadlessTest');
	try {
		const output = execSync(`java -cp "${jarFile}" com.esotericsoftware.spine.HeadlessTest ${testArgs.join(' ')}`, {
			encoding: 'utf8',
			maxBuffer: 50 * 1024 * 1024 // 50MB buffer for large output
		});
		log_ok();
		return output;
	} catch (error: any) {
		log_fail();
		log_detail(`Java execution failed: ${error.message}`);
		process.exit(1);
	}
}

function executeCpp (args: TestArgs): string {
	const cppDir = join(SPINE_ROOT, 'spine-cpp');
	const testsDir = join(cppDir, 'tests');

	if (!existsSync(testsDir)) {
		log_detail(`C++ tests directory not found: ${testsDir}`);
		process.exit(1);
	}

	// Check if we need to build
	if (needsCppBuild()) {
		// Build using build.sh
		log_action('Building C++ tests');
		try {
			execSync('./build.sh clean debug sanitize', {
				cwd: cppDir,
				stdio: ['inherit', 'pipe', 'inherit']
			});
			log_ok();
		} catch (error: any) {
			log_fail();
			log_detail(`C++ build failed: ${error.message}`);
			process.exit(1);
		}
	}

	// Now run the headless test directly
	const testArgs = [args.skeletonPath, args.atlasPath];
	if (args.animationName) {
		testArgs.push(args.animationName);
	}
	if (args.animationName2) {
		testArgs.push(args.animationName2);
	}

	const buildDir = join(cppDir, 'build');
	const headlessTest = join(buildDir, 'headless-test');

	if (!existsSync(headlessTest)) {
		log_detail(`C++ headless-test executable not found: ${headlessTest}`);
		process.exit(1);
	}

	log_action('Running C++ HeadlessTest');
	try {
		const output = execSync(`${headlessTest} ${testArgs.join(' ')}`, {
			encoding: 'utf8',
			maxBuffer: 50 * 1024 * 1024 // 50MB buffer for large output
		});
		log_ok();
		return output;
	} catch (error: any) {
		log_fail();
		log_detail(`C++ execution failed: ${error.message}`);
		process.exit(1);
	}
}

function executeHaxe (args: TestArgs): string {
	const haxeDir = join(SPINE_ROOT, 'spine-haxe');
	const testsDir = join(haxeDir, 'tests');

	if (!existsSync(testsDir)) {
		log_detail(`Haxe tests directory not found: ${testsDir}`);
		process.exit(1);
	}

	// Check if we need to build
	if (needsHaxeBuild()) {
		log_action('Building Haxe HeadlessTest');
		try {
			execSync('./build-headless-test.sh', {
				cwd: haxeDir,
				stdio: ['inherit', 'pipe', 'inherit']
			});
			log_ok();
		} catch (error: any) {
			log_fail();
			log_detail(`Haxe build failed: ${error.message}`);
			process.exit(1);
		}
	}

	// Run the headless test
	const testArgs = [args.skeletonPath, args.atlasPath];
	if (args.animationName) {
		testArgs.push(args.animationName);
	}

	const buildDir = join(haxeDir, 'build');
	const headlessTest = join(buildDir, 'headless-test', 'HeadlessTest');

	if (!existsSync(headlessTest)) {
		log_detail(`Haxe headless-test executable not found: ${headlessTest}`);
		process.exit(1);
	}

	log_action('Running Haxe HeadlessTest');
	try {
		const output = execSync(`${headlessTest} ${testArgs.join(' ')}`, {
			encoding: 'utf8',
			maxBuffer: 50 * 1024 * 1024 // 50MB buffer for large output
		});
		log_ok();
		return output;
	} catch (error: any) {
		log_fail();
		log_detail(`Haxe execution failed: ${error.message}`);
		process.exit(1);
	}
}

function parseOutput (output: string): { skeletonData: any, skeletonState: any, animationState?: any, transitionFrames?: any[] } {
	// Split output by section headers, keeping the headers
	const headerRegex = /=== ([A-Z 0-9]+?) ===/g;
	const headers: string[] = [];
	let match;
	while ((match = headerRegex.exec(output)) !== null) {
		headers.push(match[1]);
	}
	const sections = output.split(/=== [A-Z 0-9]+? ===/);

	if (sections.length < 3) {
		log_detail(`Expected at least 2 sections in output, got: ${sections.length - 1}`);
		process.exit(1);
	}

	try {
		const result: { skeletonData: any, skeletonState: any, animationState?: any, transitionFrames?: any[] } = {
			skeletonData: JSON.parse(sections[1].trim()),
			skeletonState: JSON.parse(sections[2].trim())
		};

		// Animation state is optional (only present if animation was loaded)
		if (sections.length >= 4 && sections[3].trim()) {
			result.animationState = JSON.parse(sections[3].trim());
		}

		// Transition frames
		const transitionFrames: any[] = [];
		for (let i = 0; i < headers.length; i++) {
			if (headers[i].startsWith('TRANSITION FRAME')) {
				const sectionContent = sections[i + 1].trim();
				if (sectionContent) transitionFrames.push(JSON.parse(sectionContent));
			}
		}
		if (transitionFrames.length > 0) result.transitionFrames = transitionFrames;

		return result;
	} catch (error) {
		log_detail(`Failed to parse JSON output: ${error}`);
		log_detail(`Section lengths: ${sections.map(s => s.length)}`);
		process.exit(1);
	}
}

function fixFloatsAndPropertyIds (target: any, reference: any, path: string = '', targetLanguage: string = 'cpp'): any {
	// Handle null values
	if (reference === null || target === null) return target;

	// Handle circular references
	if (reference === '<circular>' || target === '<circular>') return target;

	// Handle arrays
	if (Array.isArray(reference) && Array.isArray(target)) {
		return target.map((item: any, index: number) => {
			if (index < reference.length) {
				return fixFloatsAndPropertyIds(item, reference[index], `${path}[${index}]`, targetLanguage);
			}
			return item;
		});
	}

	// Handle objects
	if (typeof reference === 'object' && typeof target === 'object') {
		const result: any = {};

		// Copy all target properties
		for (const key in target) {
			if (target.hasOwnProperty(key)) {
				if (reference.hasOwnProperty(key)) {
					// Special cases where we always use Java value
					if (key === 'propertyIds') {
						// PropertyIds are encoded differently across all languages
						result[key] = reference[key];
					} else if (targetLanguage === 'cpp' && (
						(reference[key] === null && target[key] === '') ||
						(reference[key] === '' && target[key] === null) ||
						(key === 'bones' && reference[key] === null && Array.isArray(target[key]) && target[key].length === 0)
					)) {
						// C++ specific fixes: null vs empty string, null vs empty array for bones
						result[key] = reference[key];
					} else {
						result[key] = fixFloatsAndPropertyIds(target[key], reference[key], `${path}.${key}`, targetLanguage);
					}
				} else {
					result[key] = target[key];
				}
			}
		}

		// Add any missing properties from reference (shouldn't happen, but defensive)
		for (const key in reference) {
			if (reference.hasOwnProperty(key) && !result.hasOwnProperty(key)) {
				result[key] = reference[key];
			}
		}

		return result;
	}

	// Handle primitive values
	if (typeof reference === 'number' && typeof target === 'number') {
		// Check if both are floats and within epsilon
		const diff = Math.abs(reference - target);
		if (diff <= 0.001 && diff > 0) {
			return reference; // Use Java value
		}
	}


	return target;
}

function saveJsonFiles (args: TestArgs, parsed: any, javaParsed?: any, fixFloats?: boolean): void {
	// Ensure output directory exists
	const outputDir = join(SPINE_ROOT, 'tests', 'output');
	if (!existsSync(outputDir)) {
		mkdirSync(outputDir, { recursive: true });
	}

	// Determine file format from skeleton path
	const format = args.skeletonPath.endsWith('.json') ? 'json' : 'skel';

	// Apply float fixing if enabled and we have Java reference data
	let outputData = parsed;
	if (fixFloats && javaParsed && args.language !== 'java') {
		log_action('Fixing floats and propertyIds to match Java values');
		outputData = {
			skeletonData: fixFloatsAndPropertyIds(parsed.skeletonData, javaParsed.skeletonData, '', args.language),
			skeletonState: fixFloatsAndPropertyIds(parsed.skeletonState, javaParsed.skeletonState, '', args.language),
			animationState: parsed.animationState && javaParsed.animationState ?
				fixFloatsAndPropertyIds(parsed.animationState, javaParsed.animationState, '', args.language) : parsed.animationState,
			transitionFrames: parsed.transitionFrames && javaParsed.transitionFrames ?
				parsed.transitionFrames.map((frame: any, i: number) =>
					fixFloatsAndPropertyIds(frame, javaParsed.transitionFrames[i], '', args.language)) : parsed.transitionFrames
		};
		log_ok();
	}

	// Save files with language and format in filename
	writeFileSync(join(outputDir, `skeleton-data-${args.language}-${format}.json`), JSON.stringify(outputData.skeletonData, null, 2));
	writeFileSync(join(outputDir, `skeleton-state-${args.language}-${format}.json`), JSON.stringify(outputData.skeletonState, null, 2));

	// Only save animation state if it exists
	if (outputData.animationState) {
		writeFileSync(join(outputDir, `animation-state-${args.language}-${format}.json`), JSON.stringify(outputData.animationState, null, 2));
	}

	// Save transition frames if they exist
	if (outputData.transitionFrames) {
		for (let i = 0; i < outputData.transitionFrames.length; i++) {
			writeFileSync(join(outputDir, `transition-frame-${i}-${args.language}-${format}.json`), JSON.stringify(outputData.transitionFrames[i], null, 2));
		}
	}
}

function runTestsForFiles (language: string, skeletonPath: string, atlasPath: string, animationName?: string, animationName2?: string, fixFloats?: boolean): void {
	const testArgs: TestArgs = {
		language,
		skeletonPath,
		atlasPath,
		animationName,
		animationName2
	};

	const fileType = skeletonPath.endsWith('.json') ? 'JSON' : 'BINARY';
	const fileName = skeletonPath.split('/').pop() || skeletonPath;

	log_detail(`Testing ${fileType}: ${fileName}`);

	// Always run Java first (reference implementation)
	const javaOutput = executeJava(testArgs);
	const javaParsed = parseOutput(javaOutput);
	saveJsonFiles({ ...testArgs, language: 'java' }, javaParsed);

	// Run target language
	let targetOutput: string;
	if (language === 'cpp') {
		targetOutput = executeCpp(testArgs);
	} else if (language === 'haxe') {
		targetOutput = executeHaxe(testArgs);
	} else {
		log_detail(`Unsupported target language: ${language}`);
		process.exit(1);
	}

	const targetParsed = parseOutput(targetOutput);
	saveJsonFiles(testArgs, targetParsed, javaParsed, fixFloats);
}

function verifyOutputsMatch (targetLanguage: string): void {
	const outputDir = join(SPINE_ROOT, 'tests', 'output');
	const outputFiles = [
		`skeleton-data-java-json.json`,
		`skeleton-data-${targetLanguage}-json.json`,
		`skeleton-state-java-json.json`,
		`skeleton-state-${targetLanguage}-json.json`,
		`skeleton-data-java-skel.json`,
		`skeleton-data-${targetLanguage}-skel.json`,
		`skeleton-state-java-skel.json`,
		`skeleton-state-${targetLanguage}-skel.json`
	];

	// Check if all files exist
	const missingFiles = outputFiles.filter(file => !existsSync(join(outputDir, file)));
	if (missingFiles.length > 0) {
		log_detail(`Skipping diff check - missing files: ${missingFiles.join(', ')}`);
		return;
	}

	log_action(`Verifying Java and ${targetLanguage} outputs match`);

	const comparisons: string[][] = [
		[`skeleton-data-java-json.json`, `skeleton-data-${targetLanguage}-json.json`],
		[`skeleton-state-java-json.json`, `skeleton-state-${targetLanguage}-json.json`],
		[`skeleton-data-java-skel.json`, `skeleton-data-${targetLanguage}-skel.json`],
		[`skeleton-state-java-skel.json`, `skeleton-state-${targetLanguage}-skel.json`]
	];

	// Add transition frame comparisons
	for (let i = 0; ; i++) {
		const javaFile = `transition-frame-${i}-java-json.json`;
		const targetFile = `transition-frame-${i}-${targetLanguage}-json.json`;
		if (!existsSync(join(outputDir, javaFile)) || !existsSync(join(outputDir, targetFile))) break;
		comparisons.push([javaFile, targetFile]);
	}
	for (let i = 0; ; i++) {
		const javaFile = `transition-frame-${i}-java-skel.json`;
		const targetFile = `transition-frame-${i}-${targetLanguage}-skel.json`;
		if (!existsSync(join(outputDir, javaFile)) || !existsSync(join(outputDir, targetFile))) break;
		comparisons.push([javaFile, targetFile]);
	}

	let allMatch = true;

	for (const [javaFile, targetFile] of comparisons) {
		try {
			const javaContent = execSync(`cat "${join(outputDir, javaFile)}"`, { encoding: 'utf8' });
			const targetContent = execSync(`cat "${join(outputDir, targetFile)}"`, { encoding: 'utf8' });

			if (javaContent !== targetContent) {
				allMatch = false;
				console.error(`\n❌ Files differ: ${javaFile} vs ${targetFile}`);
			}
		} catch (error: any) {
			allMatch = false;
			console.error(`\n❌ Error comparing ${javaFile} vs ${targetFile}: ${error.message}`);
		}
	}

	if (allMatch) {
		log_ok();
	} else {
		log_fail();
		console.error(`\n❌ Java and ${targetLanguage} outputs do not match`);
		process.exit(1);
	}
}

function main (): void {
	const args = validateArgs();

	log_title('Spine Runtime Test');

	// Clean output directory first
	// TODO annoying during development of generators as it also smokes generator outputs
	// cleanOutputDirectory();

	if (args.files) {
		// Auto-discovery mode: run tests for both JSON and binary files
		const jsonFile = args.files.jsonPath.split('/').pop() || args.files.jsonPath;
		const skelFile = args.files.skelPath.split('/').pop() || args.files.skelPath;
		const atlasFile = args.files.atlasPath.split('/').pop() || args.files.atlasPath;

		log_detail(`Files: ${jsonFile}, ${skelFile}, ${atlasFile}`);

		runTestsForFiles(args.language, args.files.jsonPath, args.files.atlasPath, args.animationName, args.animationName2, args.fixFloats);
		runTestsForFiles(args.language, args.files.skelPath, args.files.atlasPath, args.animationName, args.animationName2, args.fixFloats);

		log_summary('✓ All tests completed');
		log_detail(`JSON files saved to: ${join(SPINE_ROOT, 'tests', 'output')}`);
	} else {
		// Explicit paths mode: run test for single file
		runTestsForFiles(args.language, args.skeletonPath!, args.atlasPath!, args.animationName, args.animationName2, args.fixFloats);
		log_summary('✓ Test completed');
		log_detail(`JSON files saved to: ${join(SPINE_ROOT, 'tests', 'output')}`);
	}

	// Verify outputs match
	verifyOutputsMatch(args.language);
}

if (import.meta.url === `file://${process.argv[1]}`) {
	main();
}