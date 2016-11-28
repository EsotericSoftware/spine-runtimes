#pragma once 

#include <string>

// Error reporting with levels similar to Android and are automatically forwarded to Continuous integration server
enum ErrorLevel {
	WARNLVL,
	ERRORLVL,
	INFOLVL,
	DEBUGLVL
};

extern void KOutputDebug(ErrorLevel lvl, const char* fmt ...);

extern std::string GetFileName(const std::string& thePath, bool noExtension);
extern std::string GetFileDir(const std::string& thePath, bool withSlash);
extern std::string GetFileExt(const std::string& thePath);

extern int CompareNoCase(const std::string& str1, const std::string& str2);
