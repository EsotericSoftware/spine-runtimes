/******************************************************************************
 * Spine Runtimes License Agreement
 * Last updated January 1, 2020. Replaces all prior versions.
 *
 * Copyright (c) 2013-2020, Esoteric Software LLC
 *
 * Integration of the Spine Runtimes into software or otherwise creating
 * derivative works of the Spine Runtimes is permitted under the terms and
 * conditions of Section 2 of the Spine Editor License Agreement:
 * http://esotericsoftware.com/spine-editor-license
 *
 * Otherwise, it is permitted to integrate the Spine Runtimes into software
 * or otherwise create derivative works of the Spine Runtimes (collectively,
 * "Products"), provided that each user of the Products must obtain their own
 * Spine Editor license and redistribution of the Products in any form must
 * include this license and copyright notice.
 *
 * THE SPINE RUNTIMES ARE PROVIDED BY ESOTERIC SOFTWARE LLC "AS IS" AND ANY
 * EXPRESS OR IMPLIED WARRANTIES, INCLUDING, BUT NOT LIMITED TO, THE IMPLIED
 * WARRANTIES OF MERCHANTABILITY AND FITNESS FOR A PARTICULAR PURPOSE ARE
 * DISCLAIMED. IN NO EVENT SHALL ESOTERIC SOFTWARE LLC BE LIABLE FOR ANY
 * DIRECT, INDIRECT, INCIDENTAL, SPECIAL, EXEMPLARY, OR CONSEQUENTIAL DAMAGES
 * (INCLUDING, BUT NOT LIMITED TO, PROCUREMENT OF SUBSTITUTE GOODS OR SERVICES,
 * BUSINESS INTERRUPTION, OR LOSS OF USE, DATA, OR PROFITS) HOWEVER CAUSED AND
 * ON ANY THEORY OF LIABILITY, WHETHER IN CONTRACT, STRICT LIABILITY, OR TORT
 * (INCLUDING NEGLIGENCE OR OTHERWISE) ARISING IN ANY WAY OUT OF THE USE OF
 * THE SPINE RUNTIMES, EVEN IF ADVISED OF THE POSSIBILITY OF SUCH DAMAGE.
 *****************************************************************************/

declare function CodeMirror (el: Element, config: any): void;

function loadScript (url: string): Promise<void> {
	return new Promise((resolve, reject) => {
		const script = document.createElement('script');
		script.src = url;
		script.onload = () => resolve();
		script.onerror = () => reject(new Error(`Script load error for ${url}`));
		document.head.appendChild(script);
	});
}

function loadCss (url: string): Promise<void> {
	return new Promise((resolve, reject) => {
		const link = document.createElement('link');
		link.href = url;
		link.rel = 'stylesheet';
		link.onload = () => resolve();
		link.onerror = () => reject(new Error(`CSS load error for ${url}`));
		document.head.appendChild(link);
	});
}

export class SpinePlayerEditor {
	private static DEFAULT_CODE =
		`
<script src="https://esotericsoftware.com/files/spine-player/4.1/spine-player.js"></script>
<link rel="stylesheet" href="https://esotericsoftware.com/files/spine-player/4.1/spine-player.css">

<div id="player-container" style="width: 100vw; height: 100vh;"></div>

<script>
new spine.SpinePlayer("player-container", {
	jsonUrl: "https://esotericsoftware.com/files/examples/4.1/spineboy/export/spineboy-pro.json",
	atlasUrl: "https://esotericsoftware.com/files/examples/4.1/spineboy/export/spineboy-pma.atlas"
});
</script>
		`.trim();

	private prefix: string =
		`<html>
<head>
<style>
body { margin: 0px; }
</style>
</head>
<body>`.trim()
	private postfix: string = `</body>`;
	private code: any;
	private player?: HTMLIFrameElement;

	constructor (private readonly parent: HTMLElement) {
		this.load();
	}

	private async load () {
		await Promise.all([loadScript("https://www.unpkg.com/codemirror@5.51.0/lib/codemirror.js"), loadCss("https://www.unpkg.com/codemirror@5.51.0/lib/codemirror.css")]);
		this.render(this.parent);
	}

	private render (parent: HTMLElement) {
		let dom = /*html*/`
				<div style="display: flex; flex-direction: column; width: 100%; height: 100%;">
					<div style="width: 100%; height: 50%"></div>
					<iframe style="width: 100%; height: 50%; outline: none; border: none;"></iframe>
				</div>
			`;
		parent.innerHTML = dom;
		let codeElement = parent.children[0].children[0];
		this.player = parent.children[0].children[1] as HTMLIFrameElement;

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

			(codeElement.children[0] as HTMLElement).style.height = "100%";
			this.setCode(SpinePlayerEditor.DEFAULT_CODE);
		})
	}

	setPreAndPostfix (prefix: string, postfix: string) {
		this.prefix = prefix;
		this.postfix = postfix;
		this.startPlayer()
	}

	setCode (code: string) {
		this.code.setValue(code);
		this.startPlayer();
	}

	private timerId = 0;
	startPlayer () {
		clearTimeout(this.timerId);
		this.timerId = setTimeout(() => {
			let code = this.code.getDoc().getValue();
			code = this.prefix + code + this.postfix;
			code = window.btoa(code);
			this.player!.src = "";
			this.player!.src = "data:text/html;base64," + code;
		}, 500);
	}
}
