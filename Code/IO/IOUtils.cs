using Cupscale.UI;
using DdsFileTypePlus;
using ImageMagick;
using PaintDotNet;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Reflection.Emit;
using System.Text;
using System.Text.RegularExpressions;
using System.Windows.Forms;

namespace Cupscale
{
	internal class IOUtils
	{
		public static string[] compatibleExtensions = new string[] { ".png", ".jpg", ".jpeg", ".bmp", ".tga", ".webp", ".dds" };

		public static string GetAppDataDir()
		{
			string folderPath = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData);
			string text = Path.Combine(folderPath, "Cupscale");
			Directory.CreateDirectory(text);
			return text;
		}

		public static string GetExeDir()
		{
			return AppDomain.CurrentDomain.BaseDirectory;
		}

		public static string[] ReadLines(string path)
		{
			List<string> lines = new List<string>();
			using (var fs = new FileStream(path, FileMode.Open, FileAccess.Read, FileShare.ReadWrite, 0x1000, FileOptions.SequentialScan))
			using (var sr = new StreamReader(fs, Encoding.UTF8))
			{
				string line;
				while ((line = sr.ReadLine()) != null)
				{
					lines.Add(line);
				}
			}
			return lines.ToArray();
		}

		public static bool IsPathDirectory(string path)
		{
			if (path == null)
			{
				throw new ArgumentNullException("path");
			}
			path = path.Trim();
			if (Directory.Exists(path))
			{
				return true;
			}
			if (File.Exists(path))
			{
				return false;
			}
			if (new string[2]
			{
				"\\",
				"/"
			}.Any((string x) => path.EndsWith(x)))
			{
				return true;
			}
			return string.IsNullOrWhiteSpace(Path.GetExtension(path));
		}

		public static bool IsFileValid(string path)
		{
			if (path == null)
			{
				return false;
			}
			if (!File.Exists(path))
			{
				return false;
			}
			return true;
		}

		public static void Copy(string sourceDir, string targetDir, string wildcard = "*", bool move = false, bool onlyCompatibles = false, string removeFromName = "")
		{
			Logger.Log("Copying directory \"" + sourceDir + "\" to \"" + targetDir + "\" (Move: " + move + " - RemoveFromName: " + removeFromName + ")");
			Directory.CreateDirectory(targetDir);
			DirectoryInfo source = new DirectoryInfo(sourceDir);
			DirectoryInfo target = new DirectoryInfo(targetDir);
			CopyWork(source, target, wildcard, move, onlyCompatibles, removeFromName);
		}

		private static void CopyWork(DirectoryInfo source, DirectoryInfo target, string wildcard, bool move, bool onlyCompatibles, string removeFromName)
		{
			DirectoryInfo[] directories = source.GetDirectories();
			foreach (DirectoryInfo directoryInfo in directories)
			{
				CopyWork(directoryInfo, target.CreateSubdirectory(directoryInfo.Name), wildcard, move, onlyCompatibles, removeFromName);
			}
			FileInfo[] files = source.GetFiles(wildcard);
			foreach (FileInfo fileInfo in files)
			{
				if (onlyCompatibles && !compatibleExtensions.Contains(fileInfo.Extension.ToLower()))
					continue;

				string targetPath = Path.Combine(target.FullName, fileInfo.Name);

				if (move)
					fileInfo.MoveTo(targetPath);
				else
					fileInfo.CopyTo(targetPath, overwrite: true);
			}
		}

		public static void DeleteContentsOfDir(string path)
		{
			if (!Directory.Exists(path))
				return;
			Logger.Log("Clearing " + path);
			DirectoryInfo directoryInfo = new DirectoryInfo(path);
			FileInfo[] files = directoryInfo.GetFiles();
			foreach (FileInfo fileInfo in files)
			{
				fileInfo.Delete();
			}
			DirectoryInfo[] directories = directoryInfo.GetDirectories();
			foreach (DirectoryInfo directoryInfo2 in directories)
			{
				directoryInfo2.Delete(recursive: true);
			}
		}

		public static void DeleteFilesWithoutExt (string path, bool recursive)
		{
			DirectoryInfo d = new DirectoryInfo(path);
			FileInfo[] files = null;
			if (recursive)
				files = d.GetFiles("*", SearchOption.AllDirectories);
			else
				files = d.GetFiles("*", SearchOption.TopDirectoryOnly);
			foreach (FileInfo fileInfo in files)
			{
				if(string.IsNullOrWhiteSpace(fileInfo.Extension))
					fileInfo.Delete();
			}
		}

		public static void ReplaceInFilenamesDir(string dir, string textToFind, string textToReplace, bool recursive = true, string wildcard = "*")
		{
			int counter = 1;
			DirectoryInfo d = new DirectoryInfo(dir);
			FileInfo[] files = null;
			if (recursive)
				files = d.GetFiles(wildcard, SearchOption.AllDirectories);
			else
				files = d.GetFiles(wildcard, SearchOption.TopDirectoryOnly);
			foreach (FileInfo file in files)
			{
				ReplaceInFilename(file.FullName, textToFind, textToReplace);
				counter++;
			}
		}

		public static string ReplaceInFilename(string path, string textToFind, string textToReplace)
		{
			string ext = Path.GetExtension(path);
			string newFilename = Path.GetFileNameWithoutExtension(path).Replace(textToFind, textToReplace);
			string targetPath = Path.Combine(Path.GetDirectoryName(path), newFilename + ext);
			if (File.Exists(targetPath))
			{
				File.Delete(targetPath);
				//Logger.Log("Skipped " + path + " because a file with the target name already exists.");
			}
			File.Move(path, targetPath);
			return targetPath;
		}

		public static void RenameExtensions (string dir, string oldExt, string newExt, bool recursive = true, string wildcard = "*")
		{
			DirectoryInfo d = new DirectoryInfo(dir);
			FileInfo[] files = null;
			if (recursive)
				files = d.GetFiles(wildcard, SearchOption.AllDirectories);
			else
				files = d.GetFiles(wildcard, SearchOption.TopDirectoryOnly);

			string targetPath = "";
			foreach (FileInfo file in files)
			{
				if (file.Extension.Replace(".","") == oldExt.Replace(".", ""))
                {
					targetPath = Path.ChangeExtension(file.FullName, newExt);
					if (!File.Exists(targetPath))
						File.Delete(targetPath);
					File.Move(file.FullName, targetPath);
				}
			}
		}

		public static string RenameExtension (string filepath, string oldExt, string newExt)
		{
			string targetPath = filepath;
			FileInfo file = new FileInfo(filepath);
			if (file.Extension.Replace(".", "") == oldExt.Replace(".", ""))
			{
				targetPath = Path.ChangeExtension(file.FullName, newExt);
				if (!File.Exists(targetPath))
					File.Delete(targetPath);
				File.Move(file.FullName, targetPath);
			}
			return targetPath;
		}

		public static bool TryCopy (string source, string dest, bool overwrite)		// Copy with error handling. Returns false if failed
        {
            try
            {
				File.Copy(source, dest, overwrite);
			}
			catch (Exception e)
            {
				MessageBox.Show("Copy from \"" + source + "\" to \"" + dest + " (Overwrite: " + overwrite + ") failed: \n\n" + e.Message);
				return false;
            }
			return true;
        }

		public static int GetAmountOfFiles(string path, bool recursive, string wildcard = "*")
		{
            try
            {
				DirectoryInfo d = new DirectoryInfo(path);
				FileInfo[] files = null;
				if (recursive)
					files = d.GetFiles(wildcard, SearchOption.AllDirectories);
				else
					files = d.GetFiles(wildcard, SearchOption.TopDirectoryOnly);
				return files.Length;
			}
			catch
            {
				return 0;
            }
		}

		public static string[] GetCompatibleFiles(string path, bool recursive, string wildcard = "*")
		{
			DirectoryInfo d = new DirectoryInfo(path);
			string[] files = null;
			SearchOption rec = SearchOption.AllDirectories;
			SearchOption top = SearchOption.TopDirectoryOnly;
			StringComparison ignCase = StringComparison.OrdinalIgnoreCase;

			if (recursive)
				files = Directory.GetFiles(path, wildcard, rec).Where(file => compatibleExtensions.Any(x => file.EndsWith(x, ignCase))).ToArray();
			else
				files = Directory.GetFiles(path, wildcard, top).Where(file => compatibleExtensions.Any(x => file.EndsWith(x, ignCase))).ToArray();
			
			return files;
		}

		public static int GetAmountOfCompatibleFiles(string path, bool recursive, string wildcard = "*")
		{
			return GetCompatibleFiles(path, recursive, wildcard).Length;
		}

		public static int GetAmountOfCompatibleFiles(string[] files)
		{
			int num = 0;
			foreach (string file in files)
			{
				if (compatibleExtensions.Contains(Path.GetExtension(file).ToLower()))
					num++;
			}
			return num;
		}

		public static long GetDirSize(string path, bool recursive, string[] includedExtensions = null)
		{
			long size = 0;
			// Add file sizes.
			string[] files;
			StringComparison ignCase = StringComparison.OrdinalIgnoreCase;
			if (includedExtensions == null)
				files = Directory.GetFiles(path);
			else
				files = Directory.GetFiles(path).Where(file => includedExtensions.Any(x => file.EndsWith(x, ignCase))).ToArray();

			foreach (string file in files)
				size += new FileInfo(file).Length;

			if (!recursive)
				return size;

			// Add subdirectory sizes.
			DirectoryInfo[] dis = new DirectoryInfo(path).GetDirectories();
			foreach (DirectoryInfo di in dis)
				size += GetDirSize(di.FullName, true, includedExtensions);

			return size;
		}

		public static void TrimFilenames (string path, int trimAmont = 4, bool recursive = true, string wildcard = "*")
        {
			DirectoryInfo d = new DirectoryInfo(path);
			FileInfo[] files = null;
			if (recursive)
				files = d.GetFiles(wildcard, SearchOption.AllDirectories);
			else
				files = d.GetFiles(wildcard, SearchOption.TopDirectoryOnly);
			
			foreach(FileInfo file in files)
            {
				string newPath = file.FullName.Substring(0, file.FullName.Length - 4);
				file.MoveTo(newPath);
			}
		}

		public static int GetFilenameCounterLength(string file, string prefixToRemove = "")
		{
			string filenameNoExt = Path.GetFileNameWithoutExtension(file);
			if (!string.IsNullOrEmpty(prefixToRemove))
				filenameNoExt = filenameNoExt.Replace(prefixToRemove, "");
			string onlyNumbersFilename = Regex.Replace(filenameNoExt, "[^.0-9]", "");
			return onlyNumbersFilename.Length;
		}

		public static void DeleteIfExists (string path)
        {
			if (File.Exists(path))
				File.Delete(path);
        }
	}
}
