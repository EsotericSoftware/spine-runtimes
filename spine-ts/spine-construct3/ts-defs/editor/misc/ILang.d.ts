
declare namespace SDK.Lang {
	function PushContext(str: string): void;
	function PopContext(): void;
	function Get(s: string): string;
}

// Global lang() method
declare function lang(s: string): string;
