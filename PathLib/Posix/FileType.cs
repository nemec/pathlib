namespace PathLib.Posix
{
    public enum FileType
    {
        /// <summary>
        /// The path points to a directory.
        /// </summary>
        Directory,
        /// <summary>
        /// The path points to a character device. Generally
        /// a peripheral (e.g. USB) that communicates in single
        /// characters or bytes.
        /// </summary>
        CharacterDevice,
        /// <summary>
        /// The path points to a block device. Generally
        /// a peripheral (e.g. USB) that communicates in
        /// blocks of bytes at a time.
        /// </summary>
        BlockDevice,
        /// <summary>
        /// The path points to a regular file. It has no
        /// special properties like a link, socket, or pipe.
        /// </summary>
        RegularFile,
        /// <summary>
        /// The path points to a pipe.
        /// </summary>
        Fifo,
        /// <summary>
        /// The path points to a symbolic link. There is no
        /// guarantee that the link resolves to an actual file.
        /// </summary>
        SymbolicLink,
        /// <summary>
        /// The path points to a UNIX socket file.
        /// </summary>
        Socket,
        /// <summary>
        /// The path does not exist.
        /// </summary>
        DoesNotExist
    }
}