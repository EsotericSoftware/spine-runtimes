import dts from 'rollup-plugin-dts'

export default [
	{
		input: 'spine-core/dist/index.js',
		context: 'this',
		output: [
			{
				file: 'build/spine-core.js',
				name: 'spine',
				format: 'umd',
				exports: 'named',
			}
		]
	},
	{
		input: 'spine-core/dist/index.d.ts',
		output: [{ file: 'build/spine-core.d.ts', format: 'es' }],
		plugins: [dts()],
	},

	{
		input: 'spine-canvas/dist/index.js',
		context: 'this',
		output: [
			{
				file: 'build/spine-canvas.js',
				name: 'spine',
				format: 'umd',
				exports: 'named',
			}
		]
	}
]