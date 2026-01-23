/******************************************************************************
 * Spine Runtimes License Agreement
 * Last updated April 5, 2025. Replaces all prior versions.
 *
 * Copyright (c) 2013-2025, Esoteric Software LLC
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

interface ModalButton<T> {
	text: string;
	color?: string;
	value?: T;
	style?: 'primary' | 'secondary';
}

interface ModalOptions<T> {
	darkMode: boolean;
	title: string;
	text: string;
	buttons: ModalButton<T>[];
	maxWidth?: number;
}

export function showModal<T> (options: ModalOptions<T>): Promise<T | undefined> {
	return new Promise((resolve) => {
		const { title, text, buttons, darkMode = false } = options;

		const theme = darkMode ? {
			overlayBg: 'rgba(0, 0, 0, 0.5)',
			captionBg: 'rgb(71, 71, 71)',        // gray9
			captionText: 'rgb(214, 214, 214)',   // gray27
			contentBg: 'rgb(87, 87, 87)',        // gray11
			contentText: 'rgb(214, 214, 214)',   // gray27
			buttonBg: 'rgb(71, 71, 71)',         // gray9
			buttonText: 'rgb(214, 214, 214)',    // gray27
			buttonBorder: 'rgb(56, 56, 56)',     // gray7
			buttonHoverBg: 'rgb(79, 79, 79)',    // gray10
			closeColor: 'rgb(168, 168, 168)',    // gray21
		} : {
			overlayBg: 'rgba(0, 0, 0, 0.3)',
			captionBg: 'rgb(247, 247, 247)',     // gray31
			captionText: 'rgb(94, 94, 94)',      // gray12
			contentBg: 'rgb(232, 232, 232)',     // gray29
			contentText: 'rgb(94, 94, 94)',      // gray12
			buttonBg: 'rgb(222, 222, 222)',      // gray28
			buttonText: 'rgb(94, 94, 94)',       // gray12
			buttonBorder: 'rgb(199, 199, 199)',  // gray25
			buttonHoverBg: 'rgb(214, 214, 214)', // gray27
			closeColor: 'rgb(94, 94, 94)',       // gray12
		};

		const overlay = document.createElement('div');
		overlay.style.cssText = `
			position: fixed;
			top: 0;
			left: 0;
			width: 100%;
			height: 100%;
			background: ${theme.overlayBg};
			display: flex;
			align-items: center;
			justify-content: center;
			z-index: 999999;
			font-family: system-ui, -apple-system, "Segoe UI", Roboto, "Helvetica Neue", sans-serif;
			font-size: 14px;
		  `;

		const dialog = document.createElement('div');
		dialog.style.cssText = `
			background: ${theme.contentBg};
			border-radius: 6px;
			min-width: 200px;
			max-width: 550px;
			display: flex;
			flex-direction: column;
			filter: drop-shadow(0 4px 5px rgba(10,10,10,0.35)) drop-shadow(0 2px 1px rgba(10,10,10,0.5));
		  `;

		const caption = document.createElement('div');
		caption.style.cssText = `
			background: ${theme.captionBg};
			color: ${theme.captionText};
			padding: 6px 10px;
			display: flex;
			align-items: center;
			justify-content: space-between;
			user-select: none;
			border-radius: 6px 6px 0 0;
		  `;

		const titleSpan = document.createElement('span');
		titleSpan.textContent = title;

		const closeBtn = document.createElement('button');
		closeBtn.innerHTML = `<svg xmlns="http://www.w3.org/2000/svg" width="14" height="14" viewBox="0 0 24 24" fill="${theme.closeColor}"><path d="M19 6.41L17.59 5 12 10.59 6.41 5 5 6.41 10.59 12 5 17.59 6.41 19 12 13.41 17.59 19 19 17.59 13.41 12z"/></svg>`;
		closeBtn.style.cssText = `
			background: transparent;
			border: none;
			cursor: pointer;
			padding: 2px;
			display: flex;
			align-items: center;
			justify-content: center;
			border-radius: 3px;
			opacity: 0.7;
		  `;
		closeBtn.onmouseover = () => { closeBtn.style.opacity = '1'; };
		closeBtn.onmouseout = () => { closeBtn.style.opacity = '0.7'; };

		caption.appendChild(titleSpan);
		caption.appendChild(closeBtn);

		const contents = document.createElement('div');
		contents.style.cssText = `
			padding: 12px 14px;
			color: ${theme.contentText};
			line-height: 1.4;
		  `;
		contents.textContent = text;

		const footer = document.createElement('div');
		footer.style.cssText = `
			padding: 8px 14px 12px;
			display: flex;
			justify-content: flex-end;
			gap: 6px;
		  `;

		const cleanup = () => {
			document.removeEventListener('keydown', handleKeyDown);
			overlay.remove();
		};

		closeBtn.addEventListener('click', () => {
			cleanup();
			resolve(undefined);
		});

		buttons.forEach((buttonConfig, index) => {
			const btn = document.createElement('button');
			btn.textContent = buttonConfig.text;
			btn.style.cssText = `
			  padding: 4px 14px;
			  border: 1px solid ${theme.buttonBorder};
			  border-radius: 3px;
			  background: ${theme.buttonBg};
			  color: ${theme.buttonText};
			  font-size: 14px;
			  font-family: inherit;
			  cursor: pointer;
			`;
			btn.onmouseover = () => { btn.style.background = theme.buttonHoverBg; };
			btn.onmouseout = () => { btn.style.background = theme.buttonBg; };

			btn.addEventListener('click', () => {
				cleanup();
				resolve(buttonConfig.value);
			});

			footer.appendChild(btn);

			if (index === buttons.length - 1) {
				setTimeout(() => btn.focus(), 0);
			}
		});

		overlay.addEventListener('click', (e) => {
			if (e.target === overlay) {
				cleanup();
				resolve(undefined);
			}
		});

		const handleKeyDown = (e: KeyboardEvent) => {
			if (e.key === 'Escape') {
				cleanup();
				resolve(undefined);
			}
		};
		document.addEventListener('keydown', handleKeyDown);

		dialog.appendChild(caption);
		dialog.appendChild(contents);
		dialog.appendChild(footer);
		overlay.appendChild(dialog);
		document.body.appendChild(overlay);
	});
}

