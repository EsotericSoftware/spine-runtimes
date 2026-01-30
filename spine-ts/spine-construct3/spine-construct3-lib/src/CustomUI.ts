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

interface ModalTheme {
	overlayBg: string;
	captionBg: string;
	captionText: string;
	contentBg: string;
	contentText: string;
	buttonBg: string;
	buttonText: string;
	buttonBorder: string;
	buttonHoverBg: string;
	closeColor: string;
	itemHoverBg: string;
	itemSelectedBg: string;
}

function getTheme (darkMode: boolean): ModalTheme {
	return darkMode ? {
		overlayBg: 'rgba(0, 0, 0, 0.5)',
		captionBg: 'rgb(71, 71, 71)',
		captionText: 'rgb(214, 214, 214)',
		contentBg: 'rgb(87, 87, 87)',
		contentText: 'rgb(214, 214, 214)',
		buttonBg: 'rgb(71, 71, 71)',
		buttonText: 'rgb(214, 214, 214)',
		buttonBorder: 'rgb(56, 56, 56)',
		buttonHoverBg: 'rgb(79, 79, 79)',
		closeColor: 'rgb(168, 168, 168)',
		itemHoverBg: 'rgb(79, 79, 79)',
		itemSelectedBg: 'rgb(56, 56, 56)',
	} : {
		overlayBg: 'rgba(0, 0, 0, 0.3)',
		captionBg: 'rgb(247, 247, 247)',
		captionText: 'rgb(94, 94, 94)',
		contentBg: 'rgb(232, 232, 232)',
		contentText: 'rgb(94, 94, 94)',
		buttonBg: 'rgb(222, 222, 222)',
		buttonText: 'rgb(94, 94, 94)',
		buttonBorder: 'rgb(199, 199, 199)',
		buttonHoverBg: 'rgb(214, 214, 214)',
		closeColor: 'rgb(94, 94, 94)',
		itemHoverBg: 'rgb(214, 214, 214)',
		itemSelectedBg: 'rgb(199, 199, 199)',
	};
}

function createOverlay (theme: ModalTheme): HTMLDivElement {
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
	return overlay;
}

function createDialog (theme: ModalTheme, minWidth: number): HTMLDivElement {
	const dialog = document.createElement('div');
	dialog.style.cssText = `
		background: ${theme.contentBg};
		border-radius: 6px;
		min-width: ${minWidth}px;
		max-width: 550px;
		display: flex;
		flex-direction: column;
		filter: drop-shadow(0 4px 5px rgba(10,10,10,0.35)) drop-shadow(0 2px 1px rgba(10,10,10,0.5));
	`;
	return dialog;
}

function createCaption (title: string, theme: ModalTheme, onClose: () => void): HTMLDivElement {
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
	closeBtn.addEventListener('click', onClose);

	caption.appendChild(titleSpan);
	caption.appendChild(closeBtn);

	return caption;
}

function createFooter (): HTMLDivElement {
	const footer = document.createElement('div');
	footer.style.cssText = `
		padding: 8px 14px 12px;
		display: flex;
		justify-content: flex-end;
		gap: 6px;
	`;
	return footer;
}

function createButton (text: string, theme: ModalTheme, onClick: () => void): HTMLButtonElement {
	const btn = document.createElement('button');
	btn.textContent = text;
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
	btn.addEventListener('click', onClick);
	return btn;
}

function setupModalHandlers (overlay: HTMLDivElement, onCancel: () => void, extraKeyHandler?: (e: KeyboardEvent) => boolean) {
	const handleKeyDown = (e: KeyboardEvent) => {
		if (e.key === 'Escape') {
			onCancel();
			return;
		}
		if (extraKeyHandler?.(e)) {
			return;
		}
	};

	setTimeout(() => {
		document.addEventListener('keydown', handleKeyDown);
	}, 0);

	overlay.addEventListener('click', (e) => {
		if (e.target === overlay) {
			onCancel();
		}
	});

	return () => {
		document.removeEventListener('keydown', handleKeyDown);
		overlay.remove();
	};
}

interface ModalButton<T> {
	text: string;
	value?: T;
	style?: 'primary' | 'secondary';
}

interface ModalOptions<T> {
	darkMode: boolean;
	title: string;
	text: string;
	buttons: ModalButton<T>[];
}

export function showModal<T> (options: ModalOptions<T>): Promise<T | undefined> {
	return new Promise((resolve) => {
		const { title, text, buttons, darkMode = false } = options;

		const theme = getTheme(darkMode);
		const overlay = createOverlay(theme);
		const dialog = createDialog(theme, 200);

		const cleanup = setupModalHandlers(overlay, () => {
			cleanup();
			resolve(undefined);
		});

		const caption = createCaption(title, theme, () => {
			cleanup();
			resolve(undefined);
		});

		const contents = document.createElement('div');
		contents.style.cssText = `
			padding: 12px 14px;
			color: ${theme.contentText};
			line-height: 1.4;
		`;
		contents.textContent = text;

		const footer = createFooter();

		buttons.forEach((buttonConfig, index) => {
			const btn = createButton(buttonConfig.text, theme, () => {
				cleanup();
				resolve(buttonConfig.value);
			});
			footer.appendChild(btn);

			if (index === buttons.length - 1) {
				setTimeout(() => btn.focus(), 0);
			}
		});

		dialog.appendChild(caption);
		dialog.appendChild(contents);
		dialog.appendChild(footer);
		overlay.appendChild(dialog);
		document.body.appendChild(overlay);
	});
}

interface ListSelectionOptions {
	darkMode: boolean;
	title: string;
	items: string[];
	maxHeight?: number;
}

export function showListSelectionModal (options: ListSelectionOptions): Promise<string | undefined> {
	return new Promise((resolve) => {
		const { title, items, darkMode = false, maxHeight = 400 } = options;

		const theme = getTheme(darkMode);
		const overlay = createOverlay(theme);
		const dialog = createDialog(theme, 300);

		let selectedItem: string | undefined;

		const cleanup = setupModalHandlers(overlay, () => {
			cleanup();
			resolve(undefined);
		}, (e) => {
			if (e.key === 'Enter' && selectedItem) {
				cleanup();
				resolve(selectedItem);
				return true;
			}
			return false;
		});

		const caption = createCaption(title, theme, () => {
			cleanup();
			resolve(undefined);
		});

		const contents = document.createElement('div');
		contents.style.cssText = `
			padding: 12px 0;
			color: ${theme.contentText};
			max-height: ${maxHeight}px;
			overflow-y: auto;
		`;

		items.forEach((item) => {
			const itemDiv = document.createElement('div');
			itemDiv.textContent = item;
			itemDiv.style.cssText = `
				padding: 8px 14px;
				cursor: pointer;
				user-select: none;
			`;

			itemDiv.addEventListener('click', () => {
				contents.querySelectorAll('div').forEach(div => {
					div.style.background = 'transparent';
				});
				itemDiv.style.background = theme.itemSelectedBg;
				selectedItem = item;
			});

			itemDiv.addEventListener('mouseover', () => {
				if (itemDiv.style.background !== theme.itemSelectedBg) {
					itemDiv.style.background = theme.itemHoverBg;
				}
			});

			itemDiv.addEventListener('mouseout', () => {
				if (itemDiv.style.background !== theme.itemSelectedBg) {
					itemDiv.style.background = 'transparent';
				}
			});

			contents.appendChild(itemDiv);
		});

		const footer = createFooter();

		const cancelBtn = createButton('Cancel', theme, () => {
			cleanup();
			resolve(undefined);
		});

		const okBtn = createButton('OK', theme, () => {
			cleanup();
			resolve(selectedItem);
		});

		footer.appendChild(cancelBtn);
		footer.appendChild(okBtn);

		dialog.appendChild(caption);
		dialog.appendChild(contents);
		dialog.appendChild(footer);
		overlay.appendChild(dialog);
		document.body.appendChild(overlay);

		setTimeout(() => okBtn.focus(), 0);
	});
}

interface AlertOptions {
	darkMode: boolean;
	title: string;
	message: string;
}

export function showAlertModal (options: AlertOptions): Promise<void> {
	return new Promise((resolve) => {
		const { title, message, darkMode = false } = options;

		const theme = getTheme(darkMode);
		const overlay = createOverlay(theme);
		const dialog = createDialog(theme, 300);

		const cleanup = setupModalHandlers(overlay, () => {
			cleanup();
			resolve();
		}, (e) => {
			if (e.key === 'Enter') {
				cleanup();
				resolve();
				return true;
			}
			return false;
		});

		const caption = createCaption(title, theme, () => {
			cleanup();
			resolve();
		});

		const contents = document.createElement('div');
		contents.style.cssText = `
			padding: 12px 14px;
			color: ${theme.contentText};
			line-height: 1.4;
		`;
		contents.textContent = message;

		const footer = createFooter();

		const okBtn = createButton('OK', theme, () => {
			cleanup();
			resolve();
		});

		footer.appendChild(okBtn);

		dialog.appendChild(caption);
		dialog.appendChild(contents);
		dialog.appendChild(footer);
		overlay.appendChild(dialog);
		document.body.appendChild(overlay);

		setTimeout(() => okBtn.focus(), 0);
	});
}

interface MultiListSelectionOptions {
	darkMode: boolean;
	title: string;
	items: string[];
	selectedItems?: string[];
	maxHeight?: number;
}

export function showMultiListSelectionModal (options: MultiListSelectionOptions): Promise<string[] | undefined> {
	return new Promise((resolve) => {
		const { title, items, selectedItems = [], darkMode = false, maxHeight = 400 } = options;

		const theme = getTheme(darkMode);
		const overlay = createOverlay(theme);
		const dialog = createDialog(theme, 300);

		const selectedSet = new Set(selectedItems);

		const cleanup = setupModalHandlers(overlay, () => {
			cleanup();
			resolve(undefined);
		}, (e) => {
			if (e.key === 'Enter') {
				cleanup();
				resolve(Array.from(selectedSet));
				return true;
			}
			return false;
		});

		const caption = createCaption(title, theme, () => {
			cleanup();
			resolve(undefined);
		});

		const contents = document.createElement('div');
		contents.style.cssText = `
			padding: 12px 0;
			color: ${theme.contentText};
			max-height: ${maxHeight}px;
			overflow-y: auto;
		`;

		items.forEach((item) => {
			const itemDiv = document.createElement('div');
			itemDiv.style.cssText = `
				padding: 8px 14px;
				cursor: pointer;
				user-select: none;
				display: flex;
				align-items: center;
				gap: 8px;
			`;

			const checkbox = document.createElement('input');
			checkbox.type = 'checkbox';
			checkbox.checked = selectedSet.has(item);
			checkbox.style.cssText = `
				cursor: pointer;
			`;

			const label = document.createElement('span');
			label.textContent = item;
			label.style.cssText = `
				flex: 1;
				cursor: pointer;
			`;

			itemDiv.appendChild(checkbox);
			itemDiv.appendChild(label);

			const toggleSelection = () => {
				if (selectedSet.has(item)) {
					selectedSet.delete(item);
					checkbox.checked = false;
				} else {
					selectedSet.add(item);
					checkbox.checked = true;
				}
			};

			itemDiv.addEventListener('click', (e) => {
				if (e.target !== checkbox) {
					toggleSelection();
				}
			});

			checkbox.addEventListener('change', () => {
				if (checkbox.checked) {
					selectedSet.add(item);
				} else {
					selectedSet.delete(item);
				}
			});

			itemDiv.addEventListener('mouseover', () => {
				itemDiv.style.background = theme.itemHoverBg;
			});

			itemDiv.addEventListener('mouseout', () => {
				itemDiv.style.background = 'transparent';
			});

			contents.appendChild(itemDiv);
		});

		const footer = createFooter();

		const cancelBtn = createButton('Cancel', theme, () => {
			cleanup();
			resolve(undefined);
		});

		const okBtn = createButton('OK', theme, () => {
			cleanup();
			resolve(Array.from(selectedSet));
		});

		footer.appendChild(cancelBtn);
		footer.appendChild(okBtn);

		dialog.appendChild(caption);
		dialog.appendChild(contents);
		dialog.appendChild(footer);
		overlay.appendChild(dialog);
		document.body.appendChild(overlay);

		setTimeout(() => okBtn.focus(), 0);
	});
}
