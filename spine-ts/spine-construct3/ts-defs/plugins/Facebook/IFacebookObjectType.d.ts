
/** Represents the Facebook object. */
declare class IFacebookObjectType<InstType extends IInstance = IInstance> extends IObjectType<InstType>
{
	getAccessToken(): string;
}
