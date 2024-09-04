namespace ArrayCollation;

public partial class MainWindow : Window
{
    public MainWindow()
    {
        InitializeComponent();
        MainWindowAC.Loaded += MainWindow_Loaded;
        tbStatus.Clear();
        btnCollate.Click += BtnCollate_Click;
        btnImportFile.Click += BtnImport_Click;
        cbDb.SelectionChanged += GetRowCount;
    }

    private async void GetRowCount(object sender, RoutedEventArgs e)
    {
        tbStatus.Clear();
        var progressStatus = new Progress<int>(s => SetStatus(s.ToString()));
        var resource = Consts.DATABASES.Where(r => r.Name == cbDb.SelectedValue.ToString()).FirstOrDefault();
        await Task.Run(() => 
        {
            using var db = new DataBasePG();
            db.GetRowCount(resource!.TableName, progressStatus);
        });
        return ;
    }


    private void MainWindow_Loaded(object sender, RoutedEventArgs e)
    {
        AddCbItems();
        FillInTb();
    }

    private void FillInTb()
    {
        tbGosusluga.Text = Properties.Settings.Default.tbGosusluga;
        tbResult.Text = Properties.Settings.Default.tbResult;
    }

    private void SavePaths()
    {
        Properties.Settings.Default.tbGosusluga = tbGosusluga.Text;
        Properties.Settings.Default.tbResult = tbResult.Text;
        Properties.Settings.Default.Save();
    }

    private void AddCbItems()
    {
        foreach (var resource in Consts.DATABASES)
        {
            cbDb.Items.Add(resource.Name);
        }
    }

    private string GetPath(string path)
    {
        if (!string.IsNullOrEmpty(path))
        {
            path = Path.Combine(path.Trim().Replace("\"", ""));
            if(Directory.Exists(path))
            {
                return path;
            }
        }
        return string.Empty;
    }

    private string GetFile(string file)
    {
        if (!string.IsNullOrEmpty(file))
        {
            file = file.Trim().Replace("\"", "");
            if (File.Exists(file))
            {
                return file;
            }
        }
        return string.Empty;
    }

    private void SetStatus(string status)
    {
        if (!string.IsNullOrEmpty(tbStatus.Text))
            tbStatus.Text += "\r\n";
        tbStatus.Text += status;
        tbStatus.ScrollToEnd();
    }

    private static bool YesNoDialog()
    {
        string sMessageBoxText = "Это действие перезапишит существующую таблицу. Продолжить?";
        string sCaption = "Предупреждение";

        MessageBoxButton btnMessageBox = MessageBoxButton.YesNo;
        MessageBoxImage icnMessageBox = MessageBoxImage.Warning;

        MessageBoxResult rsltMessageBox = MessageBox.Show(sMessageBoxText, sCaption, btnMessageBox, icnMessageBox);

        switch (rsltMessageBox)
        {
            case MessageBoxResult.Yes:
                return true;

            case MessageBoxResult.No:
                return false;
            default: return false;
        }
    }
}