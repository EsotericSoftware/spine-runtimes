module spine {
    export interface Map<T> {
        [key: string]: T;
    }

    export interface Disposable {
        dispose(): void;
    }
}