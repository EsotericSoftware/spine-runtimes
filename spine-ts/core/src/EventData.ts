module spine {
    export class EventData {
        name: string;
	    intValue: number;
	    floatValue: number;
	    stringValue: string;

        constructor (name: string) {
            this.name = name;
        }
    }
}