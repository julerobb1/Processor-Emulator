# Feature Implementation Notes

## 📋 Image Asset Organizer for Firmware Analysis

### Overview
Add intelligent image asset discovery and organization to the existing firmware extraction pipeline. This would automatically categorize and present visual assets from extracted firmware for easier analysis and emulation setup.

### Integration Points
- **Existing**: `ArchiveExtractor.ExtractAndAnalyze()`
- **Existing**: `FirmwareAnalyzer.AnalyzeFirmwareArchive()`
- **Addition**: New `ImageAssetOrganizer` class

### Proposed Implementation

#### Core Features
1. **Automatic Image Discovery**
   - Scan extracted firmware recursively for image files (.png, .jpg, .gif, .svg, .bmp, .ico)
   - Categorize by purpose: splash screens, logos, UI assets, backgrounds
   - Detect boot/welcome screens by filename patterns

2. **Smart Categorization**
   - **Boot Assets**: dfb_splash, startup*, boot*, welcome*, intro*
   - **Branding**: logo*, brand*, comcast*, xfinity*, spark*
   - **UI Components**: control*, button*, icon*, arrow*
   - **Backgrounds**: background*, wallpaper*, theme*

3. **Integration with MainWindow Analysis**
   - Add "Image Assets" tab to firmware analysis results
   - Show thumbnails and paths for key images
   - Highlight potential boot screens for emulator use

#### Technical Implementation
```csharp
public class ImageAssetOrganizer
{
    public ImageAssetReport AnalyzeImages(string extractedPath)
    {
        // Scan for images
        // Categorize by patterns
        // Generate report with paths and metadata
        // Return organized results
    }
}

public class ImageAssetReport
{
    public List<ImageAsset> BootScreens { get; set; }
    public List<ImageAsset> Logos { get; set; }
    public List<ImageAsset> UIComponents { get; set; }
    public List<ImageAsset> Backgrounds { get; set; }
    public Dictionary<string, List<ImageAsset>> FolderGroups { get; set; }
}
```

### Current Status: NOTED FOR FUTURE IMPLEMENTATION
**Priority**: Medium (after boot functionality is working)
**Effort**: ~2-3 hours implementation
**Dependencies**: Current ArchiveExtractor and FirmwareAnalyzer working

---

## Current Focus: U-verse / Mediaroom (MIPS / WinCE)

Read `BOOT_STATUS.md`. That file is the evidenced boot map.

Hard Disk FAT named Hard Disk is already mounted and hooked
(sigcheckfilter HookVolume returned success). After that, firmware
does not CreateFile `\\ETC.bin` or any Hard Disk path. `nk.bin`
never Launchs `tv2clientce`. Hive-init Flags=3; RunApps `HKLM\init`
is `ERROR_BADKEY`. CreateProcess(`device.exe`) maps the TOC image
then OOM on binfs inherit-list VALLOC (`0x8001B724`). A host
window, black pane, or OEMIdle is not the TV UI.

Do not implement an X1 / Comcast splash, explorer, or a host
CreateProcess of the client. Do not invent ExtraROM map or SetEvent
of FSReady / the filesys pump. The real client is dump `etc.bin`
content, not a picture pasted on the host.

---

Image-asset organizer stays parked until a real guest UI exists.
