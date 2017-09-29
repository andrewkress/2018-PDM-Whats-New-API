using System;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using EPDM.Interop.epdm;
using System.IO;
using System.Windows;
using System.Windows.Forms;

namespace CardLists {
    class CardListViewModel : INotifyPropertyChanged {
        
        public CardListViewModel() {
            notBusy = true;
            vaultNames = new ObservableCollection<String>();
            cardListEntries = new ObservableCollection<String>();
            variableNames = new ObservableCollection<String>();
            populateVaultNames();
            GetCardListCommand = new Command(() => GetCardList());
            BrowseFileCommand = new Command(() => BrowseFile());
        }

        #region properties
        public event PropertyChangedEventHandler PropertyChanged;
        public ObservableCollection<String> vaultNames { get; set; }
        public ObservableCollection<String> cardListEntries { get; set; }
        public ObservableCollection<String> variableNames { get; set; }

        private String _selectedVault;
        private String _selectedFile;
        private String _selectedVariable;
        private bool _notBusy;

        public Command GetCardListCommand { get; }
        public Command BrowseFileCommand { get; }

        private IEdmVault5 vault;

        public String selectedVault {
            get { return _selectedVault; }
            set {
                _selectedVault = value;
                OnPropertyChanged();
            }
        }

        public String selectedFile {
            get { return _selectedFile; }
            set {
                _selectedFile = value;
                populateVariableNames();
                selectedVariable = variableNames[0];
                OnPropertyChanged();
            }
        }

        public String selectedVariable {
            get { return _selectedVariable; }
            set {
                _selectedVariable = value;
                OnPropertyChanged();
            }
        }

        public bool notBusy {
            get { return _notBusy; }
            private set {
                _notBusy = value;
                OnPropertyChanged();
            }
        }
        #endregion

        protected void OnPropertyChanged([CallerMemberName] String propertyName = null) {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        private void populateVariableNames() {
            vault = new EdmVault5();
            vault.LoginAuto(selectedVault, 0);
            if (vault.IsLoggedIn) {
                variableNames.Clear();
                IEdmVariableMgr5 varMgr = (IEdmVariableMgr5)((IEdmVault7)vault).CreateUtility(EdmUtility.EdmUtil_VariableMgr);
                IEdmPos5 varPos = varMgr.GetFirstVariablePosition();
                while (!varPos.IsNull) {
                    IEdmVariable5 varToAdd = varMgr.GetNextVariable(varPos);
                    variableNames.Add(varToAdd.Name);
                }
            }
        }

        private void populateVaultNames() {
            vault = new EdmVault5();
            ((IEdmVault8)vault).GetVaultViews(out EdmViewInfo[] views, false);
            foreach (EdmViewInfo view in views) {
                vaultNames.Add(view.mbsVaultName);
            }
        }

        #region commands
        private void GetCardList() {
            notBusy = false;
            try {
                vault = new EdmVault5();
                vault.LoginAuto(selectedVault, 0);
                if (vault.IsLoggedIn) {
                    IEdmFile5 file = vault.GetFileFromPath(selectedFile, out IEdmFolder5 folder);
                    IEdmCard5 card = folder.GetCard(Path.GetExtension(selectedFile).Substring(1));
                    Object variableName = selectedVariable;
                    IEdmCardControl7 cardControl = (IEdmCardControl7)card.GetControl(card.GetControlID(ref variableName));
                    if (cardControl.GetControlVariableList(file.ID, out String[] cardListStrings)) {
                        cardListEntries.Clear();
                        foreach (String cardListItem in cardListStrings) {
                            cardListEntries.Add(cardListItem);
                        }
                    }
                }
            } catch (System.Runtime.InteropServices.COMException ex) {
                System.Windows.MessageBox.Show($"HRESULT = 0x{ex.ErrorCode.ToString("X")} {ex.Message}", "Interop Error", MessageBoxButton.OK, MessageBoxImage.Error);
            } catch (Exception ex) {
                System.Windows.MessageBox.Show(ex.Message, "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
            notBusy = true;
        }

        private void BrowseFile() {
            OpenFileDialog fileBrowser = new OpenFileDialog();
            fileBrowser.Multiselect = false;
            fileBrowser.Filter = "All files (*.*)|*.*";
            if (fileBrowser.ShowDialog() == DialogResult.OK) {
                selectedFile = fileBrowser.FileName;
            }
        }
        #endregion
    }
}
