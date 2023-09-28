using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.IO;

public class GitObject
{
    private string _cwd;

    [DisplayName("Current Working Directory")]
    public string Cwd {
        get { return _cwd; }
        set {
            var di = new DirectoryInfo(value);
            if (!di.Exists) di.Create();
            _cwd = value;
        }
    }

    public GitObject(string cwd) {
        this._cwd = cwd;
    }

    #region Visible Methods

    public void Init() {
        BaseCommand(_cwd, "init");
    }

    public void Add(string pathSpec) {
        BaseCommand(_cwd, $"add {pathSpec}");
    }

    public void Remove(string pathSpec) {
        BaseCommand(_cwd, $"rm {pathSpec}");
    }

    public IEnumerable<string> Diff(string file) {
        return BaseCommand(_cwd, "diff").Split('\n');
    }

    public void Commit(string message) {
        BaseCommand(_cwd, $"commit -m '{message}'");
    }

    public IEnumerable<string> ListBranches() {
        return BaseCommand(_cwd, "branch").Split('\n');
    }

    public void Branch(string operation, string branchName = "") {
        switch (operation.ToLower()){
            case "list":
                BaseCommand(_cwd, "branch");
                break;
            case "create":
                if (!string.IsNullOrEmpty(branchName)) {
                    BaseCommand(_cwd, $"branch {branchName}");
                }
                else{
                    throw new ArgumentException("Branch name is required for create operation.");
                }
                break;
            case "delete":
                if (!string.IsNullOrEmpty(branchName)) {
                    BaseCommand(_cwd, $"branch -d {branchName}");
                }
                else{
                    throw new ArgumentException("Branch name is required for delete operation.");
                }
                break;
            default:
                throw new ArgumentException($"Invalid operation: {operation}. Supported operations are 'list', 'create', and 'delete'.");
        }
    }

    #endregion

    private static string BaseCommand(string path, string gitArguments = "help") {
        var di = new DirectoryInfo(path);
        if (!di.Exists) di.Create();
        string command = $"cd '{path}'; git {gitArguments}";

        var process = new Process {

            StartInfo = new ProcessStartInfo {
                FileName = "powershell.exe",
                UseShellExecute = false,
                CreateNoWindow = true,
                RedirectStandardOutput = true,
                RedirectStandardError = true,
                Arguments = $"-NoProfile -ExecutionPolicy Bypass -Command {command}"

            }
        };

        process.Start();

        string output = process.StandardOutput.ReadToEnd();
        string errorOutput = process.StandardError.ReadToEnd();

        process.WaitForExit();
        if (!string.IsNullOrEmpty(errorOutput)) {
            // Assuming GrooperException is a custom exception you have defined.
            throw new Exception($"GitOutput: {errorOutput}");
        }

        return output;
    }
}
