import dts from 'rollup-plugin-dts'

export default [
	{
		input: 'build/index.js',
		context: 'this',
		output: [
			{
				file: '../build/spine-core.js',
				name: 'spine',
				format: 'umd',
				exports: 'named',
			}
		]
	}
]