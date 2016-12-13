#pragma once

#include "Components/ActorComponent.h"
#include "SpineBoneFollowerComponent.generated.h"


UCLASS(ClassGroup=(Custom), meta=(BlueprintSpawnableComponent))
class SPINEPLUGIN_API USpineBoneFollowerComponent : public UActorComponent {
	GENERATED_BODY()

public:
	UPROPERTY(EditAnywhere, BlueprintReadWrite)
	AActor* Target = 0;

	UPROPERTY(EditAnywhere, BlueprintReadWrite)
	FString BoneName;

	UPROPERTY(EditAnywhere, BlueprintReadWrite)
	bool UsePosition = true;

	UPROPERTY(EditAnywhere, BlueprintReadWrite)
	bool UseRotation = true;

	UPROPERTY(EditAnywhere, BlueprintReadWrite)
	bool UseScale = true;
	
	USpineBoneFollowerComponent ();
	
	virtual void BeginPlay () override;
		
	virtual void TickComponent (float DeltaTime, ELevelTick TickType, FActorComponentTickFunction* ThisTickFunction) override;
};
