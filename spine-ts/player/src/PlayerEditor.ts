declare function CodeMirror(el: Element, config: any): void;

module spine {
	export class SpinePlayerEditor {
		private static DEFAULT_CODE =
		`
<script src="https://esotericsoftware.com/files/spine-player/3.7/spine-player.js"></script>
<link rel="stylesheet" href="https://esotericsoftware.com/files/spine-player/3.7/spine-player.css">

<div id="player-container" style="width: 100%; height: 100vh;"></div>

<script>
new spine.SpinePlayer("player-container", {
	jsonUrl: "https://esotericsoftware.com/files/examples/spineboy/export/spineboy-pro.json",
	atlasUrl: "https://esotericsoftware.com/files/examples/spineboy/export/spineboy-pma.atlas"
});
</script>
		`.trim();

		private prefix: string =
`<html>
<head>
<style>
body {
	margin: 0px;
}
</style>
</head>
<body>`.trim()
		private postfix: string = `</body>`;
		private code: any;
		private player: HTMLIFrameElement;

		constructor(parent: HTMLElement) {
			this.render(parent);
		}

		private render (parent: HTMLElement) {
			let dom = /*html*/`
				<div class="spine-player-editor-container">
					<div class="spine-player-editor-code"></div>
					<iframe class="spine-player-editor-player"></iframe>
				</div>
			`;
			parent.innerHTML = dom;
			let codeElement = parent.getElementsByClassName("spine-player-editor-code")[0];
			this.player = parent.getElementsByClassName("spine-player-editor-player")[0] as HTMLIFrameElement;

			requestAnimationFrame(() => {
				this.code = CodeMirror(codeElement, {
					lineNumbers: true,
					tabSize: 3,
					indentUnit: 3,
					indentWithTabs: true,
					scrollBarStyle: "native",
					mode: "htmlmixed",
					theme: "monokai"
				});
				this.code.on("change", () => {
					this.startPlayer();
				});

				this.setCode(SpinePlayerEditor.DEFAULT_CODE);
			})
		}

		setPreAndPostfix(prefix: string, postfix: string) {
			this.prefix = prefix;
			this.postfix = postfix;
			this.startPlayer()
		}

		setCode(code: string) {
			this.code.setValue(code);
			this.startPlayer();
		}

		private timerId = 0;
		startPlayer() {
			clearTimeout(this.timerId);
			this.timerId = setTimeout( () => {
				let code = this.code.getDoc().getValue();
				code = this.prefix + code + this.postfix;
				code = window.btoa(code);
				this.player.src = "";
				this.player.src = "data:text/html;base64," + code;
			}, 500);
		}
	}
}