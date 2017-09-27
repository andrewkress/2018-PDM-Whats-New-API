using System;
using System.Windows;
using EPDM.Interop.epdm;
using System.Windows.Interop;
using System.Reflection;
using System.IO;

namespace CustomTabs {
    public class CustomTabs : IEdmAddIn5 {
        
        private String tabName = "GSC";
        private String iconName = "GSC.png";
        private String toolTip = "GSC Custom Tab View";
        private String uniqueId = "F4C84E2B-3D7B-4FD2-B65A-3323C82D65BD"; // generated a guid to use, can be any unique string

        public void GetAddInInfo(ref EdmAddInInfo poInfo, IEdmVault5 poVault, IEdmCmdMgr5 poCmdMgr) {
            try {
                poInfo.mbsAddInName = "PDM Custom Tab API Example";
                poInfo.mbsCompany = "GSC";
                poInfo.mbsDescription = "This AddIn will show the basics of what is needed to create your own custom tab in PDM Professional.";
                poInfo.mlAddInVersion = 20170926;
                poInfo.mlRequiredVersionMajor = 17;
                poInfo.mlRequiredVersionMinor = 5;  // Need to use 17.5 until 18 SP0 is released
                
                // need to add both a hook and a command to create the tab.  Since I am not using the command, I am only showing it in admin tool
                poCmdMgr.AddHook(EdmCmdType.EdmCmd_PreExploreInit);
                poCmdMgr.AddCmd(12321415, "CTC", (int)EdmMenuFlags.EdmMenu_NeverInContextMenu + (int)EdmMenuFlags.EdmMenu_Administration);
                // hook to intercept when the tab is activated
                poCmdMgr.AddHook(EdmCmdType.EdmCmd_ActivateAPITab);
            } catch (System.Runtime.InteropServices.COMException ex) {
                MessageBox.Show($"HRESULT = 0x{ex.ErrorCode.ToString("X")} {ex.Message}", "Interop Error", MessageBoxButton.OK, MessageBoxImage.Error);
            } catch (Exception ex) {
                MessageBox.Show(ex.Message, "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        public void OnCmd(ref EdmCmd poCmd, ref EdmCmdData[] ppoData) {
            try {
                switch (poCmd.meCmdType) {
                    case EdmCmdType.EdmCmd_PreExploreInit:
                        createCustomTabView(poCmd.mpoExtra);
                        break;
                    case EdmCmdType.EdmCmd_ActivateAPITab:
                        MessageBox.Show("The custom tab has been activated!", "Activate", MessageBoxButton.OK, MessageBoxImage.Exclamation);
                        break;
                    default:
                        break;
                }
            } catch (System.Runtime.InteropServices.COMException ex) {
                MessageBox.Show($"HRESULT = 0x{ex.ErrorCode.ToString("X")} {ex.Message}", "Interop Error", MessageBoxButton.OK, MessageBoxImage.Error);
            } catch (Exception ex) {
                MessageBox.Show(ex.Message, "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void createCustomTabView(Object poCmdMgr) {

            String iconLocation = Path.Combine(getAssemblyDirectory(), iconName);

            // create control
            WinForm wf = new WinForm();
            //CustomTabControl ctc = new CustomTabControl();

            // get control id
            //long windowHandle = new WindowInteropHelper(ctc).EnsureHandle().ToInt64();
            long windowHandle = wf.Handle.ToInt64();

            // call to add the tab
            ((IEdmCmdMgr6)poCmdMgr).AddVaultViewTab(windowHandle, tabName, iconLocation, toolTip, uniqueId);
        }

        // method to allow the png to be saved in the vault
        private String getAssemblyDirectory() {
            String codeBase = Assembly.GetExecutingAssembly().CodeBase;
            UriBuilder uri = new UriBuilder(codeBase);
            String path = Uri.UnescapeDataString(uri.Path);
            return Path.GetDirectoryName(path);
        }
    }
}
