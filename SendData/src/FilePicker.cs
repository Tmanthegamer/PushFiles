using System.IO;
using Microsoft.Win32;

namespace SendData {
    public class FilePicker {
        public FilePicker() {
            Dialog = new OpenFileDialog {Multiselect = true};
        }

        private OpenFileDialog Dialog { get; }

        public Stream[] SelectFiles(string dir = default(string)) {
            Dialog.InitialDirectory = dir == default(string) ? @"C:\" : dir;
            return Dialog.ShowDialog() == true ? Dialog.OpenFiles() : null;
        }
    }
}