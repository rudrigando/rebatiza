using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

namespace Rebatiza
{
    public class Program
    {
        static void Main(string[] args)
        {
            Console.OutputEncoding = System.Text.Encoding.UTF8;

            var logger = new ConsoleLogger();

            int exitCode = 0; // 0=ok, 1=erros operacionais, 2=exceção inesperada
            Messages? msg = null;
            RunResult? result = null;

            try
            {
                //Escolha de idioma
                Console.Write("Select language / Selecione o idioma [1=English, 2=Português]: ");
                var langChoice = Console.ReadLine()?.Trim();
                msg = langChoice == "1" ? Messages.EnUs : Messages.PtBr;

                //Modo de execução (argumento ou prompt)
                bool dryRun = args.Any(a => a.Equals("--dry-run", StringComparison.OrdinalIgnoreCase) || a.Equals("-n", StringComparison.OrdinalIgnoreCase));
                if (!dryRun)
                {
                    Console.Write(msg.ModePrompt);
                    var modeChoice = Console.ReadLine()?.Trim();
                    dryRun = modeChoice == "2";
                }
                logger.Info(dryRun ? msg.DryRunOn : msg.LiveRunOn);

                var config = ReadConfig(logger, msg);
                if (config is null)
                {
                    exitCode = 1; // entrada inválida
                    return;
                }

                var ignoredFolders = new HashSet<string>(StringComparer.OrdinalIgnoreCase)
                { ".git", "bin", "obj", ".vs", ".idea", ".vscode" };

                var binaryExtensions = new HashSet<string>(StringComparer.OrdinalIgnoreCase)
                { ".dll", ".exe", ".png", ".jpg", ".jpeg", ".gif", ".zip", ".ico", ".pdb", ".db", ".woff", ".woff2", ".eot", ".pdf", ".7z", ".tar", ".gz" };

                var scanner = new FileSystemScanner(ignoredFolders, binaryExtensions, logger);
                var (files, directories) = scanner.Scan(config.RootPath);

                if (files.Count == 0 && directories.Count == 0)
                {
                    logger.Warn(msg.NothingToProcess);
                    exitCode = 1; // nada a fazer
                    return;
                }

                logger.Info($"\n{string.Format(msg.WillProcess, files.Count, directories.Count)}");
                logger.Info(msg.Starting + "\n");

                var progress = new ProgressBar(totalSteps: files.Count + directories.Count, logger: logger);
                var engine = new RenameReplaceEngine(logger, msg, dryRun);

                result = new RunResult();

                // 1) Conteúdo + renomear arquivos
                foreach (var file in files)
                {
                    try
                    {
                        var fileResult = engine.ProcessFile(file, config.OldText, config.NewText);
                        result.Merge(fileResult);
                    }
                    catch (Exception ex)
                    {
                        result.Errors.Add(new ErrorDetail(msg.FileLabel, file, ex));
                        logger.Error(string.Format(msg.FileProcessError, scanner.ToRelative(config.RootPath, file)));
                        logger.Dim(ex.Message);
                    }
                    progress.Step();
                }

                // 2) Renomear pastas
                foreach (var dir in directories.OrderByDescending(d => d.Length))
                {
                    try
                    {
                        var dirResult = engine.ProcessDirectory(dir, config.OldText, config.NewText);
                        result.Merge(dirResult);
                    }
                    catch (Exception ex)
                    {
                        result.Errors.Add(new ErrorDetail(msg.FolderLabel, dir, ex));
                        logger.Error(string.Format(msg.DirRenameError, scanner.ToRelative(config.RootPath, dir)));
                        logger.Dim(ex.Message);
                    }
                    progress.Step();
                }

                progress.Complete();

                // Resumo
                logger.Success("\n" + (dryRun ? msg.FinishedDry : msg.Finished));
                logger.Info($"{msg.ModeLabel} {(dryRun ? msg.DryRunWord : msg.LiveRunWord)}");
                logger.Info($"{msg.FilesUpdated} {result.FilesContentUpdated}");
                logger.Info($"{msg.FilesRenamed} {result.FilesRenamed}");
                logger.Info($"{msg.DirsRenamed} {result.DirectoriesRenamed}");

                if (result.Errors.Count > 0)
                {
                    logger.Warn($"\n{string.Format(msg.ErrorsOccurred, result.Errors.Count)}");
                    logger.Dim(msg.ErrorDetails);
                    foreach (var e in result.Errors.Take(50))
                    {
                        logger.Dim($"- [{e.Kind}] {e.Path}");
                        logger.Dim($"  > {e.Exception.GetType().Name}: {e.Exception.Message}");
                    }
                    if (result.Errors.Count > 50)
                        logger.Dim(string.Format(msg.MoreErrorsOmitted, result.Errors.Count - 50));

                    exitCode = 1; // houve erros
                }
            }
            catch (Exception ex)
            {
                // Erro inesperado
                Console.ForegroundColor = ConsoleColor.Red;
                Console.WriteLine($"[FATAL] {ex.GetType().Name}: {ex.Message}");
                Console.ResetColor();
                exitCode = 2;
            }
            finally
            {
                // Mensagem final + pausa para não fechar janela
                var pressMsg = exitCode == 0
                    ? (msg?.PressKeyDone ?? Messages.EnUs.PressKeyDone)
                    : (msg?.PressKeyError ?? Messages.EnUs.PressKeyError);

                Console.WriteLine();
                Console.WriteLine(pressMsg);
                try { Console.ReadKey(true); } catch { }
                Environment.ExitCode = exitCode;
            }
        }

        //Config e Input

        private static AppConfig? ReadConfig(ConsoleLogger logger, Messages msg)
        {
            logger.Title(msg.Title);

            Console.Write(msg.AskRoot);
            var root = Console.ReadLine()?.Trim() ?? "";
            Console.Write(msg.AskOld);
            var oldText = Console.ReadLine()?.Trim() ?? "";
            Console.Write(msg.AskNew);
            var newText = Console.ReadLine()?.Trim() ?? "";

            if (!Directory.Exists(root))
            {
                logger.Error(msg.DirNotFound);
                return null;
            }

            if (string.IsNullOrWhiteSpace(oldText) || string.IsNullOrWhiteSpace(newText))
            {
                logger.Error(msg.EmptyTexts);
                return null;
            }

            return new AppConfig(root, oldText, newText);
        }
    }

    //Idiomas

    public class Messages
    {
        // UI
        public string Title { get; init; } = "";
        public string AskRoot { get; init; } = "";
        public string AskOld { get; init; } = "";
        public string AskNew { get; init; } = "";

        // Mode
        public string ModePrompt { get; init; } = "";   
        public string DryRunOn { get; init; } = "";
        public string LiveRunOn { get; init; } = "";
        public string ModeLabel { get; init; } = "";
        public string DryRunWord { get; init; } = "";
        public string LiveRunWord { get; init; } = "";

        // Flow
        public string NothingToProcess { get; init; } = "";
        public string Starting { get; init; } = "";
        public string Finished { get; init; } = "";
        public string FinishedDry { get; init; } = "";
        public string WillProcess { get; init; } = ""; 

        // Summary & labels
        public string FilesUpdated { get; init; } = "";
        public string FilesRenamed { get; init; } = "";
        public string DirsRenamed { get; init; } = "";
        public string ErrorsOccurred { get; init; } = "";
        public string ErrorDetails { get; init; } = "";
        public string MoreErrorsOmitted { get; init; } = "";
        public string SkipExists { get; init; } = "";
        public string FileLabel { get; init; } = "";
        public string FolderLabel { get; init; } = "";

        // Errors
        public string DirNotFound { get; init; } = "";
        public string EmptyTexts { get; init; } = "";
        public string FileProcessError { get; init; } = "";
        public string DirRenameError { get; init; } = "";

        // Operational messages
        public string ContentUpdated { get; init; } = "";
        public string FileRenamed { get; init; } = "";
        public string FolderRenamed { get; init; } = ""; 

        // Dry-run variants
        public string WouldUpdateContent { get; init; } = "";
        public string WouldRenameFile { get; init; } = ""; 
        public string WouldRenameFolder { get; init; } = "";

        // Final prompts 
        public string PressKeyDone { get; init; } = "";
        public string PressKeyError { get; init; } = "";

        // PT-BR
        public static Messages PtBr => new Messages
        {
            Title = "=== Rebatiza (Renomear & Substituir) ===",
            AskRoot = "Pasta raiz (ex.: C:\\Projetos\\LeadIntegrator): ",
            AskOld = "Texto a substituir: ",
            AskNew = "Novo texto: ",

            ModePrompt = "Modo de execução [1=Ao vivo (altera), 2=Dry-run (simula)]: ",
            DryRunOn = "[DRY-RUN] Modo simulação ativado — nenhum arquivo será alterado.",
            LiveRunOn = "[LIVE] Modo ao vivo — alterações serão aplicadas.",
            ModeLabel = "Modo:",
            DryRunWord = "Dry-run (simulação)",
            LiveRunWord = "Ao vivo",

            NothingToProcess = "Nada para processar (verifique o caminho e filtros).",
            Starting = "Iniciando...",
            Finished = "✅ Concluído!",
            FinishedDry = "✅ Concluído (simulação). Nenhuma alteração foi aplicada.",
            WillProcess = "Serão processados {0} arquivo(s) e {1} pasta(s).",

            FilesUpdated = "Arquivos com conteúdo alterado:",
            FilesRenamed = "Arquivos renomeados:",
            DirsRenamed = "Pastas renomeadas:",
            ErrorsOccurred = "⚠️ Ocorreram {0} erro(s). O processamento CONTINUOU.",
            ErrorDetails = "Detalhes dos erros (o caminho completo ajuda a depurar):",
            MoreErrorsOmitted = "... (+{0} erro(s) oculto(s))",
            SkipExists = "Já existe, pulando:",
            FileLabel = "Arquivo",
            FolderLabel = "Pasta",

            DirNotFound = "Diretório não encontrado.",
            EmptyTexts = "Texto antigo e novo não podem ser vazios.",
            FileProcessError = "Erro ao processar arquivo: {0}",
            DirRenameError = "Erro ao renomear pasta: {0}",

            ContentUpdated = "✓ Conteúdo atualizado: {0}",
            FileRenamed = "📄 Arquivo renomeado: {0} → {1}",
            FolderRenamed = "📁 Pasta renomeada: {0} → {1}",

            WouldUpdateContent = "[DRY-RUN] Atualizaria conteúdo: {0}",
            WouldRenameFile = "[DRY-RUN] Renomearia arquivo: {0} → {1}",
            WouldRenameFolder = "[DRY-RUN] Renomearia pasta: {0} → {1}",

            PressKeyDone = "✅ Concluído sem erros. Pressione qualquer tecla para sair...",
            PressKeyError = "⚠️ Concluído com erros. Pressione qualquer tecla para sair..."
        };

        // EN-US
        public static Messages EnUs => new Messages
        {
            Title = "=== Rebatiza (Rename & Replace) ===",
            AskRoot = "Root folder (e.g., C:\\Projects\\LeadIntegrator): ",
            AskOld = "Text to replace: ",
            AskNew = "Replacement text: ",

            ModePrompt = "Run mode [1=Live (apply), 2=Dry-run (simulate)]: ",
            DryRunOn = "[DRY-RUN] Simulation mode ON — no files will be changed.",
            LiveRunOn = "[LIVE] Live mode — changes will be applied.",
            ModeLabel = "Mode:",
            DryRunWord = "Dry-run (simulation)",
            LiveRunWord = "Live",

            NothingToProcess = "Nothing to process (check path and filters).",
            Starting = "Starting...",
            Finished = "✅ Finished!",
            FinishedDry = "✅ Finished (simulation). No changes were applied.",
            WillProcess = "Will process {0} file(s) and {1} folder(s).",

            FilesUpdated = "Files with updated content:",
            FilesRenamed = "Files renamed:",
            DirsRenamed = "Folders renamed:",
            ErrorsOccurred = "⚠️ {0} error(s) occurred. Processing CONTINUED.",
            ErrorDetails = "Error details (full path helps debugging):",
            MoreErrorsOmitted = "... (+{0} more error(s) omitted)",
            SkipExists = "Already exists, skipping:",
            FileLabel = "File",
            FolderLabel = "Folder",

            DirNotFound = "Directory not found.",
            EmptyTexts = "Old and new text cannot be empty.",
            FileProcessError = "Error processing file: {0}",
            DirRenameError = "Error renaming folder: {0}",

            ContentUpdated = "✓ Content updated: {0}",
            FileRenamed = "📄 File renamed: {0} → {1}",
            FolderRenamed = "📁 Folder renamed: {0} → {1}",

            WouldUpdateContent = "[DRY-RUN] Would update content: {0}",
            WouldRenameFile = "[DRY-RUN] Would rename file: {0} → {1}",
            WouldRenameFolder = "[DRY-RUN] Would rename folder: {0} → {1}",

            PressKeyDone = "✅ Finished with no errors. Press any key to exit...",
            PressKeyError = "⚠️ Finished with errors. Press any key to exit..."
        };
    }

    //DTOs

    public record AppConfig(string RootPath, string OldText, string NewText);

    public class RunResult
    {
        public int FilesContentUpdated { get; set; }
        public int FilesRenamed { get; set; }
        public int DirectoriesRenamed { get; set; }
        public List<ErrorDetail> Errors { get; } = new();

        public void Merge(RunResult other)
        {
            FilesContentUpdated += other.FilesContentUpdated;
            FilesRenamed += other.FilesRenamed;
            DirectoriesRenamed += other.DirectoriesRenamed;
            Errors.AddRange(other.Errors);
        }
    }

    public record ErrorDetail(string Kind, string Path, Exception Exception);

    //Logging Colorido

    public class ConsoleLogger
    {
        public void Title(string msg) => WithColor(ConsoleColor.Cyan, () => Console.WriteLine(msg));
        public void Info(string msg) => WithColor(ConsoleColor.Gray, () => Console.WriteLine(msg));
        public void Success(string msg) => WithColor(ConsoleColor.Green, () => Console.WriteLine(msg));
        public void Warn(string msg) => WithColor(ConsoleColor.Yellow, () => Console.WriteLine(msg));
        public void Error(string msg) => WithColor(ConsoleColor.Red, () => Console.WriteLine(msg));
        public void Dim(string msg) => WithColor(ConsoleColor.DarkGray, () => Console.WriteLine(msg));
        public void Inline(string msg) => WithColor(ConsoleColor.DarkGray, () => Console.Write(msg));

        private static void WithColor(ConsoleColor color, Action act)
        {
            var prev = Console.ForegroundColor;
            Console.ForegroundColor = color;
            try { act(); } finally { Console.ForegroundColor = prev; }
        }
    }

    //Barra de Progresso

    public class ProgressBar
    {
        private readonly int _total;
        private int _current;
        private readonly ConsoleLogger _logger;
        private readonly int _width;

        public ProgressBar(int totalSteps, ConsoleLogger logger, int width = 40)
        {
            _total = Math.Max(1, totalSteps);
            _logger = logger;
            _width = width;
            Draw();
        }

        public void Step(int steps = 1)
        {
            _current = Math.Min(_total, _current + steps);
            Draw();
        }

        public void Complete()
        {
            _current = _total;
            Draw();
            Console.WriteLine();
        }

        private void Draw()
        {
            double pct = (double)_current / _total;
            int filled = (int)Math.Round(pct * _width);
            string bar = new string('█', filled) + new string('░', _width - filled);
            Console.CursorVisible = false;
            Console.Write('\r');
            _logger.Inline($"[{bar}] {(int)(pct * 100),3}%  ({_current}/{_total})");
            Console.CursorVisible = true;
        }
    }

    //Scanner

    public class FileSystemScanner
    {
        private readonly HashSet<string> _ignoredFolders;
        private readonly HashSet<string> _binaryExtensions;
        private readonly ConsoleLogger _logger;

        public FileSystemScanner(HashSet<string> ignoredFolders, HashSet<string> binaryExtensions, ConsoleLogger logger)
        {
            _ignoredFolders = ignoredFolders;
            _binaryExtensions = binaryExtensions;
            _logger = logger;
        }

        public (List<string> Files, List<string> Dirs) Scan(string root)
        {
            var allFiles = new List<string>();
            var allDirs = new List<string>();

            foreach (var dir in Directory.GetDirectories(root, "*", SearchOption.AllDirectories))
            {
                if (IsIgnored(dir)) continue;
                allDirs.Add(dir);
            }

            foreach (var file in Directory.GetFiles(root, "*", SearchOption.AllDirectories))
            {
                if (IsIgnored(file)) continue;
                if (IsBinary(file)) continue;
                allFiles.Add(file);
            }

            return (allFiles, allDirs);
        }

        public string ToRelative(string root, string path)
            => Path.GetRelativePath(root, path);

        private bool IsIgnored(string path)
        {
            var parts = path.Split(Path.DirectorySeparatorChar, Path.AltDirectorySeparatorChar);
            return parts.Any(p => _ignoredFolders.Contains(p));
        }

        private bool IsBinary(string file)
        {
            var ext = Path.GetExtension(file);
            return !string.IsNullOrEmpty(ext) && _binaryExtensions.Contains(ext);
        }
    }

    // Engine

    public class RenameReplaceEngine
    {
        private readonly ConsoleLogger _logger;
        private readonly Messages _msg;
        private readonly bool _dryRun;

        public RenameReplaceEngine(ConsoleLogger logger, Messages msg, bool dryRun)
        {
            _logger = logger;
            _msg = msg;
            _dryRun = dryRun;
        }

        public RunResult ProcessFile(string file, string oldText, string newText)
        {
            var res = new RunResult();

            // Substituir conteúdo
            string content = File.ReadAllText(file, Encoding.UTF8);
            if (content.Contains(oldText, StringComparison.Ordinal))
            {
                if (_dryRun)
                {
                    _logger.Warn(string.Format(_msg.WouldUpdateContent, Path.GetFileName(file)));
                }
                else
                {
                    content = content.Replace(oldText, newText, StringComparison.Ordinal);
                    File.WriteAllText(file, content, Encoding.UTF8);
                    _logger.Success(string.Format(_msg.ContentUpdated, Path.GetFileName(file)));
                }
                res.FilesContentUpdated++;
            }

            // Renomear arquivo se necessário
            string fileName = Path.GetFileName(file);
            if (fileName.Contains(oldText, StringComparison.Ordinal))
            {
                string newFileName = fileName.Replace(oldText, newText, StringComparison.Ordinal);
                string newFullPath = Path.Combine(Path.GetDirectoryName(file)!, newFileName);

                if (_dryRun)
                {
                    _logger.Warn(string.Format(_msg.WouldRenameFile, fileName, newFileName));
                    res.FilesRenamed++;
                }
                else
                {
                    if (!File.Exists(newFullPath))
                    {
                        File.Move(file, newFullPath);
                        _logger.Info(string.Format(_msg.FileRenamed, fileName, newFileName));
                        res.FilesRenamed++;
                    }
                    else
                    {
                        _logger.Warn($"{_msg.SkipExists} {newFileName}");
                    }
                }
            }

            return res;
        }

        public RunResult ProcessDirectory(string dir, string oldText, string newText)
        {
            var res = new RunResult();

            var folderName = Path.GetFileName(dir);
            if (!folderName.Contains(oldText, StringComparison.Ordinal)) return res;

            var parent = Path.GetDirectoryName(dir)!;
            var newFolderName = folderName.Replace(oldText, newText, StringComparison.Ordinal);
            var newPath = Path.Combine(parent, newFolderName);

            if (_dryRun)
            {
                _logger.Warn(string.Format(_msg.WouldRenameFolder, folderName, newFolderName));
                res.DirectoriesRenamed++;
            }
            else
            {
                if (!Directory.Exists(newPath))
                {
                    Directory.Move(dir, newPath);
                    _logger.Info(string.Format(_msg.FolderRenamed, folderName, newFolderName));
                    res.DirectoriesRenamed++;
                }
                else
                {
                    _logger.Warn($"{_msg.SkipExists} {newFolderName}");
                }
            }

            return res;
        }
    }

    // Helpers 

    public static class StringReplaceExtensions
    {
        //Replace com StringComparison
        public static string Replace(this string str, string oldValue, string newValue, StringComparison comparison)
        {
            if (string.IsNullOrEmpty(oldValue)) return str;
            var sb = new StringBuilder();
            int prevIndex = 0, index;
            while ((index = str.IndexOf(oldValue, prevIndex, comparison)) >= 0)
            {
                sb.Append(str, prevIndex, index - prevIndex);
                sb.Append(newValue);
                prevIndex = index + oldValue.Length;
            }
            sb.Append(str, prevIndex, str.Length - prevIndex);
            return sb.ToString();
        }
    }
}
