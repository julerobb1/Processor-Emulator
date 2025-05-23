using ProcessorEmulator.Emulation;
using ProcessorEmulator.Tools;
using ProcessorEmulator.Architectures;
using System.Windows;

namespace ProcessorEmulator
{
    public partial class MainWindow : Window
    {
        private IEmulator currentEmulator;
        private ArchitectureDetector archDetector = new ArchitectureDetector();
        private PartitionAnalyzer partitionAnalyzer = new PartitionAnalyzer();
        private Disassembler disassembler = new Disassembler();
        private Recompiler recompiler = new Recompiler();

        public MainWindow()
        {
            InitializeComponent();
        }

        private void StartEmulation_Click(object sender, RoutedEventArgs e)
        {
            // Example: Load a binary, detect architecture, and select emulator
            byte[] binary = new byte[0]; // TODO: Load from file
            string arch = archDetector.Detect(binary);
            switch (arch)
            {
                case "MIPS32": currentEmulator = new Mips32Emulator(); break;
                case "MIPS64": currentEmulator = new Mips64Emulator(); break;
                case "ARM": currentEmulator = new ArmEmulator(); break;
                case "ARM64": currentEmulator = new Arm64Emulator(); break;
                case "PowerPC": currentEmulator = new PowerPcEmulator(); break;
                case "x86": currentEmulator = new X86Emulator(); break;
                case "x86-64": currentEmulator = new X64Emulator(); break;
                default: MessageBox.Show("Unknown architecture"); return;
            }
            currentEmulator.LoadBinary(binary);
            currentEmulator.Run();
        }
    }
}
