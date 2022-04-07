using System;
using System.IO;
using REghZy.Streams;

namespace FileStateCapture {
    public readonly struct FileState {
        private readonly string filePath;
        private readonly long size;

        // these are UTC times
        private readonly DateTime timeCreated;
        private readonly DateTime timeModified;

        public string FilePath => this.filePath;

        public long Size => this.size;

        public DateTime TimeCreated => this.timeCreated;

        public DateTime TimeModified => this.timeModified;

        public FileState(string filePath, long size, DateTime created, DateTime modified) {
            this.filePath = filePath;
            this.size = size;
            this.timeCreated = created;
            this.timeModified = modified;
        }

        public FileState(string filePath) {
            if (!File.Exists(filePath)) {
                throw new FileNotFoundException("File does not exist", filePath);
            }

            this.filePath = filePath;
            FileInfo info = new FileInfo(filePath);
            this.size = info.Length;
            this.timeCreated = info.CreationTimeUtc;
            this.timeModified = info.LastAccessTimeUtc;
        }

        public FileState(string directory, IDataInput input) {
            string name = input.ReadStringUTF16(input.ReadByte());
            this.filePath = directory == null ? name : Path.Combine(directory, name);
            this.size = input.ReadLong();
            this.timeCreated = new DateTime(input.ReadLong());
            this.timeModified = new DateTime(input.ReadLong());
        }

        public void Write(IDataOutput output) {
            string fileName = Path.GetFileName(this.filePath);
            output.WriteByte((byte) fileName.Length);
            output.WriteStringUTF16(fileName);
            output.WriteLong(this.size);
            output.WriteLong(this.timeCreated.Ticks);
            output.WriteLong(this.timeModified.Ticks);
        }

        public override string ToString() {
            return $"DirectoryState({this.filePath}, {this.size} bytes)";
        }
    }
}