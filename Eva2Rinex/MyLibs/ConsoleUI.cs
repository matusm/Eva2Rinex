using System;
using System.Reflection;

namespace Eva2Rinex
{
    /// <summary>
    /// A library to implement a basic user interface for a console application. 
    /// </summary>
    public static class ConsoleUI
    {

        #region Fields

        private static int maxColumns;     // number of chars per line in console window
        private static bool verbatim;      // if false - no console output
        private static bool inProcedure;   // if inside procedure - true
        private static string procedureMessage;     // part of message which is common to start/end

        #endregion

        #region Properties

        public static bool Verbatim
        {
            get { return verbatim; }
            set { if (!inProcedure) verbatim = value; }
        }

        public static string Title => Assembly.GetEntryAssembly().GetName().Name;

        public static string Version => $"{Assembly.GetEntryAssembly().GetName().Version.Major}.{Assembly.GetEntryAssembly().GetName().Version.Minor}";

        public static string FullVersion => Assembly.GetEntryAssembly().GetName().Version.ToString();

        public static string Copyright => GetAssemblyAttribute<AssemblyCopyrightAttribute>(a => a.Copyright);

        public static string Company => GetAssemblyAttribute<AssemblyCompanyAttribute>(a => a.Company);

        public static string WelcomeMessage => $"This is {Title}, version {Version} by {Copyright} ({Company})";

        public static int MaxColumns
        {
            get
            {
                return maxColumns;
            }
            set
            {
                if (value <= 0)
                    maxColumns = 1;
                else
                    maxColumns = value;
            }
        }

        #endregion

        #region Ctor

        static ConsoleUI()
        {
            inProcedure = false;
            BeVerbatim();
        }

        #endregion

        #region Public methods

        public static void BeSilent()
        {
            Verbatim = false;
        }

        public static void BeVerbatim()
        {
            Verbatim = true;
            maxColumns = Console.WindowWidth;
        }

        /// <summary>
        /// If verbatim and not inside a procedure, writes a line of text to the console.
        /// </summary>
        /// <param name="obj">Text to be written.</param>
        public static void WriteLine(object obj)
        {
            if (inProcedure) return;
            if (verbatim)
            {
                string strObj = TruncateString(obj.ToString());
                Console.WriteLine(strObj);
            }
        }

        public static void Write(object obj)
        {
            if (inProcedure) return;
            if (verbatim)
            {
                string strObj = TruncateString(obj.ToString());
                Console.Write(strObj);
            }
        }

        /// <summary>
        /// If verbatim and not inside a procedure, writes an empty line to the console.
        /// </summary>
        public static void WriteLine()
        {
            WriteLine("");
        }

        /// <summary>
        /// Notifies the user about the start of a (lengthy) operation.
        /// </summary>
        /// <param name="message">Name of that very operation.</param>
        public static void StartOperation(string message)
        {
            if (inProcedure) return;
            procedureMessage = message.Trim();
            Write($"{procedureMessage} ...");
            inProcedure = true;
        }

        /// <summary>
        /// Indicates to the user the start of file reading.
        /// </summary>
        /// <param name="fileName">File name.</param>
        public static void ReadingFile(string fileName)
        {
            StartOperation($"Reading file {fileName.Trim()}");
        }

        /// <summary>
        /// Indicates to the user the start of file writing.
        /// </summary>
        /// <param name="fileName">File name.</param>
        public static void WritingFile(string fileName)
        {
            StartOperation($"Writing file {fileName.Trim()}");
        }

        /// <summary>
        /// Indicates the successful end of operation. But only if inside an operation.
        /// </summary>
        public static void Done()
        {
            Done("");
        }

        /// <summary>
        /// Indicates the successful end of operation. But only if inside an operation.
        /// </summary>
        /// <param name="explanation">Additional information to user.</param>
        public static void Done(string explanation)
        {
            if (inProcedure == false) return;
            inProcedure = false;
            WriteLine($"\r{procedureMessage} - done. {explanation}");
        }

        /// <summary>
        /// Indicates an aborted operation.
        /// </summary>
        public static void Abort()
        {
            Abort("");
        }

        /// <summary>
        /// Indicates an aborted operation.
        /// </summary>
        /// <param name="explanation">Additional information to user.</param>
        public static void Abort(string explanation)
        {
            inProcedure = false;
            WriteLine($"\r{procedureMessage} - aborted! {explanation}");
        }

        /// <summary>
        /// Welcome message with assembly information.
        /// </summary>
        public static void Welcome()
        {
            WriteLine(WelcomeMessage);
        }

        /// <summary>
        /// Exits the program with an exit code.
        /// </summary>
        /// <param name="errorMessage">A message decribing the error.</param>
        /// <param name="errorCode">Exit code provided to the operating system.</param>
        public static void ErrorExit(string errorMessage, int errorCode)
        {
            if (errorMessage != "")
                WriteLine($"{errorMessage} (error code {errorCode})");
            Environment.Exit(errorCode);
        }

        /// <summary>
        /// Exits the program with an exit code.
        /// </summary>
        /// <param name="errorCode">Exit code provided to the operating system.</param>
        public static void ErrorExit(int errorCode)
        {
            ErrorExit("", errorCode);
        }

        /// <summary>
        /// Waits until keypressed.
        /// </summary>
        /// <param name="message">Text for information.</param>
        public static void WaitForKey(string message)
        {
            if (!string.IsNullOrEmpty(message)) WriteLine(message);
            Console.ReadKey();
        }

        /// <summary>
        /// Waits until keypressed. No info to user!
        /// </summary>
        public static void WaitForKey()
        {
            WaitForKey("");
        }
        #endregion

        #region Private methods
        /// <summary>
        /// Method to extract the assembly attributes.
        /// </summary>
        /// <typeparam name="T">A attribute.</typeparam>
        /// <param name="value">The value of the attribute.</param>
        /// <returns></returns>
        private static string GetAssemblyAttribute<T>(Func<T, string> value) where T : Attribute
        {
            T attribute = (T)Attribute.GetCustomAttribute(Assembly.GetEntryAssembly(), typeof(T));
            return value.Invoke(attribute);
        }

        /// <summary>
        /// Truncates a string so that it fits in a single line of the console.
        /// </summary>
        /// <param name="longString">The string to be truncated.</param>
        /// <returns></returns>
        private static string TruncateString(string longString)
        {
            if (string.IsNullOrEmpty(longString)) return longString;
            if (longString.Length <= maxColumns - 1) return longString;
            return longString.Substring(0, maxColumns - 4) + "...";
        }
        #endregion
    }

}

