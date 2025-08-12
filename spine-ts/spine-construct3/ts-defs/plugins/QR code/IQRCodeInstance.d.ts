
type QRCodeCorrectionLevel = "l" | "m" | "q" | "h";

declare class IQRCodeInstance extends IWorldInstance
{
	text: string;
	correctionLevel: QRCodeCorrectionLevel;
}
