module spine {
    export abstract class Attachment {
        name: string;

        constructor (name: string) {
            if (name == null) throw new Error("name cannot be null.");
            this.name = name;
        }
    }
}