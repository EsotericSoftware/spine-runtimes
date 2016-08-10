module spine {
	export class PathAttachment extends VertexAttachment {
		lengths: Array<number>;
		closed = false; constantSpeed = false;

		constructor (name: string) {
			super(name);
		}
	}
}
