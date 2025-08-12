
type Shape3DShapeType = "box" | "prism" | "wedge" | "pyramid" | "corner-out" | "corner-in";
type Shape3DFaceType = "back" | "front" | "left" | "right" | "top" | "bottom";

/** Represents the 3D Shape object.
 * @see {@link https://www.construct.net/make-games/manuals/construct-3/scripting/scripting-reference/plugin-interfaces/3d-shape | I3DShapeInstance documentation } */
declare class I3DShapeInstance extends IWorldInstance
{
	zHeight: number;
	shape: Shape3DShapeType;
	zTilingFactor: number;

	getImagePointCount(): number;
	getImagePointX(nameOrIndex: ImagePointParameter): number;
	getImagePointY(nameOrIndex: ImagePointParameter): number;
	getImagePointZ(nameOrIndex: ImagePointParameter): number;
	getImagePoint(nameOrIndex: ImagePointParameter): Vec3Arr;

	getFaceImagePointCount(face: Shape3DFaceType): number;
	getFaceImagePointX(face: Shape3DFaceType, nameOrIndex: ImagePointParameter): number;
	getFaceImagePointY(face: Shape3DFaceType, nameOrIndex: ImagePointParameter): number;
	getFaceImagePointZ(face: Shape3DFaceType, nameOrIndex: ImagePointParameter): number;
	getFaceImagePoint(face: Shape3DFaceType, nameOrIndex: ImagePointParameter): Vec3Arr;

	setFaceVisible(face: Shape3DFaceType, isVisible: boolean): void;
	isFaceVisible(face: Shape3DFaceType): boolean;
	isBackFaceCulling: boolean;
	setFaceImage(face: Shape3DFaceType, image: Shape3DFaceType): void;
	setFaceObject<InstType extends IInstance>(face: Shape3DFaceType, objectClass: IObjectClass<InstType>): void;
}
