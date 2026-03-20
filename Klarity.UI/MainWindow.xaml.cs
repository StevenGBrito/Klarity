using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Media;
using System.Collections.ObjectModel;
using System.Text.RegularExpressions;
using System.Windows.Threading;
using Microsoft.Win32;
using Klarity.Core.RoslynIntegration;
using Klarity.Core.CFG;
using Klarity.Core.DFA;
using Klarity.Core.Taint;
using Klarity.Core.Quality;
using System.IO;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using Forms = System.Windows.Forms;

namespace Klarity.UI;

public class FileExplorerNode : INotifyPropertyChanged
{
    private bool _hasErrors;
    public string Name { get; set; } = string.Empty;
    public string FullPath { get; set; } = string.Empty;
    public bool IsFile { get; set; }
    public ObservableCollection<FileExplorerNode> Children { get; } = [];

    public bool HasErrors
    {
        get => _hasErrors;
        set
        {
            if (_hasErrors == value) return;
            _hasErrors = value;
            OnPropertyChanged();
            OnPropertyChanged(nameof(StatusBrush));
        }
    }

    public System.Windows.Media.Brush StatusBrush => HasErrors 
        ? new System.Windows.Media.SolidColorBrush(System.Windows.Media.Color.FromRgb(255, 82, 82))   // Rojo
        : new System.Windows.Media.SolidColorBrush(System.Windows.Media.Color.FromRgb(166, 226, 46)); // Verde (como la IA)

    public event PropertyChangedEventHandler? PropertyChanged;
    protected void OnPropertyChanged([CallerMemberName] string? name = null)
        => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
}

public partial class MainWindow : Window
{
    // ── State ─────────────────────────────────────────────────────────────────
    public ObservableCollection<VulnerabilityItem> Vulnerabilities { get; } = [];
    private readonly DispatcherTimer _debounce = new() { Interval = TimeSpan.FromMilliseconds(800) };
    private bool _busy;
    private string? _currentFilePath;
    private readonly List<string> _allProjectFiles = [];
    private readonly Dictionary<string, FileExplorerNode> _pathNodes = [];

    // ── Init ──────────────────────────────────────────────────────────────────
    public MainWindow()
    {
        _debounce.Tick += (_, _) => { 
            _debounce.Stop(); 
            var results = Analyze(GetCode(), _currentFilePath ?? "Edición en vivo", true);
            if (_currentFilePath != null && _pathNodes.TryGetValue(_currentFilePath, out var node))
            {
                node.HasErrors = results > 0;
                UpdateParentStatus(node);
            }
        };
        InitializeComponent();
        ResultsList.ItemsSource = Vulnerabilities;
    }

    // ── Editor Events ─────────────────────────────────────────────────────────
    private void CodeEditor_TextChanged(object s, TextChangedEventArgs e)
    {
        if (_busy) return;
        UpdateLineNumbers();
        _debounce.Stop();
        _debounce.Start();
    }

    private void CodeEditor_ScrollChanged(object s, ScrollChangedEventArgs e)
        => LineNumberScroll?.ScrollToVerticalOffset(e.VerticalOffset);

    private void ResultsList_SelectionChanged(object s, SelectionChangedEventArgs e)
    {
        try
        {
            if (ResultsList.SelectedItem is VulnerabilityItem item && !string.IsNullOrEmpty(item.FullFilePath))
            {
                if (item.FullFilePath != _currentFilePath)
                {
                    LoadFile(item.FullFilePath, false); // Cargar sin re-analizar
                }
                
                if (item.LineNumber > 0)
                    GetBlock(item.LineNumber)?.BringIntoView();
            }
        }
        catch (Exception ex)
        {
            StatusText.Text = $"Error al navegar: {ex.Message}";
        }
    }

    // ── File Load ─────────────────────────────────────────────────────────────
    private void LoadFileButton_Click(object s, RoutedEventArgs e)
    {
        var dlg = new Microsoft.Win32.OpenFileDialog { Filter = "Archivos C# (*.cs)|*.cs|Todos (*.*)|*.*" };
        if (dlg.ShowDialog() != true) return;
        LoadFile(dlg.FileName);
    }

    private void LoadProjectButton_Click(object s, RoutedEventArgs e)
    {
        using var dlg = new Forms.FolderBrowserDialog { Description = "Seleccione la carpeta del proyecto C#" };
        if (dlg.ShowDialog() != Forms.DialogResult.OK) return;

        FileExplorer.Items.Clear();
        _allProjectFiles.Clear();
        _pathNodes.Clear();
        var root = new FileExplorerNode { Name = Path.GetFileName(dlg.SelectedPath), FullPath = dlg.SelectedPath, IsFile = false };
        _pathNodes[dlg.SelectedPath] = root;
        PopulateExplorer(dlg.SelectedPath, root);
        FileExplorer.Items.Add(root);
        StatusText.Text = $"Proyecto cargado: {root.Name}. Analizando todos los archivos...";
        
        AnalyzeAllFiles();
    }

    private void PopulateExplorer(string path, FileExplorerNode parent)
    {
        foreach (var dir in Directory.GetDirectories(path))
        {
            if (Path.GetFileName(dir).StartsWith('.') || Path.GetFileName(dir) == "bin" || Path.GetFileName(dir) == "obj") continue;
            var node = new FileExplorerNode { Name = Path.GetFileName(dir), FullPath = dir, IsFile = false };
            _pathNodes[dir] = node;
            PopulateExplorer(dir, node);
            if (node.Children.Count > 0 || Directory.GetFiles(dir, "*.cs").Any()) 
                parent.Children.Add(node);
        }

        foreach (var file in Directory.GetFiles(path, "*.cs"))
        {
            _allProjectFiles.Add(file);
            var node = new FileExplorerNode { Name = Path.GetFileName(file), FullPath = file, IsFile = true };
            _pathNodes[file] = node;
            parent.Children.Add(node);
        }
    }

    private void FileExplorer_SelectedItemChanged(object s, RoutedPropertyChangedEventArgs<object> e)
    {
        if (e.NewValue is FileExplorerNode { IsFile: true } node)
            LoadFile(node.FullPath);
    }

    private void LoadFile(string path, bool triggerAnalysis = true)
    {
        _currentFilePath = path;
        var code = File.ReadAllText(path);
        SetCode(code);
        StatusText.Text = $"Cargado: {Path.GetFileName(path)}";
        if (triggerAnalysis)
        {
            Vulnerabilities.Clear();
            Analyze(code, path, false);
        }
    }

    // ── Analysis ──────────────────────────────────────────────────────────────
    private async void AnalyzeAllFiles()
    {
        _busy = true;
        Vulnerabilities.Clear();
        int total = _allProjectFiles.Count;
        int count = 0;

        foreach (var file in _allProjectFiles)
        {
            count++;
            StatusText.Text = $"Analizando archivo {count} de {total}: {Path.GetFileName(file)}";
            var code = await Task.Run(() => File.ReadAllText(file));
            int findings = Analyze(code, file, false);
            
            if (_pathNodes.TryGetValue(file, out var node))
            {
                node.HasErrors = findings > 0;
                UpdateParentStatus(node);
            }
        }

        if (Vulnerabilities.Count == 0)
            Add("No se detectaron vulnerabilidades en el proyecto.", "-", "Global", "Global", line: 0, sug: string.Empty);

        StatusText.Text = $"Análisis de proyecto completado. {Vulnerabilities.Count} hallazgos.";
        _busy = false;
    }

    private void UpdateParentStatus(FileExplorerNode node)
    {
        var parentPath = Path.GetDirectoryName(node.FullPath);
        if (parentPath != null && _pathNodes.TryGetValue(parentPath, out var parent))
        {
            bool anyChildHasError = parent.Children.Any(c => c.HasErrors);
            if (parent.HasErrors != anyChildHasError)
            {
                parent.HasErrors = anyChildHasError;
                UpdateParentStatus(parent);
            }
        }
    }

    private int Analyze(string code, string filePath, bool clearFirst)
    {
        int findingsCount = 0;
        if (string.IsNullOrWhiteSpace(code)) return 0;
        if (clearFirst)
        {
            Vulnerabilities.Clear();
            ClearHighlights();
        }

        try
        {
            var ws = new WorkspaceManager();
            ws.LoadSourceCode(code);
            string fileName = Path.GetFileName(filePath);

            foreach (var tree in ws.SyntaxTrees)
            {
                // Syntax errors
                foreach (var err in tree.GetDiagnostics()
                    .Where(d => d.Severity == Microsoft.CodeAnalysis.DiagnosticSeverity.Error))
                {
                    findingsCount++;
                    var line = err.Location.GetLineSpan().StartLinePosition.Line + 1;
                    Add($"Error de Sintaxis: {err.GetMessage()}", $"Línea {line}", fileName, filePath, line,
                        "Corrija el error de sintaxis para un análisis completo.");
                }

                var cfg      = new CfgBuilder().Build(tree.GetRoot());
                var taint    = new TaintAnalysis();
                new FixedPointEngine<TaintState>(cfg, taint).Run();

                foreach (var v in taint.Vulnerabilities)
                {
                    findingsCount++;
                    Add(v.Message, v.Location, fileName, filePath, v.LineNumber, v.Suggestion);
                }

                // Code Quality Analysis
                var quality = new QualityAnalyzer();
                quality.Visit(tree.GetRoot());
                foreach (var q in quality.Issues)
                {
                    findingsCount++;
                    Add(q.Message, q.Location, fileName, filePath, q.LineNumber, q.Suggestion);
                }
            }

            if (filePath == (_currentFilePath ?? "Edición en vivo"))
            {
                ClearHighlights();
                foreach (var v in Vulnerabilities.Where(x => x.FilePath == fileName))
                {
                    if (v.LineNumber > 0) HighlightLine(v.LineNumber);
                }
            }
        }
        catch (Exception ex)
        {
            StatusText.Text = $"Error en {Path.GetFileName(filePath)}: {ex.Message}";
        }
        return findingsCount;
    }

    private void Add(string msg, string loc, string fileName, string fullPath, int line, string sug)
    {
        Vulnerabilities.Add(new VulnerabilityItem
            { Message = msg, Location = loc, FilePath = fileName, FullFilePath = fullPath, LineNumber = line, Suggestion = sug });
    }

    // ── AI Button ─────────────────────────────────────────────────────────────
    private async void AskAIButton_Click(object sender, RoutedEventArgs e)
    {
        if (sender is not System.Windows.Controls.Button btn || btn.Tag is not VulnerabilityItem item) return;

        _debounce.Stop();
        _busy = true;
        btn.IsEnabled = false;
        btn.Content   = "⏳ Analizando...";
        item.AiResponse = "Consultando IA...";
        StatusText.Text = "🤖 Consultando IA...";

        try
        {
            var ai = new Klarity.Core.AI.HttpAIService();
            item.AiResponse = await ai.AnalyzeVulnerabilityAsync(item.Message, GetCode());
            StatusText.Text = "✅ Respuesta recibida.";
        }
        catch (Exception ex)
        {
            item.AiResponse = $"### ❌ Error\n{ex.Message}";
            StatusText.Text = "Error al consultar la IA.";
        }
        finally
        {
            btn.IsEnabled = true;
            btn.Content   = "✨ Preguntar al Asistente IA";
            _busy = false;
        }
    }

    // ── Editor Helpers ────────────────────────────────────────────────────────
    private string GetCode()
    {
        var textRange = new System.Windows.Documents.TextRange(CodeEditor.Document.ContentStart, CodeEditor.Document.ContentEnd);
        // Clean up Windows/WPF specific RichTextBox hidden \r\n endings that offset paragraphs.
        return textRange.Text.Replace("\r\n", "\n").TrimEnd();
    }

    private void SetCode(string code)
    {
        _busy = true;
        var doc = new FlowDocument { 
            PageWidth = 3000,
            PagePadding = new Thickness(0)
        };
        foreach (var line in code.Split(["\r\n", "\r", "\n"], StringSplitOptions.None))
        {
            var p = new Paragraph { 
                Margin = new Thickness(0), 
                LineHeight = 18,
                LineStackingStrategy = LineStackingStrategy.BlockLineHeight
            };
            ApplySyntaxColoring(line, p);
            doc.Blocks.Add(p);
        }
        CodeEditor.Document = doc;
        UpdateLineNumbers();
        _busy = false;
    }

    private void UpdateLineNumbers()
    {
        int n = GetCode().Split('\n').Length;
        // The first visual line might be empty but valid. Minimum 1 line.
        n = Math.Max(1, n);

        var sb = new System.Text.StringBuilder();
        for (int i = 1; i <= n; i++)
        {
            sb.AppendLine(i.ToString());
        }
        
        var newText = sb.ToString();
        if (LineNumbers.Text != newText)
        {
            LineNumbers.Text = newText;
        }
    }

    private Block? GetBlock(int oneBased)
        => (oneBased >= 1 && oneBased <= CodeEditor.Document.Blocks.Count)
           ? CodeEditor.Document.Blocks.ElementAt(oneBased - 1) : null;

    private void HighlightLine(int line)
    {
        if (GetBlock(line) is { } b)
            b.Background = new SolidColorBrush(System.Windows.Media.Color.FromArgb(50, 255, 82, 82));
    }

    private void ClearHighlights()
    {
        foreach (var b in CodeEditor.Document.Blocks)
            b.Background = System.Windows.Media.Brushes.Transparent;
    }

    // ── Syntax Coloring ───────────────────────────────────────────────────────
    private static readonly HashSet<string> Keywords = new(
    [
        "using","namespace","class","public","private","protected","internal",
        "static","void","string","int","bool","var","new","return","if","else",
        "for","foreach","while","in","try","catch","throw","async","await",
        "true","false","null","this","base","readonly","const","override"
    ]);

    private static void ApplySyntaxColoring(string line, Paragraph p)
    {
        var ci = line.IndexOf("//", StringComparison.Ordinal);
        var code    = ci >= 0 ? line[..ci] : line;
        var comment = ci >= 0 ? line[ci..] : null;

        foreach (var part in Regex.Split(code, @"(""(?:[^""\\]|\\.)*"")"))
        {
            if (part.StartsWith('"')) { p.Inlines.Add(Colored(part, "#CE9178")); continue; }
            foreach (var tok in Regex.Split(part, @"(\b\w+\b)"))
                p.Inlines.Add(Keywords.Contains(tok) ? Colored(tok, "#569CD6") : new Run(tok));
        }
        if (comment != null) p.Inlines.Add(Colored(comment, "#6A9955"));
    }

    private static Run Colored(string text, string hex)
        => new(text) { Foreground = new SolidColorBrush((System.Windows.Media.Color)System.Windows.Media.ColorConverter.ConvertFromString(hex)) };

    // ── Window Chrome ─────────────────────────────────────────────────────────
    private void CloseButton_Click(object s, RoutedEventArgs e)    => Close();
    private void MinimizeButton_Click(object s, RoutedEventArgs e) => WindowState = WindowState.Minimized;
    private void MaximizeButton_Click(object s, RoutedEventArgs e) =>
        WindowState = WindowState == WindowState.Maximized ? WindowState.Normal : WindowState.Maximized;
    private void TitleBar_MouseDown(object s, System.Windows.Input.MouseButtonEventArgs e)
    { if (e.ChangedButton == System.Windows.Input.MouseButton.Left) DragMove(); }
}