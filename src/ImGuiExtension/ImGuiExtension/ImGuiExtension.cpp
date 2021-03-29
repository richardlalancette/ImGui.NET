#include "pch.h"

#include "ImGuiExtension.h"


// See also: https://www.dotnetperls.com/dllimport
// Helpers to debug issues with the DLL
// dumpbin.exe /headers .\ImGuiExtension.dll |rg machine
// dumpbin /DEPENDENTS .\ImGuiExtension.dll
// kernel32.lib;user32.lib;gdi32.lib;winspool.lib;comdlg32.lib;advapi32.lib;shell32.lib;ole32.lib;oleaut32.lib;uuid.lib;odbc32.lib;odbccp32.lib;%(AdditionalDependencies)
// The keywords and parameter types must match
// ... the above extern declaration.
extern void __cdecl CreateTextEditor()
{
}

extern void __cdecl DrawTextEditor()
{
}
