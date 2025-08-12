
type PlatformInfoExportType = "preview" | "html5" | "scirra-arcade" | "cordova-android" | "cordova-ios" | "nwjs" | "windows-webview2" | "macos-wkwebview" | "xbox-uwp-webview2" | "instant-games" | "playable-ad" | "linux-cef";
type PlatformInfoOSType = "windows" | "macos" | "linux" | "chrome-os" | "android" | "ios" | "unknown";
type PlatformInfoBrowserType = "chrome" | "chromium" | "edge" | "opera" | "nwjs" | "firefox" | "safari" | "unknown";
type PlatformInfoBrowserEngineType = "chromium" | "gecko" | "webkit";

/** Provides details about the current platform, such as browser, operating system and environment. */
declare class IPlatformInfo
{
    readonly exportType: PlatformInfoExportType;
	readonly isMobile: boolean;
	readonly os: PlatformInfoOSType;
	readonly osVersion: string;
	readonly browser: PlatformInfoBrowserType;
	readonly browserVersion: string;
	readonly browserEngine: PlatformInfoBrowserEngineType;

	readonly renderer: string;
	readonly rendererDetail: string;

	readonly canvasClientX: number;
	readonly canvasClientY: number;
	readonly canvasCssWidth: number;
	readonly canvasCssHeight: number;
	readonly canvasDeviceWidth: number;
	readonly canvasDeviceHeight: number;
	readonly devicePixelRatio: number;
}
