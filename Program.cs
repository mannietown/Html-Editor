using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Threading;
using System.Collections;
using System.Runtime.Serialization.Formatters.Binary;
using System.IO;
using System.Windows.Forms;
using System.Runtime.InteropServices;
using System.Runtime.Serialization;
using System.Drawing;
using System.Diagnostics;


namespace Html_Text_Editor
{
    class HtmlTagsLibrary : HtmlFilehandler
    {
        readonly string LibraryName = "Html Tags Library";
        public static Dictionary<string, KeyValuePair<string, string>> Library = new Dictionary<string, KeyValuePair<string, string>>();

        static public SortedList LibraryHeadings = new SortedList();

        static int LibraryEditorLinesCount = 0;
        static public Characters[][] LibraryEditorLines;

        public static Characters[] GetTagToInsert(int left, int top, int topTags)
        {
            try
            {
                string CurrentLine = new string
                ((from c in LibraryEditorLines[topTags]
                  select c.Character).ToArray()
                 );
                string Tags = "";
                foreach (string key in (from v in Library.Values.ToArray() select v.Key))
                {
                    if (CurrentLine.Contains(key))
                    {
                        Tags = key;
                        break;
                    }
                }
                Characters[] TagsArray = new Characters[Tags.Length];
                for (int i = 0; i < Tags.Length; i++)
                {
                    TagsArray[i] = new Characters { Character = Tags[i], CharacterPosition = new Point(left + i, top) };
                }

                return TagsArray;
            }
            catch (IndexOutOfRangeException) { return new Characters[0]; }
            catch (NullReferenceException) { return new Characters[0]; };
        }

        public static void SetLibraryEditorLines(string[] library)
        {
            LibraryEditorLines = new Characters[library.Length][];
            for (int i = 0; i < LibraryEditorLines.Length; i++)
            {
                LibraryEditorLines[i] = new Characters[library[i].Length];
                for (int j = 0; j < library[i].Length; j++)
                {
                    LibraryEditorLines[i][j] = new Characters { Character = library[i][j], CharacterPosition = new Point(j, i) };
                }

            }

        }
        public HtmlTagsLibrary()
        {
            SetLibrary();
        }

        public string[] GetLibrary()
        {
            using (FileStream fs = new FileStream(Path.Combine(FilePath, LibraryName + ".txt"), FileMode.Open, FileAccess.Read))
            {
                StreamReader sr = new StreamReader(fs);
                string[] nRemoved = sr.ReadToEnd().Split('\n');
                string nRemovedToString = "";
                for (int i = 0; i < nRemoved.Length; i++)
                {
                    nRemovedToString += nRemoved[i];
                }
                return nRemovedToString.Split('\r');
            }
        }
        public static void GetHeadingsFromLibrary(string[] Library)
        {

            for (int i = 0; i < Library.Length; i++)
            {
                if (Library[i][0] == '!')
                {
                    LibraryHeadings.Add(i, Library[i]);
                }
            }
        }

        public void SetLibrary()
        {
            string[] library = GetLibrary();
            GetHeadingsFromLibrary(library);
            SetLibraryEditorLines(library);

            for (int i = 0; i < LibraryHeadings.Count; i++)
            {
                for (int j = int.Parse(LibraryHeadings.GetKey(i).ToString()); j < library.Length; j++)
                {
                    if (library[j] == LibraryHeadings.GetByIndex(i).ToString())
                    {
                        continue;
                    }
                    else if (library[j][0] != '!')
                    {
                        string firstIndexValue = TagsFromDescription(library[j])[0];
                        string secondIndexValue = TagsFromDescription(library[j])[1];
                        Library.Add(LibraryHeadings.GetByIndex(i).ToString() + "[" + j + "]}", new KeyValuePair<string, string>(firstIndexValue, secondIndexValue));
                    }
                    else if (library[j] != LibraryHeadings.GetByIndex(i).ToString() && library[j][0] == '!')
                    {
                        break;
                    }
                }
            }
        }

        public string[] TagsFromDescription(string StringValue)
        {
            //index 0 = tags
            //index 1 = Discription
            try
            {
                string[] tags = new string[2]
            {
                StringValue.Split('>')[0] + '>' + StringValue.Split('>')[1] + '>',
                StringValue.Split('>')[2]
            };
                return tags;
            }
            catch (IndexOutOfRangeException)
            {
                string[] tags = new string[2]
          {
                StringValue.Split('>')[0] + '>' + StringValue.Split('>')[1] + '>',
                String.Empty
          };
                if (tags[0].Contains(">>")) { tags[0] = tags[0].Remove(tags[0].Length - 1); }
                return tags;
            }
            catch (NullReferenceException)
            {
                string[] tags = new string[2]
   {
                StringValue.Split('>')[0] + '>' + StringValue.Split('>')[1] + '>',
                String.Empty
   };
                if (tags[0].Contains(">>")) { tags[0] = tags[0].Remove(tags[0].Length - 1); }
                return tags;
            }


        }
    }
    class HtmlFilehandler
    {
        [DllImport("User32.dll")]
        static extern int SetForegroundWindow(IntPtr Point);

        public static string FolderName = "Html Documents";
        public static string FileName = "";
        public static string FilePath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.Desktop), FolderName);
        public static string FileHtmlName = "";
        public static string SerealizedEditorPrefix = "SER";
        //static string StringToWrite = "";

        public static void SetFileName(string fileName)
        {
            FileName = fileName + ".txt";
            FileHtmlName = fileName + ".html";
        }

        public static string GetStringToWrite()
        {
            TextEditor.LinesWithText();
            string GetString = "";
            for (int i = 0; i < TextEditor.IntList[1]; i++)
            {
                for (int j = 0; j < TextEditor.NonEmptyCharCounter(i, TextEditor.EditorLines); j++)
                {
                    GetString += TextEditor.EditorLines[i][j].Character;
                }
            }
            return GetString;
        }

        public static void WriteToFile()
        {
            File.WriteAllText(Path.Combine(FilePath, FileName), GetStringToWrite());
        }

        public static void SaveHtml()
        {
            if (FileName == "")
            {
                UserInterFace.PrompToCreateFile();
                WriteToFile();
                SerializeEditorLines();
                File.WriteAllText(Path.Combine(FilePath, FileHtmlName), GetStringToWrite());
                LoadHtml();
            }
            else
            {
                WriteToFile();
                SerializeEditorLines();
                File.WriteAllText(Path.Combine(FilePath, FileHtmlName), GetStringToWrite());
                LoadHtml();
            }


        }

        public static void LoadHtml()
        {
            try
            {
                Process ConsoleWindow = Process.GetCurrentProcess();
                IntPtr CurrentWindow = Process.GetProcessesByName("chrome")
                                       .First(x => x.MainWindowHandle != IntPtr.Zero)
                                       .MainWindowHandle;

                SetForegroundWindow(CurrentWindow);
                SendKeys.SendWait("{F5}");
                SetForegroundWindow(ConsoleWindow.MainWindowHandle);
            }
            catch (InvalidOperationException)
            {
                Process.Start("chrome");
            }


        }

        public static void CreateFile(string fileName)
        {
            SetFileName(fileName);
            if (!Directory.Exists(FilePath))
            {
                Directory.CreateDirectory(FilePath);
            }
            using (File.Create(Path.Combine(FilePath, FileName))) ;
            using (File.Create(Path.Combine(FilePath, FileHtmlName))) ;
        }

        public static void SerializeEditorLines()
        {
            string fileToSerialize = FilePath + "\\" + SerealizedEditorPrefix + FileName;
            if (!Directory.Exists(FilePath))
            {
                Directory.CreateDirectory(FilePath);
            }
            if (!File.Exists(fileToSerialize))
            {
                using (File.Create(fileToSerialize)) ;
            }

            using (FileStream fs = new FileStream(fileToSerialize, FileMode.Open, FileAccess.Write))
            {
                BinaryFormatter bf = new BinaryFormatter();
                bf.Serialize(fs, InitializeField.EditorLines);
            }

        }

        public static Characters[][] DeserializeEditorLines(string Path)
        {
            BinaryFormatter bf = new BinaryFormatter();
            try
            {
                using (FileStream fs = new FileStream(Path, FileMode.Open, FileAccess.Read))
                {
                    return (Characters[][])bf.Deserialize(fs);
                }
            }
            catch (SerializationException) { return TextEditor.EditorLines; }

        }

        public static void SetEditorLines(string Path)
        {
            TextEditor.EditorLines = DeserializeEditorLines(Path);
        }

        public static void OpenHtmlFile()
        {
            Process ConsoleWindow = Process.GetCurrentProcess();
            Process.Start(Path.Combine(FilePath, FileHtmlName));
            SetForegroundWindow(ConsoleWindow.MainWindowHandle);
        }


    }

    //Houses properties and methods to do with typed characters
    [Serializable]
    public class Characters
    {
        public Point CharacterPosition { get; set; }
        public char Character { get; set; }

    }

    [Serializable]
    public class InitializeField
    {
        public const int DefaultLineCount = 10;
        public const int DefaultLineLength = 100;
        public static Characters[][] EditorLines;

        public InitializeField()
        {
            SetLinesToDefault();
            HtmlTagsLibrary TagsLibrary = new HtmlTagsLibrary();
        }

        public virtual void SetLinesToDefault(Characters[] Line = null, int top = 0, int LineLength = 0)
        {
            EditorLines = new Characters[DefaultLineCount][];
            for (int i = 0; i < DefaultLineCount; i++)
            {
                EditorLines[i] = new Characters[DefaultLineLength];
                for (int j = 0; j < DefaultLineLength; j++)
                {
                    EditorLines[i][j] = new Characters { Character = '\0', CharacterPosition = new Point(j, i) };
                }
            }
        }

    }

    public class TextEditor : InitializeField
    {
        public static int[] IntList = new int[2];
        public static void LinesWithText()
        {
            //0 index represents IndexOfLast
            //1 index represent LinesWithTextCount

            int LinesWithTextCount = 0;
            int IndexOfLastLineWithText = -1;
            try
            {
                for (int i = EditorLines.Length - 1; i > -1; i--)
                {
                    if (EditorLines[i].First().Character != '\0')
                    {
                        IntList[0] = IndexOfLastLineWithText = i;
                        IntList[1] = LinesWithTextCount = i + 1;
                        break;
                    }
                }
            }
            catch (NullReferenceException) { IntList[0] = -1; IntList[1] = 0; }

        }


        public static void AddLines(Characters[][] List, int top)
        {
            if (IntList[1] == List.Length - 2)
            {
                Array.Resize(ref EditorLines, List.Length + 2);

                for (int i = List.Length - 1; i > List.Length - 3; i--)
                {
                    EditorLines[i] = new Characters[DefaultLineLength];
                    EditorLines[i] = SetLineToDefault(List[i], i, DefaultLineLength);
                }
            }

        }

        public static void WriteToEditorLinesAtEnd(char Char, int left, int top)
        {
            Characters C = new Characters() { Character = Char, CharacterPosition = new Point(left, top) };
            try
            {
                EditorLines[top][left] = C;
                Console.Write(Char);
            }
            catch (IndexOutOfRangeException)
            {
                if (NonEmptyCharCounter(top + 1, EditorLines) == 0)
                {
                    ConsoleCommand.left = 0; ConsoleCommand.top++;
                    Console.SetCursorPosition(ConsoleCommand.left, ConsoleCommand.top);
                    EditorLines[ConsoleCommand.top][ConsoleCommand.left] = C;
                    Console.Write(Char);

                }
                else
                {
                    ConsoleCommand.left = 0; ConsoleCommand.top++;
                    Console.SetCursorPosition(ConsoleCommand.left, ConsoleCommand.top);
                    InsertCharWithinText(Char, ConsoleCommand.left, ConsoleCommand.top);
                    WriteRange(0, ConsoleCommand.top, EditorLines, NonEmptyCharCounter(ConsoleCommand.top, EditorLines));
                }
            }

            LinesWithText();

        }

        public static int NonEmptyCharCounter(int top, Characters[][] List)
        {
            int Count = 0;
            try
            {
                for (int i = 0; i < List[top].Length; i++)
                {
                    if (List[top][i].Character != '\0')
                    {
                        Count++;
                    }
                    else { break; }
                }
            }
            catch (IndexOutOfRangeException) { Count = 0; }
            catch (NullReferenceException) { Count = 0; }


            return Count;

        }

        public static Characters[] SetLineToDefault(Characters[] Line, int top, int LineLength)
        {
            Line = new Characters[LineLength];
            for (int i = 0; i < LineLength; i++)
            {
                Point P = new Point(i, top);
                Characters C = new Characters { Character = '\0', CharacterPosition = P };
                Line[i] = C;
            }

            return Line;
        }

        public static void DeleteChar(int left, int top)
        {
            if (NonEmptyCharCounter(top, EditorLines) == 0)
            {
                LinesWithText();
                for (int i = top; i < IntList[1] + 1; i++)
                {
                    for (int j = 0; j < DefaultLineLength; j++)
                    {
                        EditorLines[i][j] = new Characters { Character = EditorLines[i + 1][j].Character, CharacterPosition = EditorLines[i + 1][j].CharacterPosition };
                    }
                    IncreDecreReset(EditorLines, i, 0, DefaultLineLength, false, false, false, true, false);
                }

                for (int i = top; i < IntList[1] + 1; i++)
                {

                    if (NonEmptyCharCounter(i - 1, EditorLines) >= NonEmptyCharCounter(i, EditorLines))
                    {
                        WriteRange(0, i, EditorLines, NonEmptyCharCounter(i - 1, EditorLines));
                    }
                    else
                    {
                        WriteRange(0, i, EditorLines, NonEmptyCharCounter(i, EditorLines));
                    }

                }


            }
            else
            {
                Characters[][] LineSnippet = LineSnippets(left + 1, top);

                IncreDecreReset(LineSnippet, 1, 0, LineSnippet[1].Length, false, true, false, false, false);

                int Iterator = 0;
                for (int i = left; i < LineSnippet[1].Length + left; i++)
                {

                    EditorLines[top][i] = new Characters { Character = LineSnippet[1][Iterator].Character, CharacterPosition = LineSnippet[1][Iterator].CharacterPosition };
                    Iterator++;
                }

                EditorLines[top][left + LineSnippet[1].Length] = new Characters { Character = '\0', CharacterPosition = new Point(left + LineSnippet[1].Length, top) };


                for (int i = left; i < NonEmptyCharCounter(top, EditorLines) + 1; i++)
                {
                    Console.SetCursorPosition(EditorLines[top][i].CharacterPosition.X, top);
                    Console.Write(EditorLines[top][i].Character);
                }
            }

        }

        public static Characters[][] LineSnippets(int left, int top)
        {
            Characters[][] LineSnippet = new Characters[2][];

            for (int i = 0; i < LineSnippet.Length; i++)
            {
                if (i == 0) { LineSnippet[i] = SetLineToDefault(LineSnippet[i], i, left); }
                try
                {
                    if (i == 1) { LineSnippet[i] = SetLineToDefault(LineSnippet[i], i, NonEmptyCharCounter(top, EditorLines) - left); };
                }
                catch (ArithmeticException)
                {
                    LineSnippet[1] = new Characters[0];
                }


            }

            int BeginIndex = 0;
            int EndIndex = 0;

            for (int i = 0; i < LineSnippet.Length; i++)
            {

                int LineSnippetIterator = 0;
                if (i == 0) { EndIndex = left; }
                if (i == 1) { BeginIndex = left; EndIndex = NonEmptyCharCounter(top, EditorLines); }


                for (int j = BeginIndex; j < EndIndex; j++)
                {
                    LineSnippet[i][LineSnippetIterator] = EditorLines[top][j];
                    LineSnippetIterator++;
                }
            }

            return LineSnippet;
        }

        public static void IncreDecreReset(Characters[][] List, int top, int StartIndex, int EndIndex, bool IncreColumn, bool DecreColumn, bool IncreRow, bool DecreRow, bool Reset)
        {

            if (IncreColumn == true)
            {
                for (int i = StartIndex; i < EndIndex; i++)
                {
                    List[top][i].CharacterPosition = new Point(List[top][i].CharacterPosition.X + 1, List[top][i].CharacterPosition.Y);
                }
            }
            else if (DecreColumn == true)
            {
                for (int i = StartIndex; i < EndIndex; i++)
                {
                    List[top][i].CharacterPosition = new Point(List[top][i].CharacterPosition.X - 1, List[top][i].CharacterPosition.Y);
                }
            }

            if (IncreRow == true)
            {
                for (int i = StartIndex; i < EndIndex; i++)
                {
                    List[top][i].CharacterPosition = new Point(List[top][i].CharacterPosition.X, List[top][i].CharacterPosition.Y + 1);
                }
            }
            else if (DecreRow == true)
            {
                for (int i = StartIndex; i < EndIndex; i++)
                {
                    List[top][i].CharacterPosition = new Point(List[top][i].CharacterPosition.X, List[top][i].CharacterPosition.Y - 1);
                }
            }

            if (Reset == true)
            {
                int ResetIterator = 0;
                for (int i = StartIndex; i < EndIndex; i++)
                {
                    try
                    {
                        List[top][i].CharacterPosition = new Point(ResetIterator, List[top][i].CharacterPosition.Y);
                        ResetIterator++;
                    }

                    catch (NullReferenceException)
                    {
                        List[top] = SetLineToDefault(new Characters[DefaultLineLength], top, DefaultLineLength);
                    }

                }
            }
        }

        public static void InsertRange(Characters[] Line, Characters[][] List, int StartIndex, int top, out char OutOfRangeChar)
        {
            int LineIterator = 0;
            char CharOutOfRange = '\0';

            for (int i = StartIndex; i < StartIndex + Line.Length; i++)
            {
                try
                {
                    Characters Char = new Characters { Character = Line[LineIterator].Character, CharacterPosition = new Point(i, top) };
                    List[top][i] = Char;
                    LinesWithText();

                    LineIterator++;
                }
                catch (IndexOutOfRangeException)
                {
                    CharOutOfRange = Line[Line.Length - 1].Character;
                    LinesWithText();


                }

            }

            OutOfRangeChar = CharOutOfRange;

        }

        public static void InsertCharWithinText(char Char, int left, int top)
        {
            Characters[][] CharsToPush = LineSnippets(left, top);
            IncreDecreReset(CharsToPush, 1, 0, CharsToPush[1].Length, true, false, false, false, false);

            char CharOutOfRange;
            InsertRange(CharsToPush[1], EditorLines, left + 1, top, out CharOutOfRange);

            Characters CharacterToInsert = new Characters { Character = Char, CharacterPosition = new Point(left, top) };
            try
            {
                EditorLines[top][left] = CharacterToInsert;
            }
            catch (NullReferenceException)
            {

            }


            if (CharOutOfRange != '\0')
            {
                LinesWithText();
                InsertCharWithinText(CharOutOfRange, 0, top + 1);

                WriteRange(0, top + 1, EditorLines, NonEmptyCharCounter(top + 1, EditorLines));
            }
            LinesWithText();

        }

        public static bool WithinText(int left, int top)
        {
            if (left < NonEmptyCharCounter(top, EditorLines) && left != NonEmptyCharCounter(top, EditorLines))
            {
                return true;
            }
            else if (left == 0 && left == NonEmptyCharCounter(top, EditorLines))
            {
                return true;
            }
            else
            {
                return false;
            }
        }

        public static void WriteRange(int StartIndex, int top, Characters[][] List, int length)
        {
            for (int i = StartIndex; i < length; i++)
            {
                Console.SetCursorPosition(List[top][i].CharacterPosition.X, List[top][i].CharacterPosition.Y);
                Console.Write(List[top][i].Character);
            }
        }

        public static void BackSpace(int left, int top)
        {
            Characters[][] CharsToPull = LineSnippets(left, top);
            Characters[] CharsToPullNullExtension = new Characters[CharsToPull[1].Length + 1];

            if (CharsToPull[1].Length != 0)
            {
                for (int i = 0; i < CharsToPullNullExtension.Length; i++)
                {
                    if (i == CharsToPullNullExtension.Length - 1)
                    {
                        CharsToPullNullExtension[i] = new Characters { Character = '\0', CharacterPosition = new Point(CharsToPull[1][i - 1].CharacterPosition.X + 1, top) };

                    }
                    else { CharsToPullNullExtension[i] = CharsToPull[1][i]; }

                }
                CharsToPull[1] = CharsToPullNullExtension;

                IncreDecreReset(CharsToPull, 1, 0, CharsToPull[1].Length, false, true, false, false, false);
                char OutOfRangeChar = '\0';
                InsertRange(CharsToPullNullExtension, EditorLines, left - 1, top, out OutOfRangeChar);
            }
            else
            {
                EditorLines[top][left - 1] = new Characters { Character = '\0', CharacterPosition = new Point(left - 1, top) };

            }

            LinesWithText();

        }

        public static Characters[][] TruncateAndInsert(Characters[][] List, int top)
        {
            Characters[][] TruncatedLines = new Characters[EditorLines.Length - (top + 1)][];
            for (int i = 0; i < TruncatedLines.Length; i++)
            {
                TruncatedLines[i] = new Characters[DefaultLineLength];
            }
            Array.Copy(EditorLines, top + 1, TruncatedLines, 0, TruncatedLines.Length);

            for (int i = 0; i < TruncatedLines.Length; i++)
            {
                IncreDecreReset(TruncatedLines, i, 0, TruncatedLines[i].Length, false, false, true, false, false);
            }

            int TruncatedLinesIterator = 0;
            for (int i = top + 2; i < EditorLines.Length; i++)
            {
                //LinesWithText();
                AddLines(EditorLines, i);


                try
                {
                    EditorLines[i] = TruncatedLines[TruncatedLinesIterator];

                }
                catch (IndexOutOfRangeException)
                {
                    IncreDecreReset(EditorLines, i, 0, DefaultLineLength, false, false, false, false, true);
                }
                TruncatedLinesIterator++;
            }

            return TruncatedLines;
        }

        public static void InsertLines(Characters[][] List, int left, int top)
        {
            Characters[] LineToInsert = SetLineToDefault(new Characters[DefaultLineLength], top + 1, DefaultLineLength);
            EditorLines[top + 1] = LineToInsert;
            Characters[][] LineSnippet = LineSnippets(left, top);

            if (WithinText(left, top) == true && left != 0)
            {
                IncreDecreReset(LineSnippet, 1, 0, LineSnippet[1].Length, false, false, true, false, true);
                char CharOutOfRange;
                InsertRange(LineSnippet[1], EditorLines, 0, top + 1, out CharOutOfRange);
                for (int i = left; i < left + LineSnippet[1].Length; i++)
                {
                    EditorLines[top][i] = new Characters { Character = '\0', CharacterPosition = new Point(i, top) };

                }

                for (int i = top; i < EditorLines.Length; i++)
                {
                    if (i == top)
                    {
                        WriteRange(0, i, EditorLines, NonEmptyCharCounter(top, EditorLines) + NonEmptyCharCounter(top + 1, EditorLines));

                    }
                    else
                    {
                        if (NonEmptyCharCounter(i + 1, EditorLines) >= NonEmptyCharCounter(i, EditorLines))
                        {
                            WriteRange(0, i, EditorLines, NonEmptyCharCounter(i + 1, EditorLines));
                        }
                        else { WriteRange(0, i, EditorLines, NonEmptyCharCounter(i, EditorLines)); }

                    }
                }
                ConsoleCommand.left = 0; ConsoleCommand.top++;
                Console.SetCursorPosition(ConsoleCommand.left, ConsoleCommand.top);

            }
            else if (WithinText(left, top) == true && left == 0)
            {
                for (int i = 0; i < LineSnippet[1].Length; i++)
                {
                    EditorLines[top + 1][i] = new Characters { Character = LineSnippet[1][i].Character, CharacterPosition = LineSnippet[1][i].CharacterPosition };

                }

                IncreDecreReset(EditorLines, top + 1, 0, LineSnippet[1].Length, false, false, true, false, true);
                for (int i = 0; i < LineSnippet[1].Length; i++)
                {
                    EditorLines[top][i] = new Characters { Character = '\0', CharacterPosition = new Point(i, top) };

                }

                LinesWithText();
                for (int i = top; i < EditorLines.Length; i++)
                {
                    if (NonEmptyCharCounter(i + 1, EditorLines) >= NonEmptyCharCounter(i, EditorLines))
                    {
                        WriteRange(0, i, EditorLines, NonEmptyCharCounter(i + 1, EditorLines));
                    }
                    else { WriteRange(0, i, EditorLines, NonEmptyCharCounter(i, EditorLines)); }
                }
                ConsoleCommand.left = 0; ConsoleCommand.top++;
                Console.SetCursorPosition(ConsoleCommand.left, ConsoleCommand.top);

            }
            else
            {
                EditorLines[top + 1] = LineToInsert;

                for (int i = top + 1; i < EditorLines.Length; i++)
                {
                    if (NonEmptyCharCounter(i + 1, EditorLines) >= NonEmptyCharCounter(i, EditorLines))
                    {
                        WriteRange(0, i, EditorLines, NonEmptyCharCounter(i + 1, EditorLines));
                    }
                    else { WriteRange(0, i, EditorLines, NonEmptyCharCounter(i, EditorLines)); }
                }
                ConsoleCommand.left = 0; ConsoleCommand.top++;
                Console.SetCursorPosition(ConsoleCommand.left, ConsoleCommand.top);
            }

        }

    }

    public class ConsoleCommand
    {
        static public int left = 0, top = 0;
        static public int leftTags = 0, topTags = 0;
        enum Directions { left, top, bottom };
        static bool EnterPressed = false;
        static bool DeletePressed = false;

        static char CurrentChar = '\0';

        static void NavigateEditorLines(int left, int top, Characters[][] List)
        {
            try
            {
                CurrentChar = List[top][left].Character;
            }
            catch (NullReferenceException)
            { }
            catch (IndexOutOfRangeException) { }

        }


        static bool[] MoveNext(int left, int top, ConsoleKeyInfo UserInput)
        {
            bool[] DecisionBools = new bool[3];
            try
            {
                if (TextEditor.EditorLines[top][left + 1].Character == '\0')
                {
                    DecisionBools[(int)Directions.left] = false;
                }
                else { DecisionBools[(int)Directions.left] = true; }
            }
            catch (IndexOutOfRangeException) { DecisionBools[(int)Directions.left] = false; };


            try
            {
                if (TextEditor.NonEmptyCharCounter(top - 1, TextEditor.EditorLines) > 0)
                {
                    DecisionBools[(int)Directions.top] = true;
                }
                else { DecisionBools[(int)Directions.top] = false; }
            }
            catch (ArgumentNullException) { DecisionBools[(int)Directions.top] = false; }


            if (TextEditor.NonEmptyCharCounter(top + 1, TextEditor.EditorLines) > 0)
            {
                DecisionBools[(int)Directions.bottom] = true;
            }
            else { DecisionBools[(int)Directions.bottom] = false; }


            return DecisionBools;
        }

        static void ClearWrite(bool clear, bool Write, bool ClearWrite)
        {
            if (clear == true && Write == false)
            {
                Console.Clear();
            }
            else if (Write == true && clear == false)
            {
                for (int i = 0; i < TextEditor.IntList[1]; i++)
                {
                    TextEditor.WriteRange(0, i, TextEditor.EditorLines, TextEditor.NonEmptyCharCounter(i, TextEditor.EditorLines));
                }
            }
            else if (clear == false && Write == false && ClearWrite == true)
            {
                Console.Clear();
                TextEditor.LinesWithText();
                for (int i = 0; i < TextEditor.IntList[1]; i++)
                {
                    TextEditor.WriteRange(0, i, TextEditor.EditorLines, TextEditor.NonEmptyCharCounter(i, TextEditor.EditorLines));
                }
            }
        }

        /// <summary>Moves over text
        /// seperate from CommandPrompt, called within Command Prompt
        /// Move Right bool is for its control
        /// </summary>
        static bool loopMoveRight = false;
        static void MoveRight(ConsoleKeyInfo CKI, string ClosingTag, int left)
        {
            switch (CKI.Key)
            {
                case ConsoleKey.RightArrow:
                    ConsoleCommand.left++;
                    Console.SetCursorPosition(ConsoleCommand.left, top);
                    break;
                case ConsoleKey.Tab:
                    for (int i = 0; i < ClosingTag.Length; i++)
                    {
                        TextEditor.InsertCharWithinText(ClosingTag[i], left + i, top);
                    }
                    ClearWrite(false, false, true);
                    loopMoveRight = false;
                    break;
            }
        }
        public static bool TagsListActive = false;
        public static void CommandPrompt(ConsoleKeyInfo UserInput, Characters[][] List)
        {
            if (TagsListActive == false)
            {
                if (UserInput.Modifiers == ConsoleModifiers.Control)
                {
                    switch (UserInput.Key)
                    {
                        case ConsoleKey.R:
                            HtmlFilehandler.LoadHtml();
                            break;
                        case ConsoleKey.W:
                            HtmlFilehandler.SaveHtml();
                            break;
                        case ConsoleKey.O:
                            HtmlFilehandler.OpenHtmlFile();
                            break;

                        case ConsoleKey.N:
                            if (HtmlFilehandler.FileName == "")
                            {
                                UserInterFace.PrompToCreateFile();
                                Console.Clear();
                                Console.SetCursorPosition(left, top);
                            }
                            else
                            {
                                Console.Clear();
                                left = 0; top = 0;
                                Console.SetCursorPosition(left, top);
                                UserInterFace.PrompToCreateFile();
                                ClearWrite(false, false, true);

                                Console.SetCursorPosition(left, top);
                            }

                            break;
                    }
                }
                else if (UserInput.Modifiers == ConsoleModifiers.Alt)
                {
                    switch (UserInput.Key)
                    {
                        case ConsoleKey.R:
                            Console.Clear();
                            left = 0; top = 0;
                            Console.SetCursorPosition(left, top);
                            UserInterFace.PromptToLoadFile();
                            ClearWrite(false, false, true);

                            Console.SetCursorPosition(left, top);
                            break;
                        case ConsoleKey.T:
                            TagsListActive = true;
                            ClearWrite(true, false, false);
                            UserInterFace.PromptToSelectTags();
                            leftTags = Console.CursorLeft;
                            topTags = Console.CursorTop;
                            break;
                    }

                }
                else
                {
                    switch (UserInput.Key)
                    {
                        case ConsoleKey.Delete:
                            TextEditor.DeleteChar(left, top);
                            Console.SetCursorPosition(left, top);

                            break;
                        case ConsoleKey.Enter:
                            EnterPressed = true;
                            TextEditor.TruncateAndInsert(List, top);
                            TextEditor.InsertLines(List, left, top);
                            TextEditor.LinesWithText();
                            EnterPressed = false;
                            break;

                        case ConsoleKey.Backspace:
                            DeletePressed = true;
                            if (left != 0)
                            {
                                TextEditor.BackSpace(left, top);
                                TextEditor.WriteRange(left - 1, top, List, TextEditor.NonEmptyCharCounter(top, List) + 1);
                                left--;
                                Console.SetCursorPosition(left, top);

                            }

                            break;

                        case ConsoleKey.RightArrow:
                            if (MoveNext(left, top, UserInput)[0] == false && TextEditor.NonEmptyCharCounter(top, List) == left)
                            {
                                Console.SetCursorPosition(left, top);
                                NavigateEditorLines(left, top, List);

                            }
                            else if (MoveNext(left, top, UserInput)[0] == false && TextEditor.DefaultLineLength == left + 1)
                            {

                                Console.SetCursorPosition(left, top);
                                NavigateEditorLines(left, top, List);

                            }
                            else
                            {
                                left++;
                                Console.SetCursorPosition(left, top);
                                NavigateEditorLines(left, top, List);

                            }

                            break;
                        case ConsoleKey.LeftArrow:
                            if (left != 0)
                                left--;
                            Console.SetCursorPosition(left, top);
                            NavigateEditorLines(left, top, List);
                            break;
                        case ConsoleKey.UpArrow:
                            if (MoveNext(left, top, UserInput)[1] == true && top != 0 && List[top - 1][left].Character == '\0')
                            {
                                if (TextEditor.NonEmptyCharCounter(top - 1, List) == TextEditor.DefaultLineLength)
                                {
                                    left = TextEditor.NonEmptyCharCounter(top - 1, List) - 1;
                                }

                                else { left = TextEditor.NonEmptyCharCounter(top - 1, List); }

                                top--;
                                Console.SetCursorPosition(left, top);
                                NavigateEditorLines(left, top, List);
                            }
                            else if (MoveNext(left, top, UserInput)[1] == true && top != 0 && List[top - 1][left].Character != '\0')
                            {
                                top--;
                                Console.SetCursorPosition(left, top);
                                NavigateEditorLines(left, top, List);

                            }
                            else
                            {
                                if (top != 0)
                                {
                                    top--;
                                    Console.SetCursorPosition(left, top);
                                    NavigateEditorLines(left, top, List);
                                }

                                else
                                {
                                    Console.SetCursorPosition(left, top);
                                    NavigateEditorLines(left, top, List);

                                }

                            }


                            break;
                        case ConsoleKey.DownArrow:
                            if (top == List.Length - 1)
                            {
                                Console.SetCursorPosition(left, top);
                                NavigateEditorLines(left, top, List);

                            }
                            else
                            {
                                if (MoveNext(left, top, UserInput)[2] == false)
                                {
                                    top++;
                                    left = 0;
                                    Console.SetCursorPosition(left, top);
                                    NavigateEditorLines(left, top, List);

                                }
                                else if (MoveNext(left, top, UserInput)[2] == true && List[top + 1][left].Character != '\0')
                                {
                                    top++;
                                    Console.SetCursorPosition(left, top);
                                    NavigateEditorLines(left, top, List);


                                }
                                else if (MoveNext(left, top, UserInput)[2] == true && List[top + 1][left].Character == '\0')
                                {

                                    left = TextEditor.NonEmptyCharCounter(top + 1, List);
                                    top++;
                                    Console.SetCursorPosition(left, top);
                                    NavigateEditorLines(left, top, List);

                                }

                            }

                            break;
                        default:
                            if (TextEditor.WithinText(left, top) == true)
                            {
                                Console.Write(UserInput.KeyChar);
                                TextEditor.InsertCharWithinText(UserInput.KeyChar, left, top);
                                TextEditor.WriteRange(left + 1, top, List, TextEditor.NonEmptyCharCounter(top, List));
                                DeletePressed = false;
                                NavigateEditorLines(left, top, List);
                                left++;
                                Console.SetCursorPosition(left, top);
                                if (CurrentChar == '>')
                                {
                                    HtmlFilehandler.SaveHtml();
                                }
                            }
                            else
                            {
                                TextEditor.WriteToEditorLinesAtEnd(UserInput.KeyChar, left, top);
                                NavigateEditorLines(left, top, List);
                                left++;
                                Console.SetCursorPosition(left, top);
                                if (CurrentChar == '>')
                                {
                                    HtmlFilehandler.SaveHtml();
                                }
                                DeletePressed = false;
                            }

                            break;
                    }
                }
            }
            else
            {
                if (UserInput.Modifiers == ConsoleModifiers.Control)
                {
                    switch (UserInput.Key)
                    {
                        case ConsoleKey.E:
                            ClearWrite(false, false, true);
                            TagsListActive = false;
                            break;

                    }


                }
                else
                {
                    char OutOfRange = '\0';
                    Characters[] ToInsert = HtmlTagsLibrary.GetTagToInsert(left, top, topTags);
                    switch (UserInput.Key)
                    {
                        case ConsoleKey.Tab:
                            string Tags = new string((from v in ToInsert select v.Character).ToArray());
                            string openingTag = Tags.Split('>')[0] + '>';
                            string ClosingTag = Tags.Split('>')[1] + '>';
                            for (int i = 0; i < openingTag.Length; i++)
                            {
                                TextEditor.InsertCharWithinText(openingTag[i], left + i, top);
                            }
                            ClearWrite(false, false, true);
                            Console.SetCursorPosition(left, top);
                            loopMoveRight = true;
                            while (loopMoveRight == true)
                            {
                                ConsoleKeyInfo CKI = Console.ReadKey(true);
                                MoveRight(CKI, ClosingTag, left);
                                Console.SetCursorPosition(left, top);
                            }
                            TagsListActive = false;
                            break;
                        case ConsoleKey.Enter:

                            for (int i = 0; i < ToInsert.Length; i++)
                            {
                                TextEditor.InsertCharWithinText(ToInsert[i].Character, left + i, top);
                            }
                            ClearWrite(false, false, true);
                            Console.SetCursorPosition(left, top);
                            TagsListActive = false;
                            break;
                        case ConsoleKey.RightArrow:
                            leftTags++;
                            Console.SetCursorPosition(leftTags, topTags);
                            NavigateEditorLines(leftTags, topTags, HtmlTagsLibrary.LibraryEditorLines);
                            break;
                        case ConsoleKey.UpArrow:
                            topTags--;
                            Console.SetCursorPosition(leftTags, topTags);
                            NavigateEditorLines(leftTags, topTags, HtmlTagsLibrary.LibraryEditorLines);
                            break;
                        case ConsoleKey.LeftArrow:
                            if (leftTags != 0)
                            {
                                leftTags--;
                                Console.SetCursorPosition(leftTags, topTags);
                                NavigateEditorLines(leftTags, topTags, HtmlTagsLibrary.LibraryEditorLines);
                            }

                            break;
                        case ConsoleKey.DownArrow:
                            topTags++;
                            Console.SetCursorPosition(leftTags, topTags);
                            NavigateEditorLines(leftTags, topTags, HtmlTagsLibrary.LibraryEditorLines);
                            break;
                    }
                }
            }


        }
    }

    public class TextEditorThreads
    {
        ThreadStart LinesWithTextThread = new ThreadStart(TextEditor.LinesWithText);

        public TextEditorThreads()
        {
            LinesWithTextThread.Invoke();
        }


    }

    class GetUserInput
    {
        InitializeField I = new InitializeField();
        public GetUserInput()
        {

            while (true)
            {
                //TextEditorThreads T = new TextEditorThreads();
                ConsoleKeyInfo UserInput = Console.ReadKey(true);
                ConsoleCommand.CommandPrompt(UserInput, TextEditor.EditorLines);

            }

        }


    }

    public class UserInterFace
    {
        public static void PrompToCreateFile()
        {
            Console.Write("Enter File Name:");
            string FileName = Console.ReadLine();
            HtmlFilehandler.CreateFile(FileName);
        }

        public static void PromptToLoadFile()
        {
            Console.Write("Enter File To Load:");
            string FileName = Console.ReadLine() + ".txt";
            if (File.Exists(Path.Combine(HtmlFilehandler.FilePath, FileName)) && HtmlFilehandler.FileHtmlName != FileName)
            {
                HtmlFilehandler.SetEditorLines(Path.Combine(HtmlFilehandler.FilePath, HtmlFilehandler.SerealizedEditorPrefix + FileName));
                HtmlFilehandler.SetFileName(FileName.Remove(FileName.Length - 4, 4));
            }
            else
            {
                Console.WriteLine("FileNotFound");
            }
        }

        public static void PromptToSelectTags()
        {
            for (int i = 0; i < HtmlTagsLibrary.LibraryEditorLines.Length; i++)
            {
                TextEditor.WriteRange(0, i, HtmlTagsLibrary.LibraryEditorLines, HtmlTagsLibrary.LibraryEditorLines[i].Length);
            }

        }
    }

    class Program
    {
        static void Main(string[] args)
        {
            GetUserInput GUI = new GetUserInput();
            Console.ReadLine();
        }
    }
}
