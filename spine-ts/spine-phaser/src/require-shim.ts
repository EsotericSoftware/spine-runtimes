declare global {
	var require: any;
}
if (window.Phaser) {
    let prevRequire = window.require;
    window.require = (x: string) => {
        if (prevRequire) return prevRequire(x);
        else if (x === "Phaser") return window.Phaser;
    }
}
export {}