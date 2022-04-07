using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using REghZy.Streams;

namespace FileStateCapture {
    public class DirectoryState {
        private readonly List<DirectoryState> directories;
        private readonly List<FileState> files;
        private readonly string directoryPath;
        private readonly DateTime timeCreated;
        private readonly DateTime timeModified;
        private long cachedSize = -1;

        public long Size => this.cachedSize == -1 ? this.cachedSize = CalculateSize() : this.cachedSize;

        public DateTime TimeCreated => this.timeCreated;

        public DateTime TimeModified => this.timeModified;

        public string DirectoryPath => this.directoryPath;

        public DirectoryInfo Info => new DirectoryInfo(this.directoryPath);

        public IEnumerable<DirectoryState> Directories {
            get => this.directories;
        }

        public IEnumerable<FileState> Files {
            get => this.files;
        }

        private byte DirectoryCountFlag {
            get {
                int count = this.directories.Count;
                if (count <= byte.MaxValue) {
                    return 1;
                }
                else if (count <= ushort.MaxValue) {
                    return 2;
                }
                else {
                    return 3;
                }
            }
        }

        private byte FileCountFlag {
            get {
                int count = this.files.Count;
                if (count <= byte.MaxValue) {
                    return 1;
                }
                else if (count <= ushort.MaxValue) {
                    return 2;
                }
                else {
                    return 3;
                }
            }
        }

        public DirectoryState(string directoryPath) {
            this.directoryPath = directoryPath;
            try {
                this.directories = new List<DirectoryState>(Directory.EnumerateDirectories(directoryPath).Select(p => new DirectoryState(p)));
            }
            catch (Exception e) {
                this.directories = new List<DirectoryState>();
            }

            try {
                this.files = new List<FileState>(Directory.EnumerateFiles(directoryPath).Select(p => new FileState(p)));
            }
            catch (Exception e) {
                this.files = new List<FileState>();
            }

            DirectoryInfo info = this.Info;
            this.timeCreated = info.CreationTimeUtc;
            this.timeModified = info.LastWriteTimeUtc;
        }

        /// <summary>
        /// Reads a directory state
        /// </summary>
        /// <param name="parent">The directory in which this new directory is stored in</param>
        /// <param name="input">The input to read from</param>
        public DirectoryState(string parent, IDataInput input) {
            this.timeCreated = new DateTime(input.ReadLong());
            this.timeModified = new DateTime(input.ReadLong());
            string name = input.ReadStringUTF16(input.ReadByte());
            // parent will be null if this is a root directory, e.g C:\
            string path = (this.directoryPath = (parent == null ? name : Path.Combine(parent, name)));

            int directoriesCount = 0;
            int filesCount = 0;
            byte dirFlag = input.ReadByte();
            switch (dirFlag) {
                case 1: directoriesCount = input.ReadByte(); break;
                case 2: directoriesCount = input.ReadUShort(); break;
                case 3: directoriesCount = input.ReadInt(); break;
            }

            List<DirectoryState> dirs = this.directories = new List<DirectoryState>(directoriesCount);
            for (int i = 0; i < directoriesCount; ++i) {
                dirs.Add(new DirectoryState(path, input));
            }

            byte fileFlag = input.ReadByte();
            switch (fileFlag) {
                case 1: filesCount = input.ReadByte(); break;
                case 2: filesCount = input.ReadUShort(); break;
                case 3: filesCount = input.ReadInt(); break;
            }

            List<FileState> flz = this.files = new List<FileState>(filesCount);
            for (int i = 0; i < filesCount; ++i) {
                flz.Add(new FileState(path, input));
            }
        }

        private long CalculateSize() {
            long size = 0L;

            foreach (DirectoryState directory in this.directories) {
                size += directory.Size;
            }

            foreach (FileState file in this.files) {
                size += file.Size;
            }

            return size;
        }

        public void Write(IDataOutput output) {
            output.WriteLong(this.timeCreated.Ticks);
            output.WriteLong(this.timeModified.Ticks);

            string name = FileUtils.GetDirectoryName(this.directoryPath);
            output.WriteByte((byte) name.Length);
            output.WriteStringUTF16(name);

            byte dirFlag = this.DirectoryCountFlag;
            output.WriteByte(dirFlag);
            switch (dirFlag) {
                case 1: output.WriteByte((byte) this.directories.Count); break;
                case 2: output.WriteUShort((ushort) this.directories.Count); break;
                case 3: output.WriteInt(this.directories.Count); break;
            }

            foreach(DirectoryState directory in this.directories) {
                directory.Write(output);
            }

            byte fileFlag = this.FileCountFlag;
            output.WriteByte(fileFlag);
            switch (fileFlag) {
                case 1: output.WriteByte((byte) this.files.Count); break;
                case 2: output.WriteUShort((ushort) this.files.Count); break;
                case 3: output.WriteInt(this.files.Count); break;
            }

            foreach(FileState file in this.files) {
                file.Write(output);
            }
        }

        public override string ToString() {
            if (this.cachedSize == -1) {
                return $"DirectoryState({this.directoryPath}, {this.directories.Count} dirs, {this.files.Count} files)";
            }
            else {
                return $"DirectoryState({this.directoryPath}, {this.directories.Count} dirs, {this.files.Count} files, {this.Size} bytes)";
            }
        }
    }
}