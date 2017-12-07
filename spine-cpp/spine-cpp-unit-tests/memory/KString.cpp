#include "KString.h" 
#include <stdarg.h>

#include "MiniCppUnit.hxx"

///////////////////////////////////////////////////////////////////////////////
///////////////////////////////////////////////////////////////////////////////
// String Helper 

static std::string vasprintf(const char* fmt, va_list argv)
{
	std::string result;
	va_list argv_copy; // vsnprintf modifies argv, need copy
#ifndef va_copy
	argv_copy = argv;
#else
	va_copy(argv_copy, argv);
#endif

	int len = vsnprintf(NULL, 0, fmt, argv_copy);
	if (len > 0 && len < 255)
	{
		// cover 90% of all calls
		char str[256] = { 0 };
		int len2 = vsnprintf(str, 255, fmt, argv);
		result = str;
	}
	else if (len > 0)
	{
		char* str = static_cast<char*>(alloca(len + 1)); // alloca on stack, space for null-termination
		int len2 = vsnprintf(str, len + 1, fmt, argv);
		result = str;
	}
	return result;
}


static void reportWarning(const std::string& warnStr)
{
	if (JetBrains::underTeamcity())
		gTeamCityListener.messages.messageWarning(warnStr);
	else
		fprintf(stderr, "%s", warnStr.c_str());
}

static void reportError(const std::string& errorStr)
{
	if (JetBrains::underTeamcity())
		gTeamCityListener.messages.messageError(errorStr);
	else
		fprintf(stderr, "%s", errorStr.c_str());
}

static void reportInfo(const std::string& infoStr)
{
	if (JetBrains::underTeamcity())
		gTeamCityListener.messages.messageNormal(infoStr);
	else
		fprintf(stderr, "%s", infoStr.c_str());
}

static void reportDebug(const std::string& debugStr)
{
	fprintf(stderr, "%s", debugStr.c_str());
}

static void report(ErrorLevel level, const std::string& Str)
{
	switch (level) {
	case WARNLVL: reportWarning(Str); break;
	case ERRORLVL: reportError(Str); break;
	case INFOLVL: reportInfo(Str); break;
	case DEBUGLVL: reportDebug(Str); break;
	}
}

void KOutputDebug(ErrorLevel lvl, const char* fmt ...)
{
	va_list argList;
	va_start(argList, fmt);
	std::string str = vasprintf(fmt, argList);
	va_end(argList);

	report(lvl, str);
}

#define K_MAX(a,b) ((a>b) ? a : b)

std::string GetFileName(const std::string& thePath, bool noExtension)
{
	int aLastSlash = K_MAX((int)thePath.rfind('\\'), (int)thePath.rfind('/'));

	if (noExtension)
	{
		int aLastDot = (int)thePath.rfind('.');
		if (aLastDot > aLastSlash)
			return thePath.substr(aLastSlash + 1, aLastDot - aLastSlash - 1);
	}

	if (aLastSlash == -1)
		return thePath;
	else
		return thePath.substr(aLastSlash + 1);
}

std::string GetFileDir(const std::string& thePath, bool withSlash)
{
	int aLastSlash = K_MAX((int)thePath.rfind(('\\')), (int)thePath.rfind(('/')));

	if (aLastSlash == -1)
		return ("");
	else
	{
		if (withSlash)
			return thePath.substr(0, aLastSlash + 1);
		else
			return thePath.substr(0, aLastSlash);
	}
}

std::string GetFileExt(const std::string& thePath)
{
	std::string::size_type idx = thePath.find_last_of('.');

	if (idx != std::string::npos)
		return thePath.substr(idx + 1);

	return ("");
}

/**
 * g_ascii_strcasecmp:
 * @s1: string to compare with @s2.
 * @s2: string to compare with @s1.
 *
 * Compare two strings, ignoring the case of ASCII characters.
 *
 * Unlike the BSD strcasecmp() function, this only recognizes standard
 * ASCII letters and ignores the locale, treating all non-ASCII
 * bytes as if they are not letters.
 *
 * This function should be used only on strings that are known to be
 * in encodings where the bytes corresponding to ASCII letters always
 * represent themselves. This includes UTF-8 and the ISO-8859-*
 * charsets, but not for instance double-byte encodings like the
 * Windows Codepage 932, where the trailing bytes of double-byte
 * characters include all ASCII letters. If you compare two CP932
 * strings using this function, you will get false matches.
 *
 * Return value: an integer less than, equal to, or greater than
 *               zero if @s1 is found, respectively, to be less than,
 *               to match, or to be greater than @s2.
 **/
static int g_ascii_compare_caseless(const char* s1, const char* s2)
{
#define TOUPPER(c)  (((c) >= 'a' && (c) <= 'z') ? (c) - 'a' + 'A' : (c))
#define TOLOWER(c)  (((c) >= 'A' && (c) <= 'Z') ? (c) - 'A' + 'a' : (c))
#define g_return_val_if_fail(expr,val) { if (!(expr)) return (val); }

	int c1, c2;

	g_return_val_if_fail(s1 != NULL, 0);
	g_return_val_if_fail(s2 != NULL, 0);

	while (*s1 && *s2)
	{
		c1 = (int)(unsigned char)TOLOWER(*s1);
		c2 = (int)(unsigned char)TOLOWER(*s2);
		if (c1 != c2)
			return (c1 - c2);
		s1++; s2++;
	}

	return (((int)(unsigned char)* s1) - ((int)(unsigned char)* s2));

#undef g_return_val_if_fail
#undef TOUPPER
#undef TOLOWER
}


int CompareNoCase(const std::string & str1, const std::string & str2)
{
	return g_ascii_compare_caseless(str1.c_str(), str2.c_str());
}
