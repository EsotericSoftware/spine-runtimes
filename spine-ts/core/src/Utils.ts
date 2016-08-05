module spine {
    export interface Map<T> {
        [key: string]: T;
    }

    export interface Disposable {
        dispose (): void;
    }

    export class Color {        
        constructor( public r: number = 0, public g: number = 0, public b: number = 0, public a: number = 0) {            
        }

        set (r: number, g: number, b: number, a: number) {
            this.r = r;
            this.g = g;
            this.b = b;
            this.a = a;
            this.clamp();
        }

        setFromColor(c: Color) {
            this.r = c.r;
            this.g = c.g;
            this.b = c.b;
            this.a = c.a;
        }

        add (r: number, g: number, b: number, a: number) {
            this.r += r;
            this.g += g;
            this.b += b;
            this.a += a;
            this.clamp();
        }

        clamp () {
            if (this.r < 0)
                this.r = 0;
            else if (this.r > 1) this.r = 1;

            if (this.g < 0)
                this.g = 0;
            else if (this.g > 1) this.g = 1;

            if (this.b < 0)
                this.b = 0;
            else if (this.b > 1) this.b = 1;

            if (this.a < 0)
                this.a = 0;
            else if (this.a > 1) this.a = 1;
            return this;
        }
    }

    export class MathUtils {
        static PI = 3.1415927;
	    static PI2 = MathUtils.PI * 2;        	
	    static radiansToDegrees = 180 / MathUtils.PI;
	    static radDeg = MathUtils.radiansToDegrees;	
	    static degreesToRadians = MathUtils.PI / 180;
	    static degRad = MathUtils.degreesToRadians;

        static clamp (value: number, min: number, max: number) {
            if (value < min) return min;
            if (value > max) return max;
            return value;
        }

        static cosDeg (degrees: number) {
            return Math.cos(degrees * MathUtils.degRad);
        }

        static sinDeg (degrees: number) {
            return Math.sin(degrees * MathUtils.degRad);
        }

        static signum (value: number): number {
            return value >= 0? 1: -1;            
        }
    }

    export class Utils {
        static arrayCopy<T> (source: Array<T>, sourceStart: number, dest: Array<T>, destStart: number, numElements: number) {
            for (var i = sourceStart, j = destStart; i < sourceStart + numElements; i++, j++) {
                dest[j] = source[i];
            }
        }  

        static setArraySize (array: Array<number>, size: number): Array<number> {            
            let oldSize = array.length;
            if (oldSize == size) return;
            array.length = size;
            if (oldSize < size) {
                for (var i = oldSize; i < size; i++) array[i] = 0;
            }
            return array;
        } 
    }

    export class Vector2 {        
        constructor (public x = 0, public y = 0) {            
        }

        set (x: number, y: number): Vector2 {
            this.x = x;
            this.y = y;
            return this;
        }        
    }
}