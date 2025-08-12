declare namespace SDK {
	class IFamily extends IObjectClass {
		GetMembers(): SDK.IObjectType[];
		SetMembers(memberS: SDK.IObjectType[]): void;
	}
}