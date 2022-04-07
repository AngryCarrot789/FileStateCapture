using System;
using System.IO;
using System.Threading;
using REghZy.Streams;

namespace FileStateCapture {
    class Program {
        public static string status = "Inactive";

        static void Main(string[] args) {
            Console.WriteLine("Input a target directory (to compile)");
            string directory = Console.ReadLine();

            Console.WriteLine("Input the output file path (to save the compiled directory states to)");
            string outputFile = Console.ReadLine();

            if (!Directory.Exists(directory)) {
                exit($"The target directory to compile ({directory}) does not exist");
                return;
            }

            if (File.Exists(outputFile)) {
                Console.WriteLine($"The output file already exists ({outputFile}). Do you want to overwrite it? (Y/N)");
                int read = Console.Read();
                if ((char) read != 'Y' && (char) read != 'y') {
                    exit("Cancelling compilation");
                    return;
                }
            }

            status = "Compiling...";

            new Thread(() => {
                while (true) {
                    Thread.Sleep(1000);
                    if (PrintActivity()) {
                        return;
                    }
                }
            }).Start();

            DirectoryState state = new DirectoryState(directory);

            status = "Writing...";

            using (FileStream stream = File.OpenWrite(outputFile)) {
                DataOutputStream output = new DataOutputStream(stream, SeekOrigin.Begin);
                state.Write(output);
                output.Flush();
            }

            status = null;
            exit("Finished writing!");

            long size = state.Size;
            exit($"Total size: {size} bytes ({Math.Round((double) size / 1000000000, MidpointRounding.ToZero)})~ gb");
        }

        public static void exit(string message) {
            Console.WriteLine(message);
            Console.WriteLine("Press any key to close");
            Console.Read();
        }

        public static bool PrintActivity() {
            if (status == null) {
                return true;
            }

            Console.Write(".");
            return false;
        }
    }
}