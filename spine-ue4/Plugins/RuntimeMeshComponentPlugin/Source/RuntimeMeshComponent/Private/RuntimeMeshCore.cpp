// Copyright 2016 Chris Conway (Koderz). All Rights Reserved.

#include "RuntimeMeshComponentPluginPrivatePCH.h"
#include "RuntimeMeshCore.h"


FRuntimeMeshVertexTypeRegistrationContainer& FRuntimeMeshVertexTypeRegistrationContainer::GetInstance()
{
	static FRuntimeMeshVertexTypeRegistrationContainer Instance;
	return Instance;
}

void FRuntimeMeshVertexTypeRegistrationContainer::Register(const FRuntimeMeshVertexTypeInfo* InType)
{
	if (auto ExistingRegistration = Registrations.Find(InType->TypeGuid))
	{
		// This path only exists to support hotreload

		// If you hit this then you've probably either:
		// * Changed registration details during hotreload.
		// * Accidentally copy-and-pasted an FRuntimeMeshVertexTypeRegistration object.
		ensureMsgf(ExistingRegistration->TypeInfo->TypeName == InType->TypeName,
			TEXT("Runtime mesh vertex registrations cannot change between hotreloads - \"%s\" is being reregistered as \"%s\""),
			*ExistingRegistration->TypeInfo->TypeName, *InType->TypeName);

		ExistingRegistration->ReferenceCount++;
	}
	else
	{
		Registrations.Add(InType->TypeGuid, VertexRegistration(InType));
	}
}

void FRuntimeMeshVertexTypeRegistrationContainer::UnRegister(const FRuntimeMeshVertexTypeInfo* InType)
{
	auto ExistingRegistration = Registrations.Find(InType->TypeGuid);

	check(ExistingRegistration);

	ExistingRegistration->ReferenceCount--;
	if (ExistingRegistration->ReferenceCount == 0)
	{
		Registrations.Remove(InType->TypeGuid);
	}
}

const FRuntimeMeshVertexTypeInfo* FRuntimeMeshVertexTypeRegistrationContainer::GetVertexType(FGuid Key) const
{
	auto Registration = Registrations.Find(Key);
	return (Registration ? Registration->TypeInfo : nullptr);
}