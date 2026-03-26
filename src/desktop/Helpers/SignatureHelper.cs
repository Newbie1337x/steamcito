using System;
using System.Runtime.InteropServices;

namespace steamcito.Helpers;

public static class SignatureHelper
{
    private static readonly Guid WINTRUST_ACTION_GENERIC_VERIFY_V2 =
        new Guid("00AAC56B-CD44-11d0-8CC2-00C04FC295EE");

    private const uint WTD_UI_NONE = 2;
    private const uint WTD_REVOKE_NONE = 0x00000000;
    private const uint WTD_CHOICE_FILE = 1;
    private const uint WTD_REVOCATION_CHECK_NONE = 0x00000040;

    [DllImport("wintrust.dll", CharSet = CharSet.Unicode, ExactSpelling = true)]
    private static extern uint WinVerifyTrust(
        IntPtr hwnd,
        [MarshalAs(UnmanagedType.LPStruct)] Guid pgActionID,
        ref WINTRUST_DATA pWVTData);

    public static bool IsWindowsSignatureValid(string filePath)
    {
        WINTRUST_FILE_INFO fileInfo = new WINTRUST_FILE_INFO();
        fileInfo.cbStruct = (uint)Marshal.SizeOf(typeof(WINTRUST_FILE_INFO));
        fileInfo.pcwszFilePath = filePath;
        fileInfo.hFile = IntPtr.Zero;
        fileInfo.pgKnownSubject = IntPtr.Zero;

        IntPtr pFileInfo = Marshal.AllocHGlobal(Marshal.SizeOf(typeof(WINTRUST_FILE_INFO)));
        Marshal.StructureToPtr(fileInfo, pFileInfo, false);

        WINTRUST_DATA data = new WINTRUST_DATA();
        data.cbStruct = (uint)Marshal.SizeOf(typeof(WINTRUST_DATA));
        data.pPolicyCallbackData = IntPtr.Zero;
        data.pSIPClientData = IntPtr.Zero;
        data.dwUIChoice = WTD_UI_NONE;
        data.fdwRevocationChecks = WTD_REVOKE_NONE;
        data.dwUnionChoice = WTD_CHOICE_FILE;
        data.pFile = pFileInfo;
        data.dwStateAction = 0; // WTD_STATEACTION_IGNORE
        data.hWVTStateData = IntPtr.Zero;
        data.pwszURLReference = IntPtr.Zero;
        data.dwProvFlags = WTD_REVOCATION_CHECK_NONE;
        data.dwUIContext = 0;

        uint result = WinVerifyTrust(IntPtr.Zero, WINTRUST_ACTION_GENERIC_VERIFY_V2, ref data);

        Marshal.FreeHGlobal(pFileInfo);

        return result == 0; // 0 = TRUST_E_OK
    }

    [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Unicode)]
    private struct WINTRUST_FILE_INFO
    {
        public uint cbStruct;
        public string pcwszFilePath;
        public IntPtr hFile;
        public IntPtr pgKnownSubject;
    }

    [StructLayout(LayoutKind.Sequential)]
    private struct WINTRUST_DATA
    {
        public uint cbStruct;
        public IntPtr pPolicyCallbackData;
        public IntPtr pSIPClientData;
        public uint dwUIChoice;
        public uint fdwRevocationChecks;
        public uint dwUnionChoice;
        public IntPtr pFile;
        public uint dwStateAction;
        public IntPtr hWVTStateData;
        public IntPtr pwszURLReference;
        public uint dwProvFlags;
        public uint dwUIContext;
    }
}