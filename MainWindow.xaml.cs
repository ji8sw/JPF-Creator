using System.Text;
using System.Windows;
using Microsoft.Win32;
using System.IO;
using System.Windows.Controls;
using System.Data.HashFunction.CityHash;
using System.Data.HashFunction;

namespace JPF_Creator
{
    // Must be 0-255 to fit inside of a single byte
    enum FileType
    {
        PNG,
        JPG,
        OBJ,
        TXT,
        HLSL, // DirectX Shader code
        VERT, // Vertex Shader Code
        FRAG, // Fragment Shader Code
        VERTC, // Vertex Shader (Compiled)
        FRAGC, // Fragment Shader (Compiled)
        FX, // Shader Effect Code
        FXC, // Shader Effect (Compiled)
        // Add More
        UNK = 255
    }

    public partial class MainWindow : Window
    {
        private List<FileData> SelectedFiles = new List<FileData>();

        public MainWindow()
        {
            InitializeComponent();
        }

        private void BeginSelectingFiles(object Sender, RoutedEventArgs Event)
        {
            OpenFileDialog FileDialog = new OpenFileDialog
            {
                Title = "Select Game Assets",
                Filter = "All Files|*.*",
                Multiselect = true,
                CheckFileExists = true,
                CheckPathExists = true,
            };

            if (FileDialog.ShowDialog() == true)
            {
                // begin loading all the selected files on seperate threads
                foreach (string FilePath in FileDialog.FileNames)
                {
                    FileData Info = new FileData(FilePath);
                    FileButtonList.Items.Add(Info.AssociatedButton);
                    SelectedFiles.Add(Info);
                }
            }
        }

        private void ClearSelectedFiles(object Sender, RoutedEventArgs Event)
        {
            FileButtonList.Items.Clear();
            SelectedFiles.Clear();
        }

        public bool SaveByteArrayToFile(byte[] Data)
        {
            Stream DataStream;
            SaveFileDialog SaveJPFDialog = new SaveFileDialog();
            SaveJPFDialog.Filter = "Ji9sw Package File (*.jpf)|*.jpf";
            SaveJPFDialog.RestoreDirectory = true;

            if (SaveJPFDialog.ShowDialog() == true)
            {
                if ((DataStream = SaveJPFDialog.OpenFile()) != null)
                {
                    DataStream.Write(Data, 0, Data.Length);
                    DataStream.Close();
                    return true;
                }
            }

            return false;
        }

        private void CompileJPF(object Sender, RoutedEventArgs Event)
        {
            List<byte> JPFData = new List<byte>();
            JPFData.AddRange(Encoding.ASCII.GetBytes("JPF")); // Add "JPF" Magic Header

            int TotalFiles = 0;
            foreach (FileData File in SelectedFiles)
            {
                JPFData.AddRange(File.GetAsJPFData());
                TotalFiles++;
            }

            if (SaveByteArrayToFile(JPFData.ToArray()))
                MessageBox.Show($"Compiled {TotalFiles}/{SelectedFiles.Count} assets into 1 JPF");
        }

        private void ViewSelectedFile(object Sender, RoutedEventArgs Event)
        {
            Event.Source.GetType();
        }
    }

    public class FileData
    {
        public string FilePath { get; }
        private byte[] Data { get; set; }
        public Button AssociatedButton { get; set; }

        public FileData(string filePath)
        {
            FilePath = filePath;

            AssociatedButton = new Button();
            AssociatedButton.Width = 169;
            AssociatedButton.Content = Path.GetFileName(filePath);
        }

        private void LoadFile()
        {
            try
            {
                using (var Stream = new FileStream(FilePath, FileMode.Open, FileAccess.Read))
                {
                    Data = new byte[Stream.Length];
                    int BytesRead = 0;
                    while (BytesRead < Data.Length)
                    {
                        int read = Stream.Read(Data, BytesRead, Data.Length - BytesRead);
                        BytesRead += read;
                    }
                }
            }
            catch (Exception Ex)
            {
                MessageBox.Show($"Error reading asset: {FilePath}\n{Ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private async Task LoadFileAsync()
        {
            try
            {
                await using (var Stream = new FileStream(FilePath, FileMode.Open, FileAccess.Read))
                {
                    Data = new byte[Stream.Length];
                    int BytesRead = 0;
                    while (BytesRead < Data.Length)
                    {
                        int read = Stream.Read(Data, BytesRead, Data.Length - BytesRead);
                        BytesRead += read;
                    }
                }
            }
            catch (Exception Ex)
            {
                Console.WriteLine($"Error reading asset: {FilePath}\n{Ex.Message}");
            }
        }

        public byte[] GetAsJPFData()
        {
            Task.Run(() => LoadFileAsync()).Wait();
            if (Data == null) return new byte[0];
            const int NameHashLength = 8; // 8 bytes to store the file name as a CityHash64
            const int FileTypeLength = 1; // 1 byte to store the file type
            const int FileSizeLength = 4; // 4 bytes to store the file size in bytes
            byte[] JPFData = new byte[NameHashLength + FileTypeLength + FileSizeLength + Data.Length];
            int TotalCopied = 0;

            string FileName = Path.GetFileName(FilePath);

            CityHashConfig CityHashConfig = new CityHashConfig();
            CityHashConfig.HashSizeInBits = 64;
            byte[] FileNameHashData = CityHashFactory.Instance.Create(CityHashConfig).ComputeHash(FileName).Hash;
            Array.Copy(FileNameHashData, 0, JPFData, TotalCopied, NameHashLength); // Add File Name Hash
            TotalCopied += NameHashLength;

            FileType FileTypeData = FileType.UNK;
            string Extension = Path.GetExtension(FilePath).ToLower();
            switch (Extension)
            {
                case ".png":
                    FileTypeData = FileType.PNG;
                    break;
                case ".jpg":
                    FileTypeData = FileType.JPG;
                    break;
                case ".obj":
                    FileTypeData = FileType.OBJ;
                    break;
                case ".txt":
                    FileTypeData = FileType.TXT;
                    break;
                case ".hlsl":
                    FileTypeData = FileType.HLSL;
                    break;
                case ".vert":
                    FileTypeData = FileType.VERT;
                    break;
                case ".frag":
                    FileTypeData = FileType.FRAG;
                    break;
                case ".vertc":
                    FileTypeData = FileType.VERTC;
                    break;
                case ".fragc":
                    FileTypeData = FileType.FRAGC;
                    break;
                case ".fx":
                    FileTypeData = FileType.FX;
                    break;
                case ".fxc":
                    FileTypeData = FileType.FXC;
                    break;
            }
            JPFData[TotalCopied] = (byte)FileTypeData;
            TotalCopied += FileTypeLength;

            int FileSize = Data.Length;
            byte[] FileSizeData = BitConverter.GetBytes(FileSize);
            Array.Copy(FileSizeData, 0, JPFData, TotalCopied, FileSizeData.Length); // Add File Size
            TotalCopied += FileSizeLength;

            Array.Copy(Data, 0, JPFData, TotalCopied, Data.Length); // Add File Data

            return JPFData;
        }
    }
}