
declare namespace SDK.UI {

	interface DragDropFileImportHandlerOpts {
		isZipFormat: boolean;
		toLayoutView: boolean;
	}

	interface DragDropFileImportHandlerCallbackOpts {
		layoutView: SDK.UI.ILayoutView;
		clientX: number;
		clientY: number;
		layoutX: number;
		layoutY: number;
	}

	class Util {
		static ShowLongTextPropertyDialog(initText: string, caption: string): Promise<string | null>;

		static AddDragDropFileImportHandler(callback: (filename: string, file: SDK.IZipFile | Blob, opts: DragDropFileImportHandlerCallbackOpts) => Promise<boolean>, opts?: DragDropFileImportHandlerOpts): void;
	}
}