
type Camera3DLayoutAxes = "x" | "y" | "z";
type Camera3DCameraAxes = "forward" | "up" | "right";
type Camera3DMoveType = "both" | "camera" | "look";

/** Represents the 3D Camera object.
 * @see {@link https://www.construct.net/make-games/manuals/construct-3/scripting/scripting-reference/plugin-interfaces/3d-camera | I3DCameraObjectType documentation } */
declare class I3DCameraObjectType<InstType extends IInstance = IInstance> extends IObjectType<InstType>
{
	lookAtPosition(camX: number, camY: number, camZ: number, lookX: number, lookY: number, lookZ: number, upX: number, upY: number, upZ: number): void;
	lookParallelToLayout(camX: number, camY: number, camZ: number, lookAngle: number): void;
	restore2DCamera(): void;
	moveAlongLayoutAxis(distance: number, axisStr: Camera3DLayoutAxes, whichStr: Camera3DMoveType): void;
	moveAlongCameraAxis(distance: number, axisStr: Camera3DCameraAxes, whichStr: Camera3DMoveType): void;
	rotateCamera(rotateX: number, rotateY: number, minPolar: number, maxPolar: number): void;
	fieldOfView: number;

	getCameraPosition(): Vec3Arr;
	getLookPosition(): Vec3Arr;
	getUpVector(): Vec3Arr;
	getRightVector(): Vec3Arr;
	getForwardVector(): Vec3Arr;
	getLookVector(): Vec3Arr;
	readonly zScale: number;
}
