# Windows Phone Common Tools (Custom Branch)
Compilable source of most .NET based tools and libraries from the Windows Phone Common Tools package (Windows 10 Kits). 

My aim currently is to allow generation of FFUs that have been unlocked through WPInternal's Test Code aimed at unlocking a device from Mass Storage Mode

### What's different?
- Extended information on FFU generation to show partition stores and partitions being flushed when finalizing (wpimage, ImageCommon, ImageStorageServiceManager)
- Allowed *all* partitions to be processed in ffu generation, example is `backup_bs_nv` when a device is unlocked (ImageStorageServiceManager)


### Notes
- Referenced together to allow files to work together (If you use a custom exe with a stock library from the Kits you will get mismatched assembly error)
- I use a different folder to the default settings for my Windows Phone Common Tools install, you may need to reference the location to files from YOUR kit if needed (Only a couple references will need changed)
- Inspired from old decompiled source [IUTools Components Decompiled](https://github.com/Empyreal96/IUTool_components_decompiled)
- Used ILSpy to decompile the binaries
- Targets .NET Framework 4.6 with VS2019


### Libraries:

```
CabApiWrapper
deploytest
DeviceLayoutValidation
FeatureAPI
featuremerger
ffucomponents
ffutool
imageapp
imagecommon
imagecustomization
imagesigner
imagestorageservicemanaged
imaging
ImgDump
ImgToolsCommon
imgtowim
InboxAppLib
LOGUTILS
MCSFOffline
Microsoft.Phone.TestInfra.Deployment
Microsoft.Tools.IO
microsoft.windowsphone.security.securitypolicycompiler
microsoft.windowsphone.security.securitypolicyschema
MVOffline
OemCustomizationTool
PkgBldr.Common
PkgBldr.Plugin.CsiToCsi.Finalize
PkgBldr.Plugin.CsiToPkg.Base
PkgBldr.Plugin.PkgToWm.Base
PkgBldr.Plugin.WmToCsi.Capabilities
PkgBldr.Plugin.WmToCsi.Security
PkgBldr.Tools
PkgCommonManaged
PkgGen
PkgGen.Plugin.InboxApp
PkgGen.Plugin.MCSF
PkgGenCommon
pkgsigntool
PlatformManifest
secwimtool
spkggen
TestMetadataTool
ToolsCommon
UEFIUSBFnDevTester
USB_Test_Infrastructure
UtilityLibrary
WimInterop
wpimage
WPTCEditorV2
```
