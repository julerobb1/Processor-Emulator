using System;
using System.Collections.Generic;
using System.Diagnostics;

public class VirtualRegistry
{
    private Dictionary<string, object> _hklm = new Dictionary<string, object>();

    public VirtualRegistry()
    {
        // Mediaroom often looks for these specific "Authorized" indicators
        _hklm[@"Software\Microsoft\Mediaroom\Client\IsAuthorized"] = 1;
        _hklm[@"Software\Microsoft\Mediaroom\Client\ProvisioningState"] = "Provisioned";
        _hklm[@"Software\Microsoft\Mediaroom\Client\AccountGuid"] = "00000000-0000-0000-0000-000000000001";
        _hklm[@"Hardware\SerialNumber"] = "SPOOFED12345";
    }

    public uint ReadValue(string path)
    {
        if (_hklm.ContainsKey(path))
        {
            var val = _hklm[path];
            Debug.WriteLine($"[VirtualRegistry] Reading path '{path}', returning: {val}");
            return val is int ? (uint)(int)val : 1; // Default to success for non-int values
        }
        Debug.WriteLine($"[VirtualRegistry] Path '{path}' not found. Returning 0.");
        return 0; // Not found
    }
}