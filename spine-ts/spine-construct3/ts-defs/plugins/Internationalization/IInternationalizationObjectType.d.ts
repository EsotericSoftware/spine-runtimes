
type I18nPlaceholderType = string | number;

declare class I18NLookupContext
{
    lookup(context: string, ...args: I18nPlaceholderType[]): string;
    lookupPlural(context: string, count: number, ...args: I18nPlaceholderType[]): string;
    createContext(context: string): I18NLookupContext;
}

declare class IInternationalizationObjectType<InstType extends IInstance = IInstance> extends IObjectType<InstType>
{
	locale: string;

    addString(context: string, str: string): void;
    saveToJSONString(): string;
    loadFromJSONString(jsonStr: string): void;

    setContext(context: string): void;
    getContext(): string;
    pushContext(context: string): void;
    popContext(): void;
    createContext(context: string): I18NLookupContext;
    lookup(context: string, ...args: I18nPlaceholderType[]): string;
    lookupPlural(context: string, count: number, ...args: I18nPlaceholderType[]): string;
}
