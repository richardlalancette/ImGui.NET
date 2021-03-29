#pragma once

// This code should be put in a header file for your C++ DLL. It declares
// an extern C function that receives two parameters and is called SimulateGameDLL.
// I suggest putting it at the top of a header file.

extern "C"
{
	__declspec(dllexport) void __cdecl CreateTextEditor();
    __declspec(dllexport) void __cdecl DrawTextEditor();
}
