module spine {
    export interface Map<T> {
        [key: string]: T;
    }

    export interface Disposable {
        dispose(): void;
    }

    export class Color {        
        constructor(public r: number, public g: number, public b: number, public a: number) {            
        }

        set(r: number, g: number, b: number, a: number) {
            this.r = r;
            this.g = g;
            this.b = b;
            this.a = a;
            this.clamp();
        }

        add(r: number, g: number, b: number, a: number) {
            this.r += r;
            this.g += g;
            this.b += b;
            this.a += a;
            this.clamp();
        }

        clamp() {
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
        static clamp(value: number, min: number, max: number) {
            if (value < min) return min;
            if (value > max) return max;
            return value;
        }
    }

    export class Utils {
        static arrayCopy(source: Array<number>, sourceStart: number, dest: Array<Number>, destStart: number, numElements: number) {
            for (var i = sourceStart, j = destStart; i < sourceStart + numElements; i++, j++) {
                dest[j] = source[i];
            }
        }  

        static setArraySize(array: Array<number>, size: number): Array<number> {
            let oldSize = array.length;
            array.length = size;
            if (oldSize < size) {
                for (var i = oldSize; i < size; i++) array[i] = 0;
            }
            return array;
        } 
    }
}